using System;

namespace Unity.XR.XREAL
{
    public enum XREALErrorCode
    {
        Success = 0,
        Failure = 1,
        InvalidArgument = 2,
        NotEnoughMemory = 3,
        UnSupported = 4,
        [Obsolete]
        GlassesDisconnect = 5,
        [Obsolete]
        SdkVersionMismatch = 6,
        [Obsolete]
        SdcardPermissionDeny = 7,
        RGBCameraDeviceNotFind = 8,
        DPDeviceNotFind = 9,
        [Obsolete]
        TrackingNotRunning = 10,
        GetDisplayFailure = 11,
        GetDisplayModeMismatch = 12,
        InTheCoolDown = 13,
        [Obsolete]
        UnSupportedHandtrackingCalculation = 14,
        [Obsolete]
        Busy = 15,
        [Obsolete]
        Processing = 16,
        [Obsolete]
        NumberLimited = 17,
        DisplayNoInStereoMode = 18,
        InvalidData = 19,
        NotFindRuntime = 20,
        [Obsolete]
        TimeOut = 21,

        LicenseFeatureUnsupported = 22,
        LicenseDeviceUnsupported = 23,
        LicenseExpiration = 24,

        ControlChannelInternalError = 100,
        ControlChannelInitFail = 101,
        ControlChannelStartFail = 102,
        ControlTryAgainLater = 103,
        [Obsolete]
        ImuChannelInternalError = 200,
        ImuChannelInitFail = 201,
        ImuChannelStartFail = 202,
        [Obsolete]
        ImuChannelFrequencyCritical = 203,
        [Obsolete]
        DisplayControlChannelInternalError = 300,
        [Obsolete]
        DisplayControlChannelInitFail = 301,
        [Obsolete]
        DisplayControlChannelStartFail = 302,
        [Obsolete]
        DisplayControlChannelFrequencyCritical = 303,
        [Obsolete]
        GrayCameraChannelInternalError = 400,

        PermissionDenyError = 10100,
        UnSupportDevice = 10102,
    }
}
