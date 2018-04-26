using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Util
    {
        /// <summary>
        /// Determines if the value from an axis should be considered as "Held Down"
        /// based on it's value.
        /// 
        /// For example, the "Jump" axis is typically only controlled by a button press
        /// this axis should be set to spike to full very quickly, so when it does it 
        /// can be considered held.
        /// 
        /// The axis approach is more favorable than using keycodes because it allows for
        /// the use of controllers and easier rebinding (because Unity is doing it for us)
        /// </summary>
        /// <param name="axisValue">The result of Axis.GetRawValue for the axis beinqg tested</param>
        /// <returns>True if the axis is considered to be held down, false otherwise</returns>
        public static bool IsAxisDown(float axisValue)
        {
            // use some arbitrary threshold to determine that an axis is held down or not
            return axisValue >= 0.5f;
        }
        public static Vector3 ToVector3(Vector2 v)
            => new Vector3(v.x, v.y, 0);
    }
}
