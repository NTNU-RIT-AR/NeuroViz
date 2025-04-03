namespace Unity.XR.XREAL
{
    public static class NativeConstants
    {
        public const int RECORD_FPS_DEFAULT = 30;
        public const int RECORD_VIDEO_BITRATE_DEFAULT = 10240000;
        public const int RECORD_AUDIO_BITRATE_DEFAULT = 256000;
        public const int RECORD_AUDIO_SAMPLERATE_DEFAULT = 48000;
        public const int RECORD_AUDIO_BYTES_PER_SAMPLE = 2;
        public const int RECORD_AUDIO_CHANNEL = 2;
        public const float RECORD_VOLUME_MIC = 1;
        public const float RECORD_VOLUME_APP = 1;

        public static string GlassesDisconnectErrorTip         = "Please connect your Glasses.";
        public static string SdkVersionMismatchErrorTip        = "Please update to the latest version of NRSDK.";
        public static string UnknownErrorTip                   = "Unknown error! \nPlease contact customer service.";
        public static string NotEnoughMemory                   = "Out of memory.";
        public static string SdcardPermissionDenyErrorTip      = "There is no read permission for sdcard. Please go to the authorization management page of the device to authorize.";
        public static string RGBCameraNotFindTip               = "Can not find the rgb camera device error.";
        public static string DPDeviceNotFindTip                = "Glasses display device not find! \nPlease contact customer service.";
        public static string GetDisplayFailureErrorTip         = "MRSpace display device not find! \nPlease contact customer service.";
        public static string DisplayModeMismatchErrorTip       = "Display mode mismatch, as MRSpace mode is needed! \nPlease contact customer service.";
        public static string SDKRuntimeNotFoundErrorTip        = "Not found sdk runtime! \nPlease start the server app firstly.";
        public static string LicenseExpiredErrorTip            = "License has expired.";
        public static string LicenseNotSupportCurrentDevice    = "License not support current device.";
        public static string LicenseNotSupportRequestedFeature = "License not support requested feature.";
        public static string ScreenCaptureDenyErrorTip         = "Screen capture needs to be approved.";
        public static string PermissionDenyErrorTip            = "Record audio needs the permission of \n" +
                            "'android.permission.RECORD_AUDIO', Add it to the 'AndroidManifest.xml'";
        public static string TrackingModeSwitchTip             = "Tracking system is switching mode...";
        public static string UnSupportedErrorTip               =" UnSupported error.";
    }
}
