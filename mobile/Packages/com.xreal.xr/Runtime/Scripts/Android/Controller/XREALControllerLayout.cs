using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Represents the layout of the controller for XREAL, including the arrangement of buttons and controls.
    /// In edit mode, the app, home, and trigger buttons are non-functional (clicking has no effect).
    /// </summary>
    public class XREALControllerLayout : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("The RectTransform for the trigger button on the controller")]
        [SerializeField]
        private RectTransform m_TriggerRect;
        [Header("The scale of the UI in the editor while running.")]
        [SerializeField]
        private Vector2 m_LocalScaleEditorRunning = new Vector2(0.5f, 0.5f);

        private RectTransform m_RectTransform;
        private Vector2 m_DefautScreenSize;

        void Start()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_RectTransform.localScale = m_LocalScaleEditorRunning;
            UpdateLayout();
        }

        void Update()
        {
            if (Screen.width != m_DefautScreenSize.x || Screen.height != m_DefautScreenSize.y)
            {
                UpdateLayout();
            }
        }

        void UpdateLayout()
        {
            m_DefautScreenSize = new Vector2(Screen.width, Screen.height);
            if (m_TriggerRect != null)
            {
                m_RectTransform.anchorMax = Vector2.one * 0.5f;
                m_RectTransform.anchorMin = Vector2.one * 0.5f;
                float triggerWidth = m_TriggerRect.sizeDelta.x * m_TriggerRect.lossyScale.x;
                float width = triggerWidth * 1.2f / m_RectTransform.lossyScale.x;
                float height = Screen.height * m_LocalScaleEditorRunning.y / m_RectTransform.lossyScale.y;
                m_RectTransform.sizeDelta = new Vector2(width, height);

                //set localPosition
                Vector2 position = Vector2.zero;
                float realWidth = width * m_RectTransform.lossyScale.x;
                float realHeight = height * m_RectTransform.lossyScale.y;
                position.x = (Screen.width * 0.5f - realWidth * 0.5f) / transform.parent.lossyScale.x;
                position.y = (realHeight * 0.5f - Screen.height * 0.5f) / transform.parent.lossyScale.y;
                m_RectTransform.localPosition = position;
            }
        }
#endif
    }
}
