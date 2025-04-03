using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Represents a notification for low battery warnings
    /// </summary>
    internal class XREALBatteryNotification : XREALNotification
    {
        private enum BatteryLevel
        {
            Low,
            Middle,
            Full,
        }

        /// <summary> The threshold value for low battery level, below which a low battery warning will be triggered. </summary>
        [SerializeField]
        private float m_BatteryLowLevelThreshold = 0.2f;
        /// <summary> The threshold value for middle battery level, below which a low battery warning will be triggered. </summary>
        [SerializeField]
        private float m_BatteryMiddleThreshold = 0.3f;

        private BatteryLevel m_CurrentLevel = BatteryLevel.Full;
        private float m_BatteryValue = 1.0f;

        private void Update()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            m_BatteryValue = SystemInfo.batteryLevel;
#else
            m_BatteryValue = 1.0f;
#endif
            var lastLevel = m_CurrentLevel;
            if (m_BatteryValue <= m_BatteryLowLevelThreshold)
            {
                m_CurrentLevel = BatteryLevel.Low;
                if (lastLevel > BatteryLevel.Low)
                {
                    NotifyTrigger();
                }
            }
            else if (m_BatteryValue <= m_BatteryMiddleThreshold)
            {
                m_CurrentLevel = BatteryLevel.Middle;
                if (lastLevel > BatteryLevel.Middle)
                {
                    NotifyTrigger();
                }
            }
            else if (m_BatteryValue > m_BatteryMiddleThreshold)
            {
                m_CurrentLevel = BatteryLevel.Full;
            }
        }

        internal override void NotifyTrigger()
        {
            if (glassesSpacePopup)
            {
                if (XREALLocalizationTool.Singleton != null)
                {
                    string title, message;
                    if (m_CurrentLevel == BatteryLevel.Low)
                    {
                        title = XREALLocalizationTool.Singleton.GetNotificationContent(XREALLocalizationTool.kBatteryLowWarningTitleKey);
                        message = XREALLocalizationTool.Singleton.GetNotificationContent(XREALLocalizationTool.kBatteryLowWarningMessageKey);
                    }
                    else
                    {
                        title = XREALLocalizationTool.Singleton.GetNotificationContent(XREALLocalizationTool.kBatteryMiddleWarningTitleKey);
                        message = XREALLocalizationTool.Singleton.GetNotificationContent(XREALLocalizationTool.kBatteryMiddleWarningMessageKey);
                    }

                    GameObject go = GameObject.Instantiate(glassesSpacePopup);
                    var spacePopup = go.GetComponent<XREALDialogView>();
                    if (spacePopup != null)
                    {
                        spacePopup.SetTitle(title);
                        spacePopup.SetContent(message);
                        spacePopup.SetDuration(m_NotificationDisplayTime);
                        spacePopup.Show();
                        Debug.LogFormat("[XREALBattery] battery value = {0}", m_BatteryValue);
                    }
                }
            }
        }
    }
}
