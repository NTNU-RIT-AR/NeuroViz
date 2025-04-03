using System.Collections;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// This class is used to show the mask when the tracking type is changing.
    /// </summary>
    public class XREALTrackingTypeChangeMask : MonoBehaviour
    {
        [SerializeField]
        SpriteRenderer m_FullScreenMask;
        [SerializeField]
        AnimationCurve m_AnimationCurve;
        Coroutine m_FadeCoroutine;

        void Awake()
        {
            if (XREALUtility.MainCamera != null)
            {
                var centerAnchor = XREALUtility.MainCamera.transform;
                transform.SetParent(centerAnchor, false);
                transform.SetLocalPositionAndRotation(Vector3.forward * 3, Quaternion.identity);
            }
            m_FullScreenMask.sortingOrder = short.MaxValue;
        }

        /// <summary>
        /// Show the mask.
        /// </summary>
        public void Show()
        {
            if (m_FadeCoroutine != null)
            {
                StopCoroutine(m_FadeCoroutine);
            }
            m_FadeCoroutine = StartCoroutine(Fade(1, 0));
        }

        /// <summary>
        /// Hide the mask.
        /// </summary>
        public void Hide()
        {
            if (m_FadeCoroutine != null)
            {
                StopCoroutine(m_FadeCoroutine);
            }
            m_FadeCoroutine = StartCoroutine(Fade(0, 1));
        }

        IEnumerator Fade(float endAlpha, float fadeTime)
        {
            m_FullScreenMask.enabled = true;
            m_FullScreenMask.sharedMaterial.color = new Color(0f, 0f, 0f, 1.0f - endAlpha);
            var timeElapse = 0f;
            while (timeElapse <= fadeTime)
            {
                timeElapse += Time.deltaTime;
                float percent = timeElapse / fadeTime;
                percent = Mathf.Clamp(percent, 0, 1);
                percent = m_AnimationCurve.Evaluate(percent);
                percent = Mathf.Lerp(1.0f - endAlpha, endAlpha, percent);
                m_FullScreenMask.sharedMaterial.color = new Color(0f, 0f, 0f, percent);
                yield return null;
            }
            if (endAlpha == 0)
                m_FullScreenMask.enabled = false;
            m_FadeCoroutine = null;
        }
    }
}
