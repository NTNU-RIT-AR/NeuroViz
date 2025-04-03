using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// An enum representing different types of buttons for the XREAL controller. 
    /// It includes standard buttons like Trigger, Grip, and Menu buttons, as well as custom and 2D axis buttons.
    /// </summary>
    public enum XREALButtonType
    {
        /// <summary> No button pressed or an undefined state. </summary>
        None,
        /// <summary> The trigger button, typically used for selecting actions. </summary>
        TriggerButton,
        /// <summary> The grip button, often used for grabbing or holding objects. </summary>
        GripButton,
        /// <summary> The primary action button, usually mapped to "A" or "X" on the controller. </summary>
        PrimaryButton,
        /// <summary> The secondary action button, typically mapped to "B" or "Y" on the controller. </summary>
        SecondaryButton,
        /// <summary> The menu button, generally used for opening in-game menus or system settings. </summary>
        MenuButton,
        /// <summary> The primary 2D axis input, often linked to the touchpad. </summary>
        Primary2DAxis,
        /// <summary> The secondary 2D axis input </summary>
        Secondary2DAxis,
        CustomButton0,
        CustomButton1,
        CustomButton2,
        CustomButton3,
        CustomButton4,
        CustomButton5,
        CustomButton6,
        CustomButton7,
        CustomButton8,
        CustomButton9,
    }

    /// <summary>
    /// A component that maps pointer events to corresponding controller button events based on the button type,
    /// and sends the controller button events to the XREAL Controller.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class XREALButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        XREALButtonType m_ButtonType;

        public bool IsTouchPad => m_ButtonType == XREALButtonType.Primary2DAxis
                               || m_ButtonType == XREALButtonType.Secondary2DAxis;

        public bool IsButton => m_ButtonType != XREALButtonType.None
                             && m_ButtonType != XREALButtonType.Primary2DAxis
                             && m_ButtonType != XREALButtonType.Secondary2DAxis;

        RectTransform m_Rect;

        void Start()
        {
            m_Rect = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsButton)
            {
                XREALVirtualController.Singleton.SendHapticButton(m_ButtonType, true);
            }
            if (IsTouchPad)
            {
                SendTouchPos(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsButton)
            {
                XREALVirtualController.Singleton.SendHapticButton(m_ButtonType, false);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsTouchPad)
            {
                SendTouchPos(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (IsTouchPad)
            {
                XREALVirtualController.Singleton.SendHapticAxisEnd(m_ButtonType);
            }
        }

        private void SendTouchPos(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Rect, eventData.position, eventData.pressEventCamera, out Vector2 localPosition);
            float xNormalized = Mathf.Clamp(localPosition.x / (m_Rect.rect.width / 2), -1f, 1f);
            float yNormalized = Mathf.Clamp(localPosition.y / (m_Rect.rect.height / 2), -1f, 1f);
            XREALVirtualController.Singleton.SendHapticAxis(m_ButtonType, xNormalized, yNormalized);
        }
    }
}
