using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Represents the UI for a dialog pop-up window. 
    /// Provides methods to configure and display the dialog in the application.
    /// </summary>
    public class XREALDialogView : MonoBehaviour
    {
        [SerializeField]
        protected Text m_TitleUI;
        [SerializeField]
        protected Text m_MessageUI;
        [SerializeField]
        protected Button m_ConfirmBtn;
        [SerializeField]
        protected float m_Duration = 5.0f;
        /// <summary>
        /// Event triggered when the confirm button is clicked.
        /// </summary>
        protected event Action OnConfirm;

        /// <summary>
        /// Event triggered when gameobject disable
        /// </summary>
        protected event Action<XREALDialogView> ClosedCallback;
        protected string m_TitleExtra;
        protected string m_MessageExtra;
        protected float m_DurationExtra;
        protected bool m_IsVisible;
        public bool IsVisible { get { return m_IsVisible; } }

        protected virtual void Start()
        {
            if (m_ConfirmBtn != null)
                m_ConfirmBtn.onClick.AddListener(OnConfirmClick);
        }

        protected virtual void OnEnable()
        {
            if (!m_IsVisible)
            {
                Show();
            }
            m_IsVisible = true;
        }

        protected virtual void OnDisable()
        {
            m_IsVisible = false;
            ClearData();
        }

        protected virtual void OnConfirmClick()
        {
            OnConfirm?.Invoke();
            HideSelf();
        }

        public virtual void Show()
        {
            if (m_IsVisible)
            {
                CancelInvoke("HideSelf");
            }
            m_IsVisible = true;
            if (m_TitleUI != null)
                m_TitleUI.text = m_TitleExtra;
            if (m_MessageUI != null)
                m_MessageUI.text = m_MessageExtra;
            if (m_DurationExtra > 0)
            {
                Invoke("HideSelf", m_DurationExtra);
            }
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Displays the UI, but enqueues it to ensure proper ordering. 
        /// This method is specifically for 3D UI and will only display 
        /// when there are no other 3D pop-ups currently active.
        /// </summary>
        public virtual void ShowInQueue()
        {
            if (XREALDialogController.Singleton != null)
            {
                XREALDialogController.Singleton.ShowInGlassesQueue(gameObject);
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Sets the content of the dialog's title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public virtual XREALDialogView SetTitle(string title)
        {
            m_TitleExtra = title;
            return this;
        }

        /// <summary>
        /// Sets the content of the dialog's message
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual XREALDialogView SetContent(string content)
        {
            m_MessageExtra = content;
            return this;
        }

        /// <summary>
        /// Sets the duration for the UI to automatically hide. 
        /// A value of 0 indicates the UI will not auto-hide.
        /// </summary>
        /// <param name="duration"> seconds </param>
        /// <returns></returns>
        public virtual XREALDialogView SetDuration(float duration)
        {
            m_DurationExtra = duration;
            return this;
        }

        /// <summary>
        /// Sets the callback action to be triggered when the confirm button is clicked.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public virtual XREALDialogView SetConfirmAction(Action callback)
        {
            OnConfirm = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback action to be triggered when the GameObject is closed.
        /// </summary>
        /// <param name="closeCallback"></param>
        /// <returns></returns>
        public virtual XREALDialogView SetCloseCallback(Action<XREALDialogView> closeCallback)
        {
            ClosedCallback = closeCallback;
            return this;
        }

        public void ClearData()
        {
            m_TitleExtra = null;
            m_MessageExtra = null;
            OnConfirm = null;
            ClosedCallback = null;
            m_DurationExtra = m_Duration;
        }

        protected virtual void HideSelf()
        {
            ClosedCallback?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
