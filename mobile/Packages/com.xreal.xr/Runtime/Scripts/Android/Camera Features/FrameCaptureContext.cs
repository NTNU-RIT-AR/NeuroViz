using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> A frame capture context. </summary>
    public class FrameCaptureContext
    {
        /// <summary> The blender. </summary>
        private BlenderBase m_Blender;
        /// <summary> The encoder. </summary>
        private IEncoder m_Encoder;
        /// <summary> Options for controlling the camera. </summary>
        private CameraParameters m_CameraParameters;
        /// <summary> The frame provider. </summary>
        private AbstractFrameProvider m_FrameProvider;
        /// <summary> The capture behaviour. </summary>
        private CaptureBehaviourBase m_CaptureBehaviour;
        /// <summary> True if is initialize, false if not. </summary>
        private bool m_IsInitialized = false;

        private List<IFrameConsumer> m_FrameConsumerList;

        /// <summary> Gets the preview texture. </summary>
        /// <value> The preview texture. </value>
        public Texture PreviewTexture
        {
            get
            {
                return m_Blender?.BlendTexture;
            }
        }

        /// <summary> Gets the behaviour. </summary>
        /// <returns> The behaviour. </returns>
        public CaptureBehaviourBase GetBehaviour()
        {
            return m_CaptureBehaviour;
        }

        /// <summary> Gets frame provider. </summary>
        /// <returns> The frame provider. </returns>
        public AbstractFrameProvider GetFrameProvider()
        {
            return m_FrameProvider;
        }

        /// <summary> Gets the blender. </summary>
        /// <returns> The blender. </returns>
        public BlenderBase GetBlender()
        {
            return m_Blender;
        }

        /// <summary> Request camera parameter. </summary>
        /// <returns> The CameraParameters. </returns>
        public CameraParameters RequestCameraParam()
        {
            return m_CameraParameters;
        }

        /// <summary> Gets the encoder. </summary>
        /// <returns> The encoder. </returns>
        public IEncoder GetEncoder()
        {
            return m_Encoder;
        }

        /// <summary> Constructor. </summary>
        public FrameCaptureContext() { }

        /// <summary> Starts capture mode. </summary>
        /// <param name="param"> The parameter.</param>
        public void StartCaptureMode(CameraParameters param, BlenderBase blender, AbstractFrameProvider provider)
        {
            if (m_IsInitialized)
            {
                this.Release();
                Debug.LogWarning("[CaptureContext] Capture context has been started already, release it and restart a new one.");
            }

            Debug.Log($"[CaptureContext] Create... camMode={param.camMode} blendMode={param.blendMode} side={param.captureSide}");
            if (m_CaptureBehaviour == null)
            {
                this.m_CaptureBehaviour = this.GetCaptureBehaviourByMode(param.camMode, param.captureSide, param.cameraType);
            }

            this.m_CameraParameters = param;
            this.m_Encoder = GetEncoderByMode(param.camMode);
            this.m_Encoder.Config(param);
            this.m_Blender = blender;
            this.m_Blender.Init(param.captureSide != CaptureSide.Both ? new Camera[] { m_CaptureBehaviour.CaptureCamera } : new Camera[] { m_CaptureBehaviour.CaptureCamera, m_CaptureBehaviour.CaptureCamera2 },
                m_Encoder, param);
            this.m_CaptureBehaviour.Init(this);
            this.m_CaptureBehaviour.SetBackGroundColor(param.backgroundColor);

            this.m_FrameProvider = CreateFrameProviderByMode(param.blendMode, param.frameRate, provider);
            this.m_FrameProvider.OnUpdate += UpdateFrame;

            this.m_FrameConsumerList = new List<IFrameConsumer>();
            this.Sequence(m_CaptureBehaviour)
                .Sequence(m_Blender);

            this.m_IsInitialized = true;
        }

        /// <summary> Auto adaption for BlendMode based on supported feature on current device. </summary>
        /// <param name="blendMode"> source blendMode.</param>
        /// <returns> Fallback blendMode. </returns>
        public BlendMode AutoAdaptBlendMode(BlendMode blendMode, bool isGrayCamera = false)
        {
            if (isGrayCamera)
            {
                if (!XREALPlugin.IsHMDFeatureSupported(XREALSupportedFeature.XREAL_FEATURE_PERCEPTION_HEAD_TRACKING_POSITION))
                {
                    return BlendMode.VirtualOnly;
                }
            }
            else
            {
                if (!XREALPlugin.IsHMDFeatureSupported(XREALSupportedFeature.XREAL_FEATURE_RGB_CAMERA))
                    return BlendMode.VirtualOnly;
            }

            return blendMode;
        }

        private FrameCaptureContext Sequence(IFrameConsumer consummer)
        {
            this.m_FrameConsumerList.Add(consummer);
            return this;
        }

        private void UpdateFrame(UniversalTextureFrame frame)
        {
            for (int i = 0; i < m_FrameConsumerList.Count; i++)
            {
                m_FrameConsumerList[i].OnFrame(frame);
            }
        }

        /// <summary> Gets capture behaviour by mode. </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs.</exception>
        /// <param name="mode"> The mode.</param>
        /// <returns> The capture behaviour by mode. </returns>
        private CaptureBehaviourBase GetCaptureBehaviourByMode(CamMode mode, CaptureSide side, CameraType cameraType)
        {
            if (mode == CamMode.PhotoMode)
            {
                XREALCaptureBehaviour capture = GameObject.FindObjectOfType<XREALCaptureBehaviour>();
                Transform headParent = Camera.main.transform;
                if (capture == null)
                {
                    capture = GameObject.Instantiate(Resources.Load<XREALCaptureBehaviour>("Prefabs/NRCaptureBehaviour"), headParent);
                }
                GameObject.DontDestroyOnLoad(capture.gameObject);
                if (side == CaptureSide.Both)
                {
                    capture.CaptureCamera.gameObject.SetActive(true);
                    capture.CaptureCamera2.gameObject.SetActive(true);
                    capture.CaptureCamera.gameObject.GetComponent<XREALCameraInitializer>().SwitchToEyeParam(
                        cameraType == CameraType.RGB ? XREALComponent.XREAL_COMPONENT_DISPLAY_LEFT : XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_LEFT);
                    capture.CaptureCamera2.gameObject.GetComponent<XREALCameraInitializer>().SwitchToEyeParam(
                        cameraType == CameraType.RGB ? XREALComponent.XREAL_COMPONENT_DISPLAY_RIGHT : XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_RIGHT);
                }
                else
                {
                    capture.CaptureCamera.gameObject.SetActive(true);
                    capture.CaptureCamera2.gameObject.SetActive(false);
                    capture.CaptureCamera.gameObject.GetComponent<XREALCameraInitializer>().SwitchToEyeParam(
                        cameraType == CameraType.RGB ? XREALComponent.XREAL_COMPONENT_RGB_CAMERA :
                        (side == CaptureSide.Left ? XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_LEFT : XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_RIGHT));
                }
                return capture;
            }
            else if (mode == CamMode.VideoMode)
            {
                XREALRecordBehaviour capture = GameObject.FindObjectOfType<XREALRecordBehaviour>();
                Transform headParent = Camera.main.transform;
                if (capture == null)
                {
                    capture = GameObject.Instantiate(Resources.Load<XREALRecordBehaviour>("Prefabs/NRRecorderBehaviour"), headParent);
                }
                GameObject.DontDestroyOnLoad(capture.gameObject);
                if (side == CaptureSide.Both)
                {
                    capture.CaptureCamera.gameObject.SetActive(true);
                    capture.CaptureCamera2.gameObject.SetActive(true);
                    capture.CaptureCamera.gameObject.GetComponent<XREALCameraInitializer>().SwitchToEyeParam(
                        cameraType == CameraType.RGB ? XREALComponent.XREAL_COMPONENT_DISPLAY_LEFT : XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_LEFT);
                    capture.CaptureCamera2.gameObject.GetComponent<XREALCameraInitializer>().SwitchToEyeParam(
                        cameraType == CameraType.RGB ? XREALComponent.XREAL_COMPONENT_DISPLAY_RIGHT : XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_RIGHT);
                }
                else
                {
                    capture.CaptureCamera.gameObject.SetActive(true);
                    capture.CaptureCamera2.gameObject.SetActive(false);
                    capture.CaptureCamera.gameObject.GetComponent<XREALCameraInitializer>().SwitchToEyeParam(
                        cameraType == CameraType.RGB ? XREALComponent.XREAL_COMPONENT_RGB_CAMERA :
                        (side == CaptureSide.Left ? XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_LEFT : XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_RIGHT));
                }
                return capture;
            }
            else
            {
                throw new Exception("CamMode need to be set correctly for capture behaviour!");
            }
        }

        private AbstractFrameProvider CreateFrameProviderByMode(BlendMode mode, int fps, AbstractFrameProvider frameProvider)
        {
            AbstractFrameProvider provider = null;
            switch (mode)
            {
                case BlendMode.Blend:
                case BlendMode.CameraOnly:
#if UNITY_EDITOR
                    provider = new EditorFrameProvider();
#else
                    provider = frameProvider;
#endif
                    break;
                case BlendMode.VirtualOnly:
                default:
                    provider = new NullDataFrameProvider(fps);
                    break;
            }

            return provider;
        }

        /// <summary> Gets encoder by mode. </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs.</exception>
        /// <param name="mode"> The mode.</param>
        /// <returns> The encoder by mode. </returns>
        private IEncoder GetEncoderByMode(CamMode mode)
        {
            if (mode == CamMode.PhotoMode)
            {
                return new ImageEncoder();
            }
            else if (mode == CamMode.VideoMode)
            {
                return new VideoEncoder();
            }
            else
            {
                throw new Exception("CamMode need to be set correctly for encoder!");
            }
        }

        /// <summary> Stops capture mode. </summary>
        public void StopCaptureMode()
        {
            this.Release();
        }

        /// <summary> Starts a capture. </summary>
        public void StartCapture()
        {
            if (!m_IsInitialized)
            {
                return;
            }
            Debug.Log("[CaptureContext] Start...");

            m_Encoder?.Start();
            m_FrameProvider?.Play();
        }

        /// <summary> Stops a capture. </summary>
        public void StopCapture()
        {
            if (!m_IsInitialized)
            {
                return;
            }
            Debug.Log("[CaptureContext] Stop...");

            // Need stop encoder firstly.
            m_Encoder?.Stop();
            m_FrameProvider?.Stop();
        }

        /// <summary> Releases this object. </summary>
        public void Release()
        {
            if (!m_IsInitialized)
            {
                return;
            }

            Debug.Log("[CaptureContext] Release begin...");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            if (m_FrameProvider != null)
            {
                m_FrameProvider.OnUpdate -= UpdateFrame;
                m_FrameProvider?.Release();
                m_FrameProvider = null;
            }

            m_Blender?.Dispose();
            m_Encoder?.Release();

            if (m_CaptureBehaviour != null)
            {
                GameObject.DestroyImmediate(m_CaptureBehaviour.gameObject);
                m_CaptureBehaviour = null;
            }
            Debug.LogFormat("[CaptureContext] Release end, cost:{0} ms", stopwatch.ElapsedMilliseconds);

            m_IsInitialized = false;
        }
    }
}
