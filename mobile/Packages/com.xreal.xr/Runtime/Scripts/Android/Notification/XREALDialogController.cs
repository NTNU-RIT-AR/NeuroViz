using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Manages the display and queuing for dialog UI elements. 
    /// </summary>
    public class XREALDialogController : SingletonMonoBehaviour<XREALDialogController>
    {
        private Queue<GameObject> m_SpacePopupShowQueue = new Queue<GameObject>();

        private GameObject m_CurrentUIOnScreen;
        private GameObject m_CurrentUIInGlass;
        public GameObject CurrentUIOnScreen { get { return m_CurrentUIOnScreen; } }
        public GameObject CurrentUIInGlass { get { return m_CurrentUIInGlass; } }

        private void Update()
        {
            if (m_SpacePopupShowQueue.Count > 0 && (m_CurrentUIInGlass == null || !m_CurrentUIInGlass.activeSelf))
            {
                m_CurrentUIInGlass = m_SpacePopupShowQueue.Dequeue();
                if (m_CurrentUIInGlass != null)
                    m_CurrentUIInGlass.SetActive(true);
            }
        }

        /// <summary>
        /// Displays the specified pop-up UI on the mobile screen.
        /// </summary>
        /// <param name="popupUI"></param>
        public void ShowOnScreen(GameObject popupUI)
        {
            if (popupUI != null)
            {
                popupUI.SetActive(true);
                m_CurrentUIOnScreen = popupUI;
            }
        }

        /// <summary>
        /// Displays the specified pop-up UI in the glasses screen.
        /// </summary>
        /// <param name="popupUI"></param>
        public void ShowInGlasses(GameObject popupUI)
        {
            if (popupUI != null)
            {
                m_CurrentUIInGlass = popupUI;
                popupUI.SetActive(true);
            }
        }

        /// <summary>
        /// Adds the specified pop-up UI to the glasses display queue. 
        /// The pop-up will only be shown when there are no other pop-ups currently displayed on the glasses.
        /// </summary>
        /// <param name="popupUI"></param>
        public void ShowInGlassesQueue(GameObject popupUI)
        {
            if (popupUI != null)
                m_SpacePopupShowQueue.Enqueue(popupUI);
        }
    }
}
