#if UNITY_EDITOR && UNITY_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// xreal controller sumulator
    /// </summary>
    public class XREALControllerSimulator : SingletonMonoBehaviour<XREALControllerSimulator>
    {
        private XREALSimulatorControllerState m_RightControllerState;
        private XREALController m_RightXREALController;
        private bool m_Touching = false;
        private Vector2 m_TouchStartPos = Vector2.zero;

        protected override void Awake()
        {
            base.Awake();
            m_RightXREALController = InputSystem.AddDevice<XREALController>();
            InputSystem.SetDeviceUsage(m_RightXREALController, CommonUsages.RightHand);
        }

        protected override void OnDestroy()
        {
            InputSystem.RemoveDevice(m_RightXREALController);
            base.OnDestroy();
        }

        internal void UpdateButton(XREALButtonType buttonType, bool appendButton)
        {
            m_RightControllerState = m_RightControllerState.WithButton(buttonType, appendButton);
            InputState.Change(m_RightXREALController, m_RightControllerState);
        }

        internal void SendHapticAxis(XREALButtonType buttonType, float touchX, float touchY)
        {
            if (!m_Touching)
            {
                m_TouchStartPos.x = touchX;
                m_TouchStartPos.y = touchY;
                m_Touching = true;
            }
            float deltaX = Mathf.Clamp(touchX - m_TouchStartPos.x, -1, 1);
            float deltaY = Mathf.Clamp(touchY - m_TouchStartPos.y, -1, 1);
            switch (buttonType)
            {
                case XREALButtonType.Primary2DAxis:
                    m_RightControllerState.primary2DAxis.x = deltaX;
                    m_RightControllerState.primary2DAxis.y = deltaY;
                    break;
                case XREALButtonType.Secondary2DAxis:
                    m_RightControllerState.secondary2DAxis.x = deltaX;
                    m_RightControllerState.secondary2DAxis.y = deltaY;
                    break;
            }
            InputState.Change(m_RightXREALController, m_RightControllerState);
        }

        internal void SendHapticAxisEnd(XREALButtonType buttonType)
        {
            m_Touching = false;
            switch (buttonType)
            {
                case XREALButtonType.Primary2DAxis:
                    m_RightControllerState.primary2DAxis.x = 0;
                    m_RightControllerState.primary2DAxis.y = 0;
                    break;
                case XREALButtonType.Secondary2DAxis:
                    m_RightControllerState.secondary2DAxis.x = 0;
                    m_RightControllerState.secondary2DAxis.y = 0;
                    break;
            }
            InputState.Change(m_RightXREALController, m_RightControllerState);
        }
    }
}
#endif
