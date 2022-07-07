using System.Collections.Generic;
using UnityEngine;

namespace waypoints.common
{
    public static class DebounceInput
    {
        private static readonly Dictionary<KeyCode, float> keyToLastPressTime = new Dictionary<KeyCode, float>(110);

        public static bool GetKey(KeyCode keyCode, float debounceTime = 0.3f)
        {
            var isPressed = Input.GetKey(keyCode);
            if (!isPressed)
            {
                return false;
            }
            
            var currentTime = Time.time;
            
            if (!keyToLastPressTime.ContainsKey(keyCode))
            {
                keyToLastPressTime.Add(keyCode, currentTime);
                return true;
            } 
            else if (currentTime - keyToLastPressTime[keyCode] > debounceTime)
            {
                keyToLastPressTime[keyCode] = currentTime;
                return true;
            }

            return false;
        }
    }
}