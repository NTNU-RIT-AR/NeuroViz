using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.XR.XREAL
{
    public static class MotionEvent
    {
        public const int ACT_DOWN = 0;
        public const int ACT_UP = 1;
        public const int ACT_MOVE = 2;
        public const int ACT_HOVER_ENTER = 9;
        public const int ACT_HOVER_EXIT = 10;
        public const int ACT_HOVER_MOVE = 7;
    }

    /// <summary>
    /// A component that handles pointer input events for a Quad object, including pointer down, up, hover, and scroll events.
    /// This class provides callbacks for these events. The position of the pointer is translated into the local space of the Quad,
    /// and custom actions are triggered based on the pointer's state.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class QuadPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
    {
        PointerEventData m_DownEventData = null;
        PointerEventData m_HoverEventData = null;
        public Action<Vector2, int> OnPointerEvent;
        public Action<Vector2, Vector2> OnScrollEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
            m_DownEventData = eventData;
            OnPointerEvent?.Invoke(GetLocalPointerPosition(eventData), MotionEvent.ACT_DOWN);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerEvent?.Invoke(GetLocalPointerPosition(eventData), MotionEvent.ACT_UP);
            m_DownEventData = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_HoverEventData = eventData;
            OnPointerEvent?.Invoke(GetLocalPointerPosition(eventData), MotionEvent.ACT_HOVER_ENTER);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerEvent?.Invoke(GetLocalPointerPosition(eventData), MotionEvent.ACT_HOVER_EXIT);
            m_HoverEventData = null;
        }

        void Update()
        {
            if (m_DownEventData != null)
            {
                OnPointerEvent?.Invoke(GetLocalPointerPosition(m_DownEventData), MotionEvent.ACT_MOVE);
            }
            if (m_HoverEventData != null)
            {
                OnPointerEvent?.Invoke(GetLocalPointerPosition(m_HoverEventData), MotionEvent.ACT_HOVER_MOVE);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            OnScrollEvent?.Invoke(GetLocalPointerPosition(eventData), eventData.scrollDelta);
        }

        protected virtual Vector2 GetLocalPointerPosition(PointerEventData eventData)
        {
            return transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
        }
    }
}
