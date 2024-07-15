using UnityEngine;
using UnityEngine.Events;


namespace Exoa.Touch
{
    /// <summary>This is the base class for all swiping actions.</summary>
    public abstract class SwipeBase : MonoBehaviour
    {
        public enum ModifyType
        {
            None,
            Normalize,
            Normalize4
        }

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

        /// <summary>The required angle of the swipe in degrees.
        /// 0 = Up.
        /// 90 = Right.
        /// 180 = Down.
        /// 270 = Left.</summary>
        public float RequiredAngle { set { requiredAngle = value; } get { return requiredAngle; } }
        [SerializeField] private float requiredAngle;

        /// <summary>The angle of the arc in degrees that the swipe must be inside.
        /// -1 = No requirement.
        /// 90 = Quarter circle (+- 45 degrees).
        /// 180 = Semicircle (+- 90 degrees).</summary>
        public float RequiredArc { set { requiredArc = value; } get { return requiredArc; } }
        [SerializeField] private float requiredArc = -1.0f;

        /// <summary>Called on the first frame the conditions are met.</summary>
        public LeanFingerEvent OnFinger { get { if (onFinger == null) onFinger = new LeanFingerEvent(); return onFinger; } }
        [SerializeField] public LeanFingerEvent onFinger;

        /// <summary>Should the swipe delta be modified before use?
        /// Normalize = The swipe delta magnitude/length will be set to 1.
        /// Normalize4 = The swipe delta will be + or - 1 on either the x or y axis.</summary>
        public ModifyType Modify { set { modify = value; } get { return modify; } }
        [SerializeField] private ModifyType modify;

        /// <summary>The coordinate space of the OnDelta values.</summary>
        public CoordinateType Coordinate { set { coordinate = value; } get { return coordinate; } }
        [SerializeField] private CoordinateType coordinate;

        /// <summary>The swipe delta will be multiplied by this value.</summary>
        public float Multiplier { set { multiplier = value; } get { return multiplier; } }
        [SerializeField] private float multiplier = 1.0f;

        /// <summary>Called on the first frame the conditions are met.
        /// Vector2 = The scaled swipe delta.</summary>
        public Vector2Event OnDelta { get { if (onDelta == null) onDelta = new Vector2Event(); return onDelta; } }
        [SerializeField] public Vector2Event onDelta;

        /// <summary>Called on the first frame the conditions are met.
        /// Float = The distance/magnitude/length of the swipe delta vector.</summary>
        public FloatEvent OnDistance { get { if (onDistance == null) onDistance = new FloatEvent(); return onDistance; } }
        [SerializeField] public FloatEvent onDistance;

        /// <summary>The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more information.</summary>
        public ScreenDepth ScreenDepth = new ScreenDepth(ScreenDepth.ConversionType.DepthIntercept);

        /// <summary>Called on the first frame the conditions are met.
        /// Vector3 = Start point in world space.</summary>
        public Vector3Event OnWorldFrom { get { if (onWorldFrom == null) onWorldFrom = new Vector3Event(); return onWorldFrom; } }
        [SerializeField] public Vector3Event onWorldFrom;

        /// <summary>Called on the first frame the conditions are met.
        /// Vector3 = End point in world space.</summary>
        public Vector3Event OnWorldTo { get { if (onWorldTo == null) onWorldTo = new Vector3Event(); return onWorldTo; } }
        [SerializeField] public Vector3Event onWorldTo;

        /// <summary>Called on the first frame the conditions are met.
        /// Vector3 = The vector between the start and end points in world space.</summary>
        public Vector3Event OnWorldDelta { get { if (onWorldDelta == null) onWorldDelta = new Vector3Event(); return onWorldDelta; } }
        [SerializeField] public Vector3Event onWorldDelta;

        /// <summary>Called on the first frame the conditions are met.
        /// Vector3 = Start point in world space.
        /// Vector3 = End point in world space.</summary>
        public Vector3Vector3Event OnWorldFromTo { get { if (onWorldFromTo == null) onWorldFromTo = new Vector3Vector3Event(); return onWorldFromTo; } }
        [SerializeField] public Vector3Vector3Event onWorldFromTo;

        protected bool AngleIsValid(Vector2 vector)
        {
            if (requiredArc >= 0.0f)
            {
                var angle = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
                var angleDelta = Mathf.DeltaAngle(angle, requiredAngle);

                if (angleDelta < requiredArc * -0.5f || angleDelta >= requiredArc * 0.5f)
                {
                    return false;
                }
            }

            return true;
        }

        protected void HandleFingerSwipe(TouchFinger finger, Vector2 screenFrom, Vector2 screenTo)
        {
            var finalDelta = screenTo - screenFrom;

            if (AngleIsValid(finalDelta) == true)
            {
                if (onFinger != null)
                {
                    onFinger.Invoke(finger);
                }

                switch (coordinate)
                {
                    case CoordinateType.ScaledPixels: finalDelta *= InputTouch.ScalingFactor; break;
                    case CoordinateType.ScreenPercentage: finalDelta *= InputTouch.ScreenFactor; break;
                }

                switch (modify)
                {
                    case ModifyType.Normalize:
                        {
                            finalDelta = finalDelta.normalized;
                        }
                        break;

                    case ModifyType.Normalize4:
                        {
                            if (finalDelta.x < -Mathf.Abs(finalDelta.y)) finalDelta = -Vector2.right;
                            if (finalDelta.x > Mathf.Abs(finalDelta.y)) finalDelta = Vector2.right;
                            if (finalDelta.y < -Mathf.Abs(finalDelta.x)) finalDelta = -Vector2.up;
                            if (finalDelta.y > Mathf.Abs(finalDelta.x)) finalDelta = Vector2.up;
                        }
                        break;
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

                var worldFrom = ScreenDepth.Convert(screenFrom, gameObject);
                var worldTo = ScreenDepth.Convert(screenTo, gameObject);

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
}
