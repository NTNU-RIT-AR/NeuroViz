using UnityEngine;
using UnityEngine.UI;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// A singleton MonoBehaviour class that manages the XREAL home menu. 
    /// It handles the visibility of the home menu.
    /// </summary>
    public class XREALHomeMenu : SingletonMonoBehaviour<XREALHomeMenu>
    {
        [SerializeField]
        Button m_ConfirmBtn;
        [SerializeField]
        Button m_CancelBtn;

        void Start()
        {
            m_ConfirmBtn.onClick.AddListener(OnComfirmButtonClick);
            m_CancelBtn.onClick.AddListener(OnCancelButtonClick);
        }

        void OnCancelButtonClick()
        {
            Show(false);
        }

        void OnComfirmButtonClick()
        {
            Show(false);
            XREALPlugin.QuitApplication();
        }

        public void Toggle()
        {
            Show(!gameObject.activeSelf);
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }
    }
}
