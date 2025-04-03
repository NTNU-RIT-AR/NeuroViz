using System;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Enum representing the type of popup UI, indicating whether the popup appears on the phone screen or the glasses screen.
    /// </summary>
    public enum PopupUIType
    {
        /// <summary>
        /// No popup UI type specified.
        /// </summary>
        None,
        /// <summary>
        /// Popup UI that appears in the 3D space (glasses screen).
        /// </summary>
        SpaceUI,
        /// <summary>
        /// Popup UI that appears on the phone screen.
        /// </summary>
        ScreenUI,
    }

    /// <summary>
    /// Represents a notification for native error
    /// </summary>
    public class XREALNativeErrorNotification : XREALNotification
    {
        /// <summary> Event queue for all listeners interested in OnXREALSDKFailPreComfirm events. </summary>
        public static event Action OnXREALSDKFailPreComfirm;
        /// <summary> A UI popup that is displayed on the mobile screen.  </summary>
        [SerializeField]
        internal GameObject mobileScreenPopup;

        private void OnEnable()
        {
            XREALCallbackHandler.OnXREALError += OnErrorCallback;
        }

        private void Start()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (XREALPlugin.GetDeviceCategory() != XREALDeviceCategory.XREAL_DEVICE_CATEGORY_INVALID 
                && !XREALSettings.GetSettings().IsDeviceSupported())
                XREALCallbackHandler.InvokeXREALError(XREALErrorCode.UnSupportDevice);
#endif
        }

        private void OnDisable()
        {
            XREALCallbackHandler.OnXREALError -= OnErrorCallback;
        }

        void QuitApplication()
        {
            XREALPlugin.QuitApplication();
        }

        private void MobileScreenPopupCloseCallback(XREALDialogView view)
        {
            QuitApplication();
        }

        void OnErrorCallback(XREALErrorCode errorCode, string defaultContent)
        {
            Debug.Log("[XREALErrorReceiver] OnErrorCallback " + errorCode);

            PopupUIType uiType = GetPopUIType(errorCode);

            if (uiType == PopupUIType.None)
                return;

            string messageContent = ErrorCodeToMessage(errorCode, defaultContent);
            if (uiType == PopupUIType.ScreenUI)
            {
                if (mobileScreenPopup != null)
                {
                    GameObject go = GameObject.Instantiate(mobileScreenPopup);
                    var screenPopup = go.GetComponent<XREALDialogView>();
                    if (screenPopup != null)
                    {
                        screenPopup.SetContent(messageContent)
                           .SetDuration(m_NotificationDisplayTime)
                           .SetConfirmAction(QuitApplication)
                           .SetCloseCallback(MobileScreenPopupCloseCallback)
                           .Show();
                    }
                }
                OnXREALSDKFailPreComfirm?.Invoke();
            }
            else
            {
                if (glassesSpacePopup != null)
                {
                    string titleContent = ErrorCodeToTitle(errorCode);
                    GameObject go = GameObject.Instantiate(glassesSpacePopup);
                    var spacePopup = go.GetComponent<XREALDialogView>();
                    if (spacePopup != null)
                    {
                        spacePopup.SetContent(messageContent)
                          .SetTitle(titleContent)
                          .SetDuration(m_NotificationDisplayTime)
                          .ShowInQueue();
                    }
                }
            }
        }

        private string ErrorCodeToMessage(XREALErrorCode errorCode, string defaultContent)
        {
            if (XREALLocalizationTool.Singleton != null)
            {
                if (XREALLocalizationTool.Singleton.GetMessageFromErrorCode(errorCode, out string errorMsg))
                    return errorMsg;
            }

            if (defaultContent != null && !defaultContent.Contains("+"))
            {
                return defaultContent;
            }
            else
            {
                return GetDefaultMessage(errorCode, defaultContent);
            }
        }

        private string ErrorCodeToTitle(XREALErrorCode errorCode)
        {
            if (XREALLocalizationTool.Singleton != null)
            {
                if (XREALLocalizationTool.Singleton.GetTitleFromErrorCode(errorCode, out string title))
                    return title;
            }
            return errorCode.ToString();
        }

        private PopupUIType GetPopUIType(XREALErrorCode errorCode)
        {
            PopupUIType uiType = PopupUIType.None;
            if (XREALLocalizationTool.Singleton != null)
            {
                bool result = XREALLocalizationTool.Singleton.GetPopupUITypeFromErrorCode(errorCode, out uiType);
                if (result)
                    return uiType;
            }

            switch (errorCode)
            {
                case XREALErrorCode.Failure:
                case XREALErrorCode.InvalidArgument:
                case XREALErrorCode.InvalidData:
                    uiType = PopupUIType.None;
                    break;

                case XREALErrorCode.NotEnoughMemory:
                case XREALErrorCode.ControlChannelInternalError:
                case XREALErrorCode.ControlChannelInitFail:
                case XREALErrorCode.ControlChannelStartFail:
                case XREALErrorCode.ImuChannelInitFail:
                case XREALErrorCode.ImuChannelStartFail:
                case XREALErrorCode.DPDeviceNotFind:
                case XREALErrorCode.GetDisplayFailure:
                case XREALErrorCode.GetDisplayModeMismatch:
                case XREALErrorCode.DisplayNoInStereoMode:
                case XREALErrorCode.NotFindRuntime:
                case XREALErrorCode.UnSupportDevice:

                    uiType = PopupUIType.ScreenUI;
                    break;
                case XREALErrorCode.RGBCameraDeviceNotFind:
                case XREALErrorCode.LicenseFeatureUnsupported:
                case XREALErrorCode.LicenseDeviceUnsupported:
                case XREALErrorCode.LicenseExpiration:
                case XREALErrorCode.PermissionDenyError:
                    uiType = PopupUIType.SpaceUI;
                    break;

                default:
                    uiType = PopupUIType.None;
                    break;
            }

            return uiType;
        }

        private string GetDefaultMessage(XREALErrorCode errorCode, string defaultContent)
        {
            string errorMsg = null;
            switch (errorCode)
            {
                case XREALErrorCode.Success:
                    break;
                case XREALErrorCode.Failure:
                    break;
                case XREALErrorCode.InvalidArgument:
                    break;
                case XREALErrorCode.NotEnoughMemory:
                    errorMsg = NativeConstants.NotEnoughMemory;
                    break;
                case XREALErrorCode.UnSupported:
                case XREALErrorCode.UnSupportDevice:
                    errorMsg = defaultContent + NativeConstants.UnSupportedErrorTip;
                    break;
                case XREALErrorCode.ControlChannelInternalError:
                case XREALErrorCode.ControlChannelInitFail:
                case XREALErrorCode.ControlChannelStartFail:
                case XREALErrorCode.ImuChannelInitFail:
                case XREALErrorCode.ImuChannelStartFail:
                    errorMsg = NativeConstants.GlassesDisconnectErrorTip;
                    break;
                case XREALErrorCode.RGBCameraDeviceNotFind:
                    errorMsg = NativeConstants.RGBCameraNotFindTip;
                    break;
                case XREALErrorCode.DPDeviceNotFind:
                    errorMsg = NativeConstants.DPDeviceNotFindTip;
                    break;
                case XREALErrorCode.GetDisplayFailure:
                    errorMsg = NativeConstants.GetDisplayFailureErrorTip;
                    break;
                case XREALErrorCode.GetDisplayModeMismatch:
                case XREALErrorCode.DisplayNoInStereoMode:
                    errorMsg = NativeConstants.DisplayModeMismatchErrorTip;
                    break;
                case XREALErrorCode.InTheCoolDown:
                    break;
                case XREALErrorCode.InvalidData:
                    break;
                case XREALErrorCode.NotFindRuntime:
                    errorMsg = NativeConstants.SDKRuntimeNotFoundErrorTip;
                    break;
                case XREALErrorCode.LicenseFeatureUnsupported:
                    errorMsg = NativeConstants.LicenseNotSupportRequestedFeature;
                    break;
                case XREALErrorCode.LicenseDeviceUnsupported:
                    errorMsg = NativeConstants.LicenseNotSupportCurrentDevice;
                    break;
                case XREALErrorCode.LicenseExpiration:
                    errorMsg = NativeConstants.LicenseExpiredErrorTip;
                    break;
                case XREALErrorCode.ControlTryAgainLater:
                    break;
                case XREALErrorCode.PermissionDenyError:
                    errorMsg = NativeConstants.PermissionDenyErrorTip;
                    break;

                default:
                    break;
            }
            errorMsg = string.Format("nrsdk errorMsg = {0} code = {1} {2}", errorMsg, errorCode, (int)errorCode);
            return errorMsg;
        }
    }
}
