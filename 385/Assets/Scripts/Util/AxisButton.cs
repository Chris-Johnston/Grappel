using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// A wrapper for an axis that enables it to act as a button
    /// </summary>
    public class AxisButton
    {
        private const bool DEBUG = false;

        /// <summary>
        /// The name of the axis to watch
        /// </summary>
        public string AxisName { get; set; }

        /// <summary>
        /// At what point is the button considered to be held down.
        /// </summary>
        public float ActivationThreshold { get; set; }

        /// <summary>
        /// The current state of the button
        /// True is pressed, false is not pressed
        /// </summary>
        public bool CurrentState { get; private set; }

        /// <summary>
        /// The state of the button in the last update call
        /// True is pressed, false is not pressed
        /// </summary>
        public bool LastPolledState { get; private set; }

        /// <summary>
        /// AxisButton
        /// Wrapper for an axis that allows it to behave like a button
        /// Only deals with the positive axis, 0-1
        /// </summary>
        /// <param name="name">Which axis name to monitor</param>
        /// <param name="threshold">What value determines if an axis is held down or not</param>
        public AxisButton(string name, float threshold = 0.5f)
        {
            AxisName = name;
            ActivationThreshold = threshold;
        }

        /// <summary>
        /// Polls the axis and updates the status 
        /// </summary>
        public void Update()
        {
            // update the prev update call's value
            LastPolledState = CurrentState;

            // poll the input to get the current state of the axis
            var axisValue = Input.GetAxis(AxisName);

            CurrentState = axisValue >= ActivationThreshold;

            // ignore this warning
#pragma warning disable CS0162 // Unreachable code detected
            if (DEBUG)
            {
                Debug.Log($"Axis: {AxisName} Value: {axisValue} State: {CurrentState}");
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        /// <summary>
        /// Did the user just click the button
        /// When the button transitions from being unpressed to pressed
        /// </summary>
        /// <returns></returns>
        public bool IsButtonClicked()
            => !LastPolledState && CurrentState;

        /// <summary>
        /// Is the user holding down the button
        /// when the button is held down
        /// </summary>
        /// <returns></returns>
        public bool IsButtonHeld()
            => CurrentState;

        /// <summary>
        /// Did the user just release the button
        /// when the button transitions from being pressed to unpressed
        /// </summary>
        /// <returns></returns>
        public bool IsButtonReleased()
            => LastPolledState && !CurrentState;
    }
}
