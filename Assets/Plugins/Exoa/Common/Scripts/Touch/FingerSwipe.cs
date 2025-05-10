using UnityEngine;
using Exoa.Common;

namespace Exoa.Touch
{
    /// <summary>This component fires events if a finger has swiped across the screen.
    /// A swipe is defined as a touch that began and ended within the LeanTouch.TapThreshold time, and moved more than the LeanTouch.SwipeThreshold distance.</summary>

    [AddComponentMenu("Exoa/Touch/Finger Swipe")]
    public class FingerSwipe : SwipeBase
    {
        /// <summary>Ignore fingers with StartedOverGui?</summary>
        public bool IgnoreStartedOverGui { set { ignoreStartedOverGui = value; } get { return ignoreStartedOverGui; } }
        [SerializeField] private bool ignoreStartedOverGui = true;

        /// <summary>Ignore fingers with OverGui?</summary>
        public bool IgnoreIsOverGui { set { ignoreIsOverGui = value; } get { return ignoreIsOverGui; } }
        [SerializeField] private bool ignoreIsOverGui;

        /// <summary>If the specified object is set and isn't selected, then this component will do nothing.</summary>
        public TouchSelectable RequiredSelectable { set { requiredSelectable = value; } get { return requiredSelectable; } }
        [SerializeField] private TouchSelectable requiredSelectable;

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            requiredSelectable = GetComponentInParent<TouchSelectable>();
        }
#endif

        protected virtual void Start()
        {
            if (requiredSelectable == null)
            {
                requiredSelectable = GetComponentInParent<TouchSelectable>();
            }
        }

        protected virtual void OnEnable()
        {
            InputTouch.OnFingerSwipe += HandleFingerSwipe;
        }

        protected virtual void OnDisable()
        {
            InputTouch.OnFingerSwipe -= HandleFingerSwipe;
        }

        private void HandleFingerSwipe(TouchFinger finger)
        {
            if (ignoreStartedOverGui == true && finger.StartedOverGui == true)
            {
                return;
            }

            if (ignoreIsOverGui == true && finger.IsOverGui == true)
            {
                return;
            }

            if (requiredSelectable != null && requiredSelectable.IsSelected == false)
            {
                return;
            }

            HandleFingerSwipe(finger, finger.StartScreenPosition, finger.ScreenPosition);
        }
    }
}
