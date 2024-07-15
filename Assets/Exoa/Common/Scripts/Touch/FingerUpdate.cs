using UnityEngine;
using UnityEngine.Events;
using Exoa.Common;


namespace Exoa.Touch
{
    /// <summary>This component allows you to detect when a finger is touching the screen.</summary>

    [AddComponentMenu("Exoa/Touch/Finger Update")]
    public class FingerUpdate : MonoBehaviour
    {
        public enum CoordinateType
        {
            ScaledPixels,
            ScreenPixels,
            ScreenPercentage
        }

        [System.Serializable] public class LeanFingerEvent : UnityEvent<TouchFinger> { }
        [System.Serializable] public class FloatEvent : UnityEvent<float> { }
        [System.Serializable] public class Vector2Event : UnityEvent<Vector2> { }
        [System.Serializable] public class Vector3Event : UnityEvent<Vector3> { }
        [System.Serializable] public class Vector3Vector3Event : UnityEvent<Vector3, Vector3> { }

        /// <summary>Ignore fingers with StartedOverGui?</summary>
        public bool IgnoreStartedOverGui { set { ignoreStartedOverGui = value; } get { return ignoreStartedOverGui; } }
        [SerializeField] private bool ignoreStartedOverGui = true;

        /// <summary>Ignore fingers with OverGui?</summary>
        public bool IgnoreIsOverGui { set { ignoreIsOverGui = value; } get { return ignoreIsOverGui; } }
        [SerializeField] private bool ignoreIsOverGui;

        /// <summary>If the finger didn't move, ignore it?</summary>
        public bool IgnoreIfStatic { set { ignoreIfStatic = value; } get { return ignoreIfStatic; } }
        [SerializeField] private bool ignoreIfStatic;

        /// <summary>If the finger just began touching the screen, ignore it?</summary>
        public bool IgnoreIfDown { set { ignoreIfDown = value; } get { return ignoreIfDown; } }
        [SerializeField] private bool ignoreIfDown;

        /// <summary>If the finger just stopped touching the screen, ignore it?</summary>
        public bool IgnoreIfUp { set { ignoreIfUp = value; } get { return ignoreIfUp; } }
        [SerializeField] private bool ignoreIfUp;

        /// <summary>If the finger is the mouse hover, ignore it?</summary>
        public bool IgnoreIfHover { set { ignoreIfHover = value; } get { return ignoreIfHover; } }
        [SerializeField] private bool ignoreIfHover = true;

        /// <summary>If the specified object is set and isn't selected, then this component will do nothing.</summary>
        public TouchSelectable RequiredSelectable { set { requiredSelectable = value; } get { return requiredSelectable; } }
        [SerializeField] private TouchSelectable requiredSelectable;

        /// <summary>Called on every frame the conditions are met.</summary>
        public LeanFingerEvent OnFinger { get { if (onFinger == null) onFinger = new LeanFingerEvent(); return onFinger; } }
        [SerializeField] private LeanFingerEvent onFinger;

        /// <summary>The coordinate space of the OnDelta values.</summary>
        public CoordinateType Coordinate { set { coordinate = value; } get { return coordinate; } }
        [SerializeField] private CoordinateType coordinate;

        /// <summary>The delta values will be multiplied by this when output.</summary>
        public float Multiplier { set { multiplier = value; } get { return multiplier; } }
        [SerializeField] private float multiplier = 1.0f;

        /// <summary>This event is invoked when the requirements are met.
        /// Vector2 = Position Delta based on your Coordinates setting.</summary>
        public Vector2Event OnDelta { get { if (onDelta == null) onDelta = new Vector2Event(); return onDelta; } }
        [SerializeField] private Vector2Event onDelta;

        /// <summary>Called on the first frame the conditions are met.
        /// Float = The distance/magnitude/length of the swipe delta vector.</summary>
        public FloatEvent OnDistance { get { if (onDistance == null) onDistance = new FloatEvent(); return onDistance; } }
        [SerializeField] private FloatEvent onDistance;

