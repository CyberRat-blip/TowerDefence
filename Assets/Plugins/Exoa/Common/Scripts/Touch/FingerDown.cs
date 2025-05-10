using UnityEngine;
using UnityEngine.Events;
using Exoa.Common;

namespace Exoa.Touch
{
    /// <summary>This component tells you when a finger begins touching the screen, as long as it satisfies the specified conditions.</summary>

    [AddComponentMenu("Exoa/Touch/Finger Down")]
    public class FingerDown : MonoBehaviour
    {
        [System.Serializable] public class LeanFingerEvent : UnityEvent<TouchFinger> { }
        [System.Serializable] public class Vector3Event : UnityEvent<Vector3> { }
        [System.Serializable] public class Vector2Event : UnityEvent<Vector2> { }

        [System.Flags]
        public enum ButtonTypes
        {
            LeftMouse = 1 << 0,
            RightMouse = 1 << 1,
            MiddleMouse = 1 << 2,
            Touch = 1 << 5
        }

        /// <summary>Ignore fingers with StartedOverGui?</summary>
        public bool IgnoreStartedOverGui { set { ignoreStartedOverGui = value; } get { return ignoreStartedOverGui; } }
        [SerializeField] private bool ignoreStartedOverGui = true;

        /// <summary>Which inputs should this component react to?</summary>
        public ButtonTypes RequiredButtons { set { requiredButtons = value; } get { return requiredButtons; } }
        [SerializeField] private ButtonTypes requiredButtons = (ButtonTypes)~0;

        /// <summary>If the specified object is set and isn't selected, then this component will do nothing.</summary>
        public TouchSelectable RequiredSelectable { set { requiredSelectable = value; } get { return requiredSelectable; } }
        [SerializeField] private TouchSelectable requiredSelectable;

        /// <summary>This event will be called if the above conditions are met when your finger begins touching the screen.</summary>
        public LeanFingerEvent OnFinger { get { if (onFinger == null) onFinger = new LeanFingerEvent(); return onFinger; } }
        [SerializeField] private LeanFingerEvent onFinger;

        /// <summary>The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more information.</summary>
        public ScreenDepth ScreenDepth = new ScreenDepth(ScreenDepth.ConversionType.DepthIntercept);

        /// <summary>This event will be called if the above conditions are met when your finger begins touching the screen.
        /// Vector3 = Start point based on the ScreenDepth settings.</summary>
        public Vector3Event OnWorld { get { if (onWorld == null) onWorld = new Vector3Event(); return onWorld; } }
        [SerializeField] private Vector3Event onWorld;

        /// <summary>This event will be called if the above conditions are met when your finger begins touching the screen.
        /// Vector2 = Finger position in screen space.</summary>
        public Vector2Event OnScreen { get { if (onScreen == null) onScreen = new Vector2Event(); return onScreen; } }
        [SerializeField] private Vector2Event onScreen;

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            requiredSelectable = GetComponentInParent<TouchSelectable>();
        }
#endif

        protected virtual void Awake()
        {
            if (requiredSelectable == null)
            {
                requiredSelectable = GetComponentInParent<TouchSelectable>();
            }
        }

        protected virtual void OnEnable()
        {
            InputTouch.OnFingerDown += HandleFingerDown;
        }

        protected virtual void OnDisable()
        {
            InputTouch.OnFingerDown -= HandleFingerDown;
        }

        protected virtual bool UseFinger(TouchFinger finger)
        {
            if (ignoreStartedOverGui == true && finger.IsOverGui == true)
            {
                return false;
            }

            if (RequiredButtonPressed(finger) == false)
            {
                return false;
            }

            if (requiredSelectable != null && requiredSelectable.IsSelected == false)
            {
                return false;
            }

            if (finger.Index == InputTouch.HOVER_FINGER_INDEX)
            {
                return false;
            }

            return true;
        }

        protected void InvokeFinger(TouchFinger finger)
        {
            if (onFinger != null)
            {
                onFinger.Invoke(finger);
            }

            if (onWorld != null)
            {
                var position = ScreenDepth.Convert(finger.ScreenPosition, gameObject);

                onWorld.Invoke(position);
            }

            if (onScreen != null)
            {
                onScreen.Invoke(finger.ScreenPosition);
            }
        }

        protected virtual void HandleFingerDown(TouchFinger finger)
        {
            if (UseFinger(finger) == true)
            {
                InvokeFinger(finger);
            }
        }

        private bool RequiredButtonPressed(TouchFinger finger)
        {
            if (finger.Index < 0)
            {
                if (BaseTouchInput.GetMouseExists() == true)
                {
                    if ((requiredButtons & ButtonTypes.LeftMouse) != 0 && BaseTouchInput.GetMouseIsHeld(0) == true)
                    {
                        return true;
                    }

                    if ((requiredButtons & ButtonTypes.RightMouse) != 0 && BaseTouchInput.GetMouseIsHeld(1) == true)
                    {
                        return true;
                    }

                    if ((requiredButtons & ButtonTypes.MiddleMouse) != 0 && BaseTouchInput.GetMouseIsHeld(2) == true)
                    {
                        return true;
                    }
                }
            }
            else if ((requiredButtons & ButtonTypes.Touch) != 0)
            {
                return true;
            }

            return false;
        }
    }
}
