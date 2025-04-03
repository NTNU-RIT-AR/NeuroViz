#if UNITY_INPUT_SYSTEM
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Unity.XR.XREAL
{
    [StructLayout(LayoutKind.Explicit, Size = 18)]
    internal struct XREALSimulatorControllerState : IInputStateTypeInfo
    {
        public static FourCC formatId => new FourCC('X', 'R', 'X', 'R');
        public FourCC format => formatId;

        /// <summary>
        /// Represents the primary 2D axis input from the controller,
        /// typically used for an additional joystick or touchpad.
        /// </summary>
        [InputControl(usage = "Primary2DAxis", aliases = new[] { "thumbstick", "joystick" }, offset = 0)]
        [FieldOffset(0)]
        public Vector2 primary2DAxis;

        /// <summary>
        /// Represents the secondary 2D axis input from the controller,  
        /// typically used for an additional joystick or touchpad.
        /// </summary>
        [InputControl(usage = "Secondary2DAxis", offset = 8)]
        [FieldOffset(8)]
        public Vector2 secondary2DAxis;

        /// <summary>
        /// Represents a bitmask for all button inputs on the XREAL controller.  
        /// Each bit corresponds to a specific button, as defined in <see cref="XREALButtonType"/>.
        /// </summary>
        /// <remarks>
        /// - This includes primary, secondary, grip, trigger, menu, and custom buttons.  
        /// - The bitmask allows multiple button states to be stored efficiently within a single `ushort`.  
        /// </remarks>
        [InputControl(name = nameof(XREALController.PrimaryButton), usage = "PrimaryButton", layout = "Button", bit = (uint)XREALButtonType.PrimaryButton, offset = 16)]
        [InputControl(name = nameof(XREALController.SecondaryButton), usage = "SecondaryButton", layout = "Button", bit = (uint)XREALButtonType.SecondaryButton, offset = 16)]
        [InputControl(name = nameof(XREALController.GripButton), usage = "GripButton", layout = "Button", bit = (uint)XREALButtonType.GripButton, offset = 16, alias = "gripPressed")]
        [InputControl(name = nameof(XREALController.TriggerButton), usage = "TriggerButton", layout = "Button", bit = (uint)XREALButtonType.TriggerButton, offset = 16, alias = "triggerPressed")]
        [InputControl(name = nameof(XREALController.MenuButton), usage = "MenuButton", layout = "Button", bit = (uint)XREALButtonType.MenuButton, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId0), usage = "ButtonId0", layout = "Button", bit = (uint)XREALButtonType.CustomButton0, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId1), usage = "ButtonId1", layout = "Button", bit = (uint)XREALButtonType.CustomButton1, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId2), usage = "ButtonId2", layout = "Button", bit = (uint)XREALButtonType.CustomButton2, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId3), usage = "ButtonId3", layout = "Button", bit = (uint)XREALButtonType.CustomButton3, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId4), usage = "ButtonId4", layout = "Button", bit = (uint)XREALButtonType.CustomButton4, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId5), usage = "ButtonId5", layout = "Button", bit = (uint)XREALButtonType.CustomButton5, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId6), usage = "ButtonId6", layout = "Button", bit = (uint)XREALButtonType.CustomButton6, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId7), usage = "ButtonId7", layout = "Button", bit = (uint)XREALButtonType.CustomButton7, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId8), usage = "ButtonId8", layout = "Button", bit = (uint)XREALButtonType.CustomButton8, offset = 16)]
        [InputControl(name = nameof(XREALController.ButtonId9), usage = "ButtonId9", layout = "Button", bit = (uint)XREALButtonType.CustomButton9, offset = 16)]
        [FieldOffset(16)]
        public ushort buttons;

        /// <summary>
        /// Sets or clears the state of a specific button in the controller's button bitmask.
        /// </summary>
        /// <param name="button">The button to modify, specified as an <see cref="XREALButtonType"/>.</param>
        /// <param name="state">If true, the button is set (pressed). If false, the button is cleared (released).</param>
        /// <returns>A new <see cref="XREALSimulatorControllerState"/> instance with the updated button state.</returns>
        public XREALSimulatorControllerState WithButton(XREALButtonType button, bool state = true)
        {
            var bit = 1 << (int)button;
            if (state)
                buttons |= (ushort)bit;
            else
                buttons &= (ushort)~bit;
            return this;
        }

        /// <summary>
        /// Checks if a specific button is pressed, based on its bit in the button bitmask.
        /// </summary>
        /// <param name="button">The button to check, specified as an <see cref="XREALButtonType"/>.</param>
        /// <returns>True if the specified button is pressed (bit is set), otherwise false.</returns>
        public bool HasButton(XREALButtonType button)
        {
            var bit = 1 << (int)button;
            return (buttons & bit) != 0;
        }

        /// <summary>
        /// Resets the state of the controller, clearing all input data.
        /// </summary>
        /// <remarks>
        /// This will reset the primary and secondary 2D axes to their default values (Vector2.zero), 
        /// and clear all button states (set buttons to 0).
        /// </remarks>
        public void Reset()
        {
            primary2DAxis = default;
            secondary2DAxis = default;
            buttons = default;
        }
    }
}
#endif
