using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Represents a notification system for displaying pop-up messages or alerts.
    /// </summary>
    public class XREALNotification : MonoBehaviour
    {
        [SerializeField]
        protected float m_NotificationDisplayTime = 5.0f;
        [SerializeField]
        internal GameObject glassesSpacePopup;

        internal virtual void NotifyTrigger() { }
    }
}
