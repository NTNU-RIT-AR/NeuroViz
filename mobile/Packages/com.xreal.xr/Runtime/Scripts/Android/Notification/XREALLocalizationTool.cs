namespace Unity.XR.XREAL
{
    /// <summary>
    /// A utility class for storing notification-related data and methods for modifying notifications.
    /// Provides functionality for handling and customizing notification strings.
    /// </summary>
    public class XREALLocalizationTool : SingletonMonoBehaviour<XREALLocalizationTool>
    {
        public const string kBatteryLowWarningTitleKey = "BatteryLowWarningTitle";
        public const string kBatteryLowWarningMessageKey = "BatteryLowWarningMessage";
        public const string kBatteryMiddleWarningTitleKey = "BatteryMiddleWarningTitle";
        public const string kBatteryMiddleWarningMessageKey = "BatteryMiddleWarningMessage";
        public const string kTemperatureTitleKey = "TemperatureTitle";
        public const string kTemperatureMessageKey = "TemperatureMessage";
        public const string kTrackingLoseTitleKey = "TrackingLoseTitle";
        public const string kTrackingLoseMessageKey = "TrackingLoseMessage";

        /// <summary> The title displayed when the battery level is low. </summary>
        public string BatteryLowWarningTitle = "Low Battery";
        /// <summary> The message displayed when the battery level is low. </summary>
        public string BatteryLowWarningMessage = "Battery is extremely low. Please charge your phone.";
        /// <summary> The title displayed when the battery level reaches a warning threshold (middle level). </summary>
        public string BatteryMiddleWarningTitle = "Battery Warning";
        /// <summary> The message displayed when the battery level reaches a waring threshold (middle level). </summary>
        public string BatteryMiddleWarningMessage = "Battery is low. Please charge to at least 30% battery.";

        /// <summary> The title displayed when the temperature of the glasses is too high, indicating a heat warning. </summary>
        public string TemperatureTitle = "Heat Warning";
        /// <summary> The message displayed when the temperature of the glasses is too high, indicating a heat warning. </summary>
        public string TemperatureMessage = "The glasses temperature is too high, the device will shut down soon. Please take a break and use it again later.";

        /// <summary> The title displayed when the SLAM tracking state is abnormal or lost. </summary>
        public string TrackingLoseTitle = "Tracking";
        /// <summary> The message displayed when the SLAM tracking state is abnormal or lost. </summary>
        public string TrackingLoseMessage = "Feature points are not enough, Please look around...";

        public delegate bool TranslateStrToStrDelegate(string key, out string value);
        /// <summary> An event delegate that converts one string to another. </summary>
        public TranslateStrToStrDelegate NotificationStrGenerator;

        public delegate bool NativeErrorToStrDelegate(XREALErrorCode code, out string value);
        /// <summary> A delegate to modify and generate the title for native errors. </summary>
        public NativeErrorToStrDelegate NativeErrorTitleGenerator;
        /// <summary> A delegate to modify and generate the message for native errors. </summary>
        public NativeErrorToStrDelegate NativeErrorMessageGenerator;

        public delegate bool NativeErrorToPopupUITypeDelegate(XREALErrorCode code, out PopupUIType popupUIType);
        /// <summary> A delegate to modify the PopupUIType for native errors. </summary>
        public NativeErrorToPopupUITypeDelegate NativeErrorUITypeChange;

        /// <summary>
        /// Retrieves the notification content associated with the given key.
        /// </summary>
        /// <param name="keyStr">The key used to look up the notification content.</param>
        /// <returns>The notification content as a string. Returns an empty string if the key is not found.</returns>
        internal string GetNotificationContent(string keyStr)
        {
            if (string.IsNullOrEmpty(keyStr))
                return "";

            string contentStr = "";
            if (NotificationStrGenerator != null)
            {
                if (!NotificationStrGenerator.Invoke(keyStr, out contentStr))
                    contentStr = GetValueByKey(keyStr);
            }
            else
            {
                contentStr = GetValueByKey(keyStr);
            }
            return contentStr;
        }

        private string GetValueByKey(string keyStr)
        {
            string valueStr = "";
            switch (keyStr)
            {
                case kTemperatureTitleKey:
                    valueStr = TemperatureTitle;
                    break;
                case kTemperatureMessageKey:
                    valueStr = TemperatureMessage;
                    break;
                case kBatteryLowWarningTitleKey:
                    valueStr = BatteryLowWarningTitle;
                    break;
                case kBatteryLowWarningMessageKey:
                    valueStr = BatteryLowWarningMessage;
                    break;
                case kBatteryMiddleWarningTitleKey:
                    valueStr = BatteryMiddleWarningTitle;
                    break;
                case kBatteryMiddleWarningMessageKey:
                    valueStr = BatteryMiddleWarningMessage;
                    break;
                case kTrackingLoseTitleKey:
                    valueStr = TrackingLoseTitle;
                    break;
                case kTrackingLoseMessageKey:
                    valueStr = TrackingLoseMessage;
                    break;
            }
            return valueStr;
        }

        /// <summary>
        /// Retrieves the title associated with a given error code.
        /// </summary>
        /// <param name="code"> native error code </param>
        /// <param name="title"> out title </param>
        /// <returns></returns>
        internal bool GetTitleFromErrorCode(XREALErrorCode code, out string title)
        {
            if (NativeErrorTitleGenerator != null)
                return NativeErrorTitleGenerator.Invoke(code, out title);

            title = "";
            return false;
        }

        /// <summary>
        /// Retrieves the message associated with a given error code.
        /// </summary>
        /// <param name="code"> native error code </param>
        /// <param name="message"> out messge </param>
        /// <returns></returns>
        internal bool GetMessageFromErrorCode(XREALErrorCode code, out string message)
        {
            if (NativeErrorMessageGenerator != null)
                return NativeErrorMessageGenerator.Invoke(code, out message);

            message = "";
            return false;
        }

        /// <summary>
        /// Retrieves the type of popup UI associated with a given error code.
        /// </summary>
        /// <param name="code"> native error code </param>
        /// <param name="popupUIType"> out PopupUIType </param>
        /// <returns></returns>
        internal bool GetPopupUITypeFromErrorCode(XREALErrorCode code, out PopupUIType popupUIType)
        {
            if (NativeErrorUITypeChange != null)
                return NativeErrorUITypeChange(code, out popupUIType);
            popupUIType = PopupUIType.SpaceUI;
            return false;
        }
    }
}
