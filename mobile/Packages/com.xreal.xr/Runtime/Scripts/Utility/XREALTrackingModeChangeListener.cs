using System.Collections;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Listen to the tracking type change event and show the mask when the tracking type is changing.
    /// </summary>
    public class XREALTrackingModeChangeListener : MonoBehaviour
    {
        [SerializeField]
        XREALTrackingTypeChangeMask m_TrackingTypeChangePrefab;

        XREALTrackingTypeChangeMask m_TrackingTypeChangeMask;
        Coroutine m_FadeCoroutine;
        bool m_IsTrackingTypeChanging = false;
        WaitForSeconds m_Delay = new WaitForSeconds(0.1f);
        void Awake()
        {
            if (m_TrackingTypeChangePrefab != null)
            {
                XREALPlugin.OnBeginChangeTrackingType += OnBeginTrackingTypeChanged;
                XREALPlugin.OnTrackingTypeChanged += OnTrackingTypeChanged;
            }
        }

        void OnBeginTrackingTypeChanged(TrackingType fromTrackingType, TrackingType targetTrackingType)
        {
            if (m_TrackingTypeChangeMask == null)
            {
                m_TrackingTypeChangeMask = Instantiate(m_TrackingTypeChangePrefab);
            }
            if (m_FadeCoroutine != null)
            {
                StopCoroutine(m_FadeCoroutine);
            }
            m_IsTrackingTypeChanging = true;
            m_FadeCoroutine = StartCoroutine(Fade());
        }

        void OnTrackingTypeChanged(bool result, TrackingType targetTrackingType)
        {
            m_IsTrackingTypeChanging = false;
        }

        IEnumerator Fade()
        {
            m_TrackingTypeChangeMask.Show();
            yield return null;
            yield return null;
            yield return null;

            var timeElapse = 0f;
            const float MaxTimeLastLimited = 6f;
            while (m_IsTrackingTypeChanging && timeElapse < MaxTimeLastLimited)
            {
                timeElapse += Time.deltaTime;
                yield return null;
            }
            yield return m_Delay; // Delay for a while after the switch is complete before hiding the mask
            m_TrackingTypeChangeMask.Hide();
            m_FadeCoroutine = null;
        }

        void OnDestroy()
        {
            XREALPlugin.OnBeginChangeTrackingType -= OnBeginTrackingTypeChanged;
            XREALPlugin.OnTrackingTypeChanged -= OnTrackingTypeChanged;

            if (m_FadeCoroutine != null)
            {
                StopCoroutine(m_FadeCoroutine);
                m_FadeCoroutine = null;
            }
            if (m_TrackingTypeChangeMask != null)
            {
                Destroy(m_TrackingTypeChangeMask.gameObject);
                m_TrackingTypeChangeMask = null;
            }
        }
    }
}
