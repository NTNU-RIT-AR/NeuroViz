using UnityEngine;
#if XR_ARFOUNDATION
using UnityEngine.XR.ARSubsystems;
#endif

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Represents a notification for the SLAM tracking state.
    /// </summary>
    internal class XREALSlamStateNotification : XREALNotification
    {
#pragma warning disable CS0414
        /// <summary> whether slam is ready. </summary>
        private bool m_TrackingReady = false;
        /// <summary> Stores the reason for tracking in the previous frame. </summary>
        private NotTrackingReason m_LastReason;
        /// <summary> Represents the current DOF tracking type. </summary>
        private TrackingType m_TrackingType;
        private bool m_IgnoreLostTrackingDialog;
        private GameObject m_SpacePopupObj;
#pragma warning restore CS0414

        private void OnEnable()
        {
            m_TrackingType = XREALPlugin.GetTrackingType();
            XREALPlugin.OnBeginChangeTrackingType += OnBeginChangeTrackingType;
            XREALPlugin.OnTrackingTypeChanged += OnTrackingTypeChanged;
        }

        private void OnDisable()
        {
            XREALPlugin.OnBeginChangeTrackingType -= OnBeginChangeTrackingType;
            XREALPlugin.OnTrackingTypeChanged -= OnTrackingTypeChanged;
        }

        private void Update()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (m_IgnoreLostTrackingDialog || m_TrackingType == TrackingType.MODE_0DOF || m_TrackingType == TrackingType.MODE_0DOF_STAB)
                return;

            var currentReason = XREALPlugin.GetTrackingReason();
            if (currentReason != m_LastReason)
            {
                if (!m_TrackingReady)
                {
                    if (currentReason == NotTrackingReason.None)
                        m_TrackingReady = true;
                }
                else
                {
                    if (currentReason != NotTrackingReason.None)
                    {
                        NotifyTrigger();
                    }
                }
                m_LastReason = currentReason;
            }
#endif
        }

        internal override void NotifyTrigger()
        {
            if (glassesSpacePopup)
            {
                if (XREALLocalizationTool.Singleton != null)
                {
                    string title = XREALLocalizationTool.Singleton.GetNotificationContent(XREALLocalizationTool.kTrackingLoseTitleKey);
                    string message = XREALLocalizationTool.Singleton.GetNotificationContent(XREALLocalizationTool.kTrackingLoseMessageKey);

                    if(m_SpacePopupObj == null)
                    {
                        m_SpacePopupObj = Instantiate(glassesSpacePopup);
                    }
                    var spacePopup = m_SpacePopupObj.GetComponent<XREALDialogView>();
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

        /// <summary>
        /// Called when the tracking type is about to change.
        /// </summary>
        /// <param name="from"> The current tracking type before the change. </param>
        /// <param name="to"> The new tracking type after the change. </param>
        private void OnBeginChangeTrackingType(TrackingType from, TrackingType to)
        {
            m_IgnoreLostTrackingDialog = true;
        }

        /// <summary>
        /// Called when the tracking type has changed.
        /// </summary>
        /// <param name="result"> Indicates whether the tracking type change was successful. </param>
        /// <param name="targetTrackingType"> The new target tracking type after the change. </param>
        private void OnTrackingTypeChanged(bool result, TrackingType targetTrackingType)
        {
            if (result)
            {
                m_TrackingType = targetTrackingType;
            }
            if (XREALMainThreadDispather.Singleton != null)
            {
                XREALMainThreadDispather.Singleton.QueueOnMainThreadWithDelay(() =>
                {
                    m_IgnoreLostTrackingDialog = false;
                }, 1.0f);
            }
            else
            {
                m_IgnoreLostTrackingDialog = false;
            }
        }
    }
}
