#if UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace Unity.XR.XREAL
{
    [Preserve]
    [InputControlLayout(displayName = "XREAL Controller",
#if UNITY_EDITOR
        stateType = typeof(XREALSimulatorControllerState),
#endif
        commonUsages = new[] { "LeftHand", "RightHand" })]
    public class XREALController : XRControllerWithRumble
    {
        [Preserve, InputControl]
        public ButtonControl TriggerButton { get; private set; }

        [Preserve, InputControl]
        public ButtonControl GripButton { get; private set; }

        [Preserve, InputControl]
        public ButtonControl PrimaryButton { get; private set; }

        [Preserve, InputControl]
        public ButtonControl SecondaryButton { get; private set; }

        [Preserve, InputControl]
        public ButtonControl MenuButton { get; private set; }

        [Preserve, InputControl]
        public Vector2Control Primary2DAxis { get; private set; }

        [Preserve, InputControl]
        public Vector2Control Secondary2DAxis { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId0 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId1 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId2 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId3 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId4 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId5 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId6 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId7 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId8 { get; private set; }

        [Preserve, InputControl]
        public ButtonControl ButtonId9 { get; private set; }

        protected override void FinishSetup()
        {
            base.FinishSetup();

            TriggerButton = GetChildControl<ButtonControl>("TriggerButton");
            GripButton = GetChildControl<ButtonControl>("GripButton");
            PrimaryButton = GetChildControl<ButtonControl>("PrimaryButton");
            SecondaryButton = GetChildControl<ButtonControl>("SecondaryButton");
            MenuButton = GetChildControl<ButtonControl>("MenuButton");
            Primary2DAxis = GetChildControl<Vector2Control>("Primary2DAxis");
            Secondary2DAxis = GetChildControl<Vector2Control>("Secondary2DAxis");
            ButtonId0 = GetChildControl<ButtonControl>("ButtonId0");
            ButtonId1 = GetChildControl<ButtonControl>("ButtonId1");
            ButtonId2 = GetChildControl<ButtonControl>("ButtonId2");
            ButtonId3 = GetChildControl<ButtonControl>("ButtonId3");
            ButtonId4 = GetChildControl<ButtonControl>("ButtonId4");
            ButtonId5 = GetChildControl<ButtonControl>("ButtonId5");
            ButtonId6 = GetChildControl<ButtonControl>("ButtonId6");
            ButtonId7 = GetChildControl<ButtonControl>("ButtonId7");
            ButtonId8 = GetChildControl<ButtonControl>("ButtonId8");
            ButtonId9 = GetChildControl<ButtonControl>("ButtonId9");
        }
    }

    [Preserve]
    [InputControlLayout(displayName = "XREAL Hand Tracking", commonUsages = new[] { "LeftHand", "RightHand" })]
    public class XREALHandTracking : TrackedDevice
    {
        [Preserve, InputControl]
        public ButtonControl indexPressed { get; private set; }

        [Preserve, InputControl]
        public AxisControl pinchStrengthIndex { get; private set; }

        [Preserve, InputControl]
        public Vector3Control pointerPosition { get; private set; }

        [Preserve, InputControl]
        public QuaternionControl pointerRotation { get; private set; }

        protected override void FinishSetup()
        {
            base.FinishSetup();

            indexPressed = GetChildControl<ButtonControl>("indexPressed");
            pinchStrengthIndex = GetChildControl<AxisControl>("pinchStrengthIndex");
            pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
            pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");

            var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & UnityEngine.XR.InputDeviceCharacteristics.Left) != 0)
                    InputSystem.SetDeviceUsage(this, CommonUsages.LeftHand);
                else if ((deviceDescriptor.characteristics & UnityEngine.XR.InputDeviceCharacteristics.Right) != 0)
                    InputSystem.SetDeviceUsage(this, CommonUsages.RightHand);
            }
        }
    }
}
#endif
