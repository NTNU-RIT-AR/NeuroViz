using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Represents a notification for high temperature alerts on the glasses.
    /// </summary>
    internal class XREALTemperatureNotification : XREALNotification
    {
        private void OnEnable()
        {
            XREALCallbackHandler.OnXREALGlassesTemperatureLevel += OnXREALGlassesTemperatureLevel;
        }

        private void OnDisable()
        {
            XREALCallbackHandler.OnXREALGlassesTemperatureLevel -= OnXREALGlassesTemperatureLevel;
        }

        private void OnXREALGlassesTemperatureLevel(XREALTemperatureLevel level)
        {
            if (level == XREALTemperatureLevel.LEVEL_WARM || level == XREALTemperatureLevel.LEVEL_HOT)
            {
                NotifyTrigger();
            }
        }

        internal override void NotifyTrigger()
        {
            if (glassesSpacePopup)
            {
                if (XREALLocalizationTool.Singleton != null)
                {
                    string title = XREALLocalizationTool.Singleton.GetNotificationContent(XREALLocalizationTool.kTemperatureTitleKey);
                    string message = XREALLocalizationTool.Singleton.GetNotificationContent(XREALLocalizationTool.kTemperatureMessageKey);

                    GameObject go = GameObject.Instantiate(glassesSpacePopup);
                    var spacePopup = go.GetComponent<XREALDialogView>();
                    if (spacePopup != null)
                    {
                        spacePopup.SetTitle(title);
                        spacePopup.SetContent(message);
                        spacePopup.SetDuration(m_NotificationDisplayTime);
                        spacePopup.Show();
                    }
                }
            }
        }
    }
}
