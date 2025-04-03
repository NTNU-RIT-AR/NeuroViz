#if XR_ARFOUNDATION && UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.ARSubsystems;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Editor
{
    class XREALImageLibraryBuildProcessor : IPreprocessBuildWithReport, ARBuildProcessor.IPreprocessBuild
    {
        public int callbackOrder => 0;

        static void BuildAssets()
        {
            var assets = AssetDatabase.FindAssets($"t:{nameof(XRReferenceImageLibrary)}");
            var libraries = assets
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>);

            var index = 0;
            foreach (var library in libraries)
            {
                index++;
                EditorUtility.DisplayProgressBar(
                    $"Compiling {nameof(XRReferenceImageLibrary)} ({index} of {assets.Length})",
                    $"{AssetDatabase.GetAssetPath(library)} ({library.count} image{(library.count == 1 ? "" : "s")})",
                    (float)index / assets.Length);

                try
                {
                    if (library.count != 0 || MarkerTrackingTool.IsMarkerTrackingInstalled())
                    {
                        library.SetDataForKey(XREALUtility.k_Identifier, library.BuildDB());
                    }
                }
                catch (InvalidWidthException e)
                {
                    throw new BuildFailedException($"{nameof(XRReferenceImage)} named '{e.ReferenceImage.name}' in library '{AssetDatabase.GetAssetPath(library)}' requires a non-zero width.");
                }
                catch (MissingTextureException e)
                {
                    throw new BuildFailedException($"{nameof(XRReferenceImage)} named '{e.ReferenceImage.name}' in library '{AssetDatabase.GetAssetPath(library)}' is missing a texture.");
                }
                catch (BadTexturePathException e)
                {
                    throw new BuildFailedException($"Could not resolve texture path for {nameof(XRReferenceImage)} named '{e.ReferenceImage.name}' in library '{AssetDatabase.GetAssetPath(library)}'.");
                }
                catch (LoadTextureException e)
                {
                    throw new BuildFailedException($"Could not load texture for {nameof(XRReferenceImage)} named '{e.ReferenceImage.name}' in library '{AssetDatabase.GetAssetPath(library)}'.");
                }
                catch (EncodeToPNGFailedException e)
                {
                    throw new BuildFailedException($"{nameof(XRReferenceImage)} named '{e.ReferenceImage.name}' in library '{AssetDatabase.GetAssetPath(library)}' could not be encoded to a PNG.");
                }
                catch (BuildDatabaseFailedException)
                {
                    throw new BuildFailedException($"The XREAL command line tool fail to build an image database for library '{AssetDatabase.GetAssetPath(library)}'.");
                }
            }
            EditorUtility.ClearProgressBar();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            if (!XREALUtility.IsLoaderActive())
                return;

            BuildAssets();
        }

        public void OnPreprocessBuild(PreprocessBuildEventArgs buildEventArgs)
        {
            if (buildEventArgs.activeLoadersForBuildTarget?.OfType<XREALXRLoader>().Any() == false)
                return;

            BuildAssets();
        }
    }

    static class ImageLibraryBuildProcessorExtensions
    {
#if UNITY_EDITOR_WIN
        static string platformName => "Windows";
#elif UNITY_EDITOR_OSX
        static string platformName => "MacOS";
#else
        static string platformName => throw new NotSupportedException();
#endif

#if UNITY_EDITOR_WIN
        static string extension => ".exe";
#else
        static string extension => "";
#endif
        internal static byte[] BuildDB(this XRReferenceImageLibrary library)
        {
            if (MarkerTrackingTool.IsMarkerTrackingInstalled())
            {
                var interMarkerPath = Path.Combine(MarkerTrackingTool.MARKER_RES_ROOT, "InterMarker.bin");
                return File.ReadAllBytes(interMarkerPath);
            }
            var tempDirectory = Path.Combine(Path.GetTempPath(), GetTempFileNameWithoutExtension());

            try
            {
                Directory.CreateDirectory(tempDirectory);
                var imageListPath = library.ToInputImageListPath(tempDirectory);
                var dbPath = Path.Combine(tempDirectory, $"{GetTempFileNameWithoutExtension()}.bin");
                var cliPath = Path.GetFullPath($"{XREALBuildProcessor.PackagePath}/Tools~/{platformName}/trackableImageTools{extension}");
#if UNITY_EDITOR_OSX
                Cli.Execute("/bin/chmod", $"+x \"{cliPath}\"");
#endif
                var (stdOut, stdErr, exitCode) = Cli.Execute(cliPath, new[]
                {
                    $"--images_config_file \"{imageListPath}\"",
                    $"--save_path \"{dbPath}\"",
                });

                const string pattern = @"Detection score:(?<detectionScore>[\d.]+), Tracking score:(?<trackingScore>[\d.]+), Total Score:(?<totalScore>[\d.]+)";
                int index = 0;
                foreach (var line in stdOut.Split('\n'))
                {
                    Match match = Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        float detectionScore = float.Parse(match.Groups["detectionScore"].Value);
                        float trackingScore = float.Parse(match.Groups["trackingScore"].Value);
                        float totalScore = float.Parse(match.Groups["totalScore"].Value);
                        if (detectionScore < 20 || trackingScore < 20 || totalScore < 50)
                        {
                            Debug.LogWarning($"{nameof(XRReferenceImage)} named '{library[index].name}' in library '{AssetDatabase.GetAssetPath(library)}' has insufficient feature points or high self-similarity. Please use images with well-distributed feature points and low degrees of self-similarity.");
                        }
                        index++;
                    }
                }

                if (!File.Exists(dbPath))
                    throw new BuildDatabaseFailedException();

                return File.ReadAllBytes(dbPath);
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        static string ToInputImageListPath(this XRReferenceImageLibrary library, string destinationDirectory)
        {
            var entries = new List<string>();
            foreach (var referenceImage in library)
            {
                if (!referenceImage.specifySize || referenceImage.width <= 0f)
                    throw new InvalidWidthException(referenceImage);

                if (referenceImage.textureGuid.Equals(Guid.Empty))
                    throw new MissingTextureException(referenceImage);

                entries.Add(referenceImage.ToImageDBEntry(destinationDirectory));
            }

            var path = Path.Combine(destinationDirectory, $"{GetTempFileNameWithoutExtension()}.txt");
            File.WriteAllText(path, string.Join("\n", entries));
            return path;
        }

        static string ToImageDBEntry(this XRReferenceImage referenceImage, string destinationDirectory)
        {
            return string.Join("|",
                referenceImage.guid.ToString("N"),
                referenceImage.CopyTo(destinationDirectory),
                referenceImage.width.ToString("G", CultureInfo.InvariantCulture));
        }

        static readonly string[] SupportedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        static string CopyTo(this XRReferenceImage referenceImage, string destinationDirectory)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(referenceImage.textureGuid.ToString("N"));
            if (string.IsNullOrEmpty(assetPath))
                throw new BadTexturePathException(referenceImage);

            var path = Path.Combine(destinationDirectory, GetTempFileNameWithoutExtension());
            var fileExtension = Path.GetExtension(assetPath).ToLower();
            if (SupportedExtensions.Contains(fileExtension))
            {
                path += fileExtension;
                File.Copy(assetPath, path);
            }
            else
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (texture == null)
                    throw new LoadTextureException(referenceImage);

                if (!texture.isReadable)
                {
                    RenderTexture renderTexture = new(texture.width, texture.height, 0);
                    Graphics.Blit(texture, renderTexture);
                    RenderTexture currentActiveRT = RenderTexture.active;
                    RenderTexture.active = renderTexture;
                    Texture2D readableTexture = new(texture.width, texture.height);
                    readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    RenderTexture.active = currentActiveRT;
                    texture = readableTexture;
                }
                var bytes = texture.EncodeToPNG();
                if (bytes == null)
                    throw new EncodeToPNGFailedException(referenceImage);

                path += ".png";
                File.WriteAllBytes(path, bytes);
            }
            return path;
        }

        static string GetTempFileNameWithoutExtension() => Guid.NewGuid().ToString("N");
    }

    internal abstract class ReferenceImageException : Exception
    {
        internal XRReferenceImage ReferenceImage { get; }

        internal ReferenceImageException(XRReferenceImage referenceImage) => ReferenceImage = referenceImage;
    }

    internal class InvalidWidthException : ReferenceImageException
    {
        internal InvalidWidthException(XRReferenceImage referenceImage) : base(referenceImage) { }
    }

    internal class MissingTextureException : ReferenceImageException
    {
        internal MissingTextureException(XRReferenceImage referenceImage) : base(referenceImage) { }
    }

    internal class BadTexturePathException : ReferenceImageException
    {
        internal BadTexturePathException(XRReferenceImage referenceImage) : base(referenceImage) { }
    }

    internal class LoadTextureException : ReferenceImageException
    {
        internal LoadTextureException(XRReferenceImage referenceImage) : base(referenceImage) { }
    }

    internal class EncodeToPNGFailedException : ReferenceImageException
    {
        internal EncodeToPNGFailedException(XRReferenceImage referenceImage) : base(referenceImage) { }
    }

    internal class BuildDatabaseFailedException : Exception
    {
    }
}
#endif
