using UnityEngine;
using System.Collections.Generic;

namespace Exoa.Touch
{
    /// <summary>This component tells you when a finger finishes touching the screen. The finger must begin touching the screen with the specified the specified conditions for it to be considered.</summary>

    [AddComponentMenu("Exoa/Touch/Finger Up")]
    public class FingerUp : FingerDown
    {
        /// <summary>Ignore fingers with OverGui?</summary>
        public bool IgnoreIsOverGui { set { ignoreIsOverGui = value; } get { return ignoreIsOverGui; } }
        [SerializeField] private bool ignoreIsOverGui;

        private List<TouchFinger> fingers = new List<TouchFinger>();

        protected override void OnEnable()
        {
            InputTouch.OnFingerDown += HandleFingerDown;
            InputTouch.OnFingerUp += HandleFingerUp;
        }

        protected override void OnDisable()
        {
            InputTouch.OnFingerDown -= HandleFingerDown;
            InputTouch.OnFingerUp -= HandleFingerUp;
        }

        protected override bool UseFinger(TouchFinger finger)
        {
            if (ignoreIsOverGui == true && finger.IsOverGui == true)
            {
                return false;
            }

            return base.UseFinger(finger);
        }

        protected override void HandleFingerDown(TouchFinger finger)
        {
            if (UseFinger(finger) == true)
            {
                fingers.Add(finger);
            }
        }

        protected virtual void HandleFingerUp(TouchFinger finger)
        {
            if (fingers.Remove(finger) == true)
            {
                InvokeFinger(finger);
            }
        }
    }
}
