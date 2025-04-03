using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Handles enable or disable of notification messages within the application.
    /// </summary>
    public class XREALNotificationListener : MonoBehaviour
    {
        /// <summary> True to enable, false to disable the low power tips. </summary>
        [Header("Whether to open the low power prompt")]
        [SerializeField]
        private bool m_EnableLowPowerTips = true;
        [SerializeField]
        /// <summary> The low power notification. </summary>
        private XREALBatteryNotification BatteryNotification;

        /// <summary> True to enable, false to disable the high temporary tips. </summary>
        [Header("Whether to open the over temperature prompt")]
        [SerializeField]
        private bool m_EnableHighTempTips = true;
        [SerializeField]
        /// <summary> The high temporary notification. </summary>
        private XREALTemperatureNotification TemperatureNotification;

        /// <summary> True to enable, false to disable the slam state tips. </summary>
        [Header("Whether to open the slam state prompt")]
        [SerializeField]
        private bool m_EnableSlamStateTips = true;
        /// <summary> The slam state notification.  </summary>
        [SerializeField]
        private XREALSlamStateNotification SlamStateNotification;

        [Header("Whether to open the native error prompt")]
        [SerializeField]
        private bool m_EnableNativeErrorTips = true;
        /// <summary> The Native error notification </summary>
        [SerializeField]
        private XREALNativeErrorNotification NativeErrorNotification;

        /// <summary>
        /// Enables or disables low battery notification.
        /// </summary>
        public bool EnableLowPowerTips
        {
            get { return m_EnableLowPowerTips; }
            set
            {
                if (m_EnableLowPowerTips != value)
                {
                    m_EnableLowPowerTips = value;
                    BatteryNotification?.gameObject.SetActive(m_EnableLowPowerTips);
                }
            }
        }

        /// <summary>
        /// Enables or disables high-temperature notifications.
        /// </summary>
        public bool EnableHighTempTips
        {
            get { return m_EnableHighTempTips; }
            set
            {
                if (m_EnableHighTempTips != value)
                {
                    m_EnableHighTempTips = value;
                    TemperatureNotification?.gameObject.SetActive(m_EnableHighTempTips);
                }
            }
        }

        /// <summary>
        /// Enables or disables slam track notifications.
        /// </summary>
        public bool EnableSlamStateTips
        {
            get { return m_EnableSlamStateTips; }
            set
            {
                if (m_EnableSlamStateTips != value)
                {
                    m_EnableSlamStateTips = value;
                    SlamStateNotification?.gameObject.SetActive(m_EnableSlamStateTips);
                }
            }
        }

        /// <summary>
        /// Enables or disables native errors notification.
        /// </summary>
        public bool EnableNativeErrorTips
        {
            get { return m_EnableNativeErrorTips; }
            set
            {
                if (m_EnableNativeErrorTips != value)
                {
                    m_EnableNativeErrorTips = value;
                    NativeErrorNotification?.gameObject.SetActive(m_EnableNativeErrorTips);
                }
            }
        }

        private void Awake()
        {
            BatteryNotification?.gameObject.SetActive(m_EnableLowPowerTips);
            TemperatureNotification?.gameObject.SetActive(m_EnableHighTempTips);
            SlamStateNotification?.gameObject.SetActive(m_EnableSlamStateTips);
            NativeErrorNotification?.gameObject.SetActive(m_EnableNativeErrorTips);
        }
    }
}