        /// <summary>The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more information.</summary>
        public ScreenDepth ScreenDepth = new ScreenDepth(ScreenDepth.ConversionType.DepthIntercept);

        /// <summary>Called on the first frame the conditions are met.
        /// Vector3 = Start point in world space.</summary>
        public Vector3Event OnWorldFrom { get { if (onWorldFrom == null) onWorldFrom = new Vector3Event(); return onWorldFrom; } }
        [SerializeField] private Vector3Event onWorldFrom;

        /// <summary>Called on the first frame the conditions are met.
        /// Vector3 = End point in world space.</summary>
        public Vector3Event OnWorldTo { get { if (onWorldTo == null) onWorldTo = new Vector3Event(); return onWorldTo; } }
        [SerializeField] private Vector3Event onWorldTo;

        /// <summary>Called on the first frame the conditions are met.
        /// Vector3 = The vector between the start and end points in world space.</summary>
        public Vector3Event OnWorldDelta { get { if (onWorldDelta == null) onWorldDelta = new Vector3Event(); return onWorldDelta; } }
        [SerializeField] private Vector3Event onWorldDelta;

        /// <summary>Called on the first frame the conditions are met.
        /// Vector3 = Start point in world space.
        /// Vector3 = End point in world space.</summary>
        public Vector3Vector3Event OnWorldFromTo { get { if (onWorldFromTo == null) onWorldFromTo = new Vector3Vector3Event(); return onWorldFromTo; } }
        [SerializeField] private Vector3Vector3Event onWorldFromTo;

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
            InputTouch.OnFingerUpdate += HandleFingerUpdate;
        }

        protected virtual void OnDisable()
        {
            InputTouch.OnFingerUpdate -= HandleFingerUpdate;
        }

        private void HandleFingerUpdate(TouchFinger finger)
        {
            if (ignoreStartedOverGui == true && finger.StartedOverGui == true)
            {
                return;
            }

            if (ignoreIsOverGui == true && finger.IsOverGui == true)
            {
                return;
            }

            if (ignoreIfStatic == true && finger.ScreenDelta.magnitude <= 0.0f)
            {
                return;
            }

            if (ignoreIfDown == true && finger.Down == true)
            {
                return;
            }

            if (ignoreIfUp == true && finger.Up == true)
            {
                return;
            }

            if (ignoreIfHover == true && finger.Index == InputTouch.HOVER_FINGER_INDEX)
            {
                return;
            }

            if (requiredSelectable != null && requiredSelectable.IsSelected == false)
            {
                return;
            }

            if (onFinger != null)
            {
                onFinger.Invoke(finger);
            }

            var finalDelta = finger.ScreenDelta;

            switch (coordinate)
            {
                case CoordinateType.ScaledPixels: finalDelta *= InputTouch.ScalingFactor; break;
                case CoordinateType.ScreenPercentage: finalDelta *= InputTouch.ScreenFactor; break;
            }

            finalDelta *= multiplier;

            if (onDelta != null)
            {
                onDelta.Invoke(finalDelta);
            }

            if (onDistance != null)
            {
                onDistance.Invoke(finalDelta.magnitude);
            }

            var worldFrom = ScreenDepth.Convert(finger.LastScreenPosition, gameObject);
            var worldTo = ScreenDepth.Convert(finger.ScreenPosition, gameObject);

            if (onWorldFrom != null)
            {
                onWorldFrom.Invoke(worldFrom);
            }

            if (onWorldTo != null)
            {
                onWorldTo.Invoke(worldTo);
            }

            if (onWorldDelta != null)
            {
                onWorldDelta.Invoke(worldTo - worldFrom);
            }

            if (onWorldFromTo != null)
            {
                onWorldFromTo.Invoke(worldFrom, worldTo);
            }
        }
    }
}
