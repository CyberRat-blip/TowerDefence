using System.Runtime.InteropServices;
using UnityEngine;


namespace Exoa.Maths
{
    [System.Serializable]
    public class Springs
    {

        [Range(0f, 5f)]
        public float halfLife = .1f;
        public float distanceFromTargetToComplete = 0.01f;
        //[Range(0f, 100f)]
        //private float frequency = 8f;
        //[Range(0f, 100f)]
        //private float angularFrequency = 8f;



        public float Update(ref FloatSpring spring, float target, System.Action onComplete = null)
        {
            var result = spring.TrackExponential(target, halfLife, Time.unscaledDeltaTime);
            float distance = Mathf.Abs(result - target);

            if (distance <= distanceFromTargetToComplete && !spring.Completed)
            {
                spring.Completed = true;
                onComplete?.Invoke();
            }
            else if (distance > distanceFromTargetToComplete)
            {
                spring.Completed = false;
            }
            return result;
        }

        public Vector2 Update(ref Vector2Spring spring, Vector2 target, System.Action onComplete = null)
        {
            var result = spring.TrackExponential(target, halfLife, Time.unscaledDeltaTime);
            float distance = Vector2.Distance(result, target);
            if (distance <= distanceFromTargetToComplete && !spring.Completed)
            {
                spring.Completed = true;
                onComplete?.Invoke();
            }
            else if (distance > distanceFromTargetToComplete)
            {
                spring.Completed = false;
            }
            return result;
        }

        public Vector3 Update(ref Vector3Spring spring, Vector3 target, System.Action onComplete = null)
        {
            var result = spring.TrackExponential(target, halfLife, Time.unscaledDeltaTime);
            float distance = Vector3.Distance(result, target);
            if (distance <= distanceFromTargetToComplete && !spring.Completed)
            {
                spring.Completed = true;
                onComplete?.Invoke();
            }
            else if (distance > distanceFromTargetToComplete)
            {
                spring.Completed = false;
            }
            return result;
        }

        public Vector4 Update(ref Vector4Spring spring, Vector4 target, System.Action onComplete = null)
        {
            var result = spring.TrackExponential(target, halfLife, Time.unscaledDeltaTime);
            float distance = Vector4.Distance(result, target);
            if (distance <= distanceFromTargetToComplete && !spring.Completed)
            {
                spring.Completed = true;
                onComplete?.Invoke();
            }
            else if (distance > distanceFromTargetToComplete)
            {
                spring.Completed = false;
            }
            return result;
        }

        public Quaternion Update(ref QuaternionSpring spring, Quaternion target, System.Action onComplete = null)
        {
            var result = spring.TrackExponential(target, halfLife, Time.unscaledDeltaTime);

            float distance = Quaternion.Angle(result, target);
            if (distance <= distanceFromTargetToComplete && !spring.Completed)
            {
                spring.Completed = true;
                onComplete?.Invoke();
            }
            else if (distance > distanceFromTargetToComplete)
            {
                spring.Completed = false;
            }
            return result;
        }
    }

    public static class ColorExt
    {
        public static Color ToColor(this Vector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }
        public static Vector4 ToVector4(this Color v)
        {
            return new Vector4(v.r, v.g, v.b, v.a);
        }
    }



    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct FloatSpring
    {
        public static readonly int Stride = 2 * sizeof(float);

        public float Value;
        public float Velocity;
        private bool completed;

        public bool Completed { get => completed; set => completed = value; }

        public void Reset()
        {
            Value = 0.0f;
            Velocity = 0.0f;
            completed = false;
        }

        public void Reset(float initValue)
        {
            Value = initValue;
            Velocity = 0.0f;
            completed = false;
        }

        public void Reset(float initValue, float initVelocity)
        {
            Value = initValue;
            Velocity = initVelocity;
            completed = false;
        }

        public float TrackDampingRatio(float targetValue, float angularFrequency, float dampingRatio, float deltaTime)
        {
            if (angularFrequency < MathUtil.Epsilon)
            {
                Velocity = 0.0f;
                return Value;
            }

            float delta = targetValue - Value;

            float f = 1.0f + 2.0f * deltaTime * dampingRatio * angularFrequency;
            float oo = angularFrequency * angularFrequency;
            float hoo = deltaTime * oo;
            float hhoo = deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            float detX = f * Value + deltaTime * Velocity + hhoo * targetValue;
            float detV = Velocity + hoo * delta;

            Velocity = detV * detInv;
            Value = detX * detInv;

            if (Velocity < MathUtil.Epsilon && Mathf.Abs(delta) < MathUtil.Epsilon)
            {
                Velocity = 0.0f;
                Value = targetValue;
            }

            return Value;
        }

        public float TrackHalfLife(float targetValue, float frequencyHz, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                Velocity = 0.0f;
                Value = targetValue;
                return Value;
            }

            float angularFrequency = frequencyHz * MathUtil.TwoPi;
            float dampingRatio = 0.6931472f / (angularFrequency * halfLife);
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }

        public float TrackExponential(float targetValue, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                Velocity = 0.0f;
                Value = targetValue;
                return Value;
            }

            float angularFrequency = 0.6931472f / halfLife;
            float dampingRatio = 1.0f;
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Vector2Spring
    {
        public static readonly int Stride = 4 * sizeof(float);

        public Vector2 Value;
        public Vector2 Velocity;
        private bool completed;

        public bool Completed { get => completed; set => completed = value; }

        public void Reset()
        {
            Value = Vector2.zero;
            Velocity = Vector2.zero;
            completed = false;
        }

        public void Reset(Vector2 initValue)
        {
            Value = initValue;
            Velocity = Vector2.zero;
            completed = false;
        }

        public void Reset(Vector2 initValue, Vector2 initVelocity)
        {
            Value = initValue;
            Velocity = initVelocity;
            completed = false;
        }

        public Vector2 TrackDampingRatio(Vector2 targetValue, float angularFrequency, float dampingRatio, float deltaTime)
        {
            if (angularFrequency < MathUtil.Epsilon)
            {
                Velocity = Vector2.zero;
                return Value;
            }

            Vector2 delta = targetValue - Value;

            float f = 1.0f + 2.0f * deltaTime * dampingRatio * angularFrequency;
            float oo = angularFrequency * angularFrequency;
            float hoo = deltaTime * oo;
            float hhoo = deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector2 detX = f * Value + deltaTime * Velocity + hhoo * targetValue;
            Vector2 detV = Velocity + hoo * delta;

            Velocity = detV * detInv;
            Value = detX * detInv;

            if (Velocity.magnitude < MathUtil.Epsilon && delta.magnitude < MathUtil.Epsilon)
            {
                Velocity = Vector2.zero;
                Value = targetValue;
            }

            return Value;
        }

        public Vector2 TrackHalfLife(Vector2 targetValue, float frequencyHz, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                Velocity = Vector2.zero;
                Value = targetValue;
                return Value;
            }

            float angularFrequency = frequencyHz * MathUtil.TwoPi;
            float dampingRatio = 0.6931472f / (angularFrequency * halfLife);
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }

        public Vector2 TrackExponential(Vector2 targetValue, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                Velocity = Vector2.zero;
                Value = targetValue;
                return Value;
            }

            float angularFrequency = 0.6931472f / halfLife;
            float dampingRatio = 1.0f;
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Vector3Spring
    {
        public static readonly int Stride = 8 * sizeof(float);

        public Vector3 Value;
        private float m_padding0;
        public Vector3 Velocity;
        private float m_padding1;
        private bool completed;

        public bool Completed { get => completed; set => completed = value; }

        public void Reset()
        {
            Value = Vector3.zero;
            Velocity = Vector3.zero;
            completed = false;
        }

        public void Reset(Vector3 initValue)
        {
            Value = initValue;
            Velocity = Vector3.zero;
            completed = false;
        }

        public void Reset(Vector3 initValue, Vector3 initVelocity)
        {
            Value = initValue;
            Velocity = initVelocity;
            completed = false;
        }

        public Vector3 TrackDampingRatio(Vector3 targetValue, float angularFrequency, float dampingRatio, float deltaTime)
        {
            if (angularFrequency < MathUtil.Epsilon)
            {
                Velocity = Vector3.zero;
                return Value;
            }

            Vector3 delta = targetValue - Value;

            float f = 1.0f + 2.0f * deltaTime * dampingRatio * angularFrequency;
            float oo = angularFrequency * angularFrequency;
            float hoo = deltaTime * oo;
            float hhoo = deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector3 detX = f * Value + deltaTime * Velocity + hhoo * targetValue;
            Vector3 detV = Velocity + hoo * delta;

            Velocity = detV * detInv;
            Value = detX * detInv;

            if (Velocity.magnitude < MathUtil.Epsilon && delta.magnitude < MathUtil.Epsilon)
            {
                Velocity = Vector3.zero;
                Value = targetValue;
            }

            return Value;
        }

        public Vector3 TrackHalfLife(Vector3 targetValue, float frequencyHz, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                Velocity = Vector3.zero;
                Value = targetValue;
                return Value;
            }

            float angularFrequency = frequencyHz * MathUtil.TwoPi;
            float dampingRatio = 0.6931472f / (angularFrequency * halfLife);
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }

        public Vector3 TrackExponential(Vector3 targetValue, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                Velocity = Vector3.zero;
                Value = targetValue;
                return Value;
            }

            float angularFrequency = 0.6931472f / halfLife;
            float dampingRatio = 1.0f;
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Vector4Spring
    {
        public static readonly int Stride = 8 * sizeof(float);

        public Vector4 Value;
        public Vector4 Velocity;
        private bool completed;

        public bool Completed { get => completed; set => completed = value; }

        public void Reset()
        {
            Value = Vector4.zero;
            Velocity = Vector4.zero;
            completed = false;
        }

        public void Reset(Vector4 initValue)
        {
            Value = initValue;
            Velocity = Vector4.zero;
            completed = false;
        }

        public void Reset(Vector4 initValue, Vector4 initVelocity)
        {
            Value = initValue;
            Velocity = initVelocity;
            completed = false;
        }

        public Vector4 TrackDampingRatio(Vector4 targetValue, float angularFrequency, float dampingRatio, float deltaTime)
        {
            if (angularFrequency < MathUtil.Epsilon)
            {
                Velocity = Vector4.zero;
                return Value;
            }

            Vector4 delta = targetValue - Value;

            float f = 1.0f + 2.0f * deltaTime * dampingRatio * angularFrequency;
            float oo = angularFrequency * angularFrequency;
            float hoo = deltaTime * oo;
            float hhoo = deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector4 detX = f * Value + deltaTime * Velocity + hhoo * targetValue;
            Vector4 detV = Velocity + hoo * delta;

            Velocity = detV * detInv;
            Value = detX * detInv;

            if (Velocity.magnitude < MathUtil.Epsilon && delta.magnitude < MathUtil.Epsilon)
            {
                Velocity = Vector4.zero;
                Value = targetValue;
            }

            return Value;
        }

        public Vector4 TrackHalfLife(Vector4 targetValue, float frequencyHz, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                Velocity = Vector4.zero;
                Value = targetValue;
                return Value;
            }

            float angularFrequency = frequencyHz * MathUtil.TwoPi;
            float dampingRatio = 0.6931472f / (angularFrequency * halfLife);
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }

        public Vector4 TrackExponential(Vector4 targetValue, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                Velocity = Vector4.zero;
                Value = targetValue;
                return Value;
            }

            float angularFrequency = 0.6931472f / halfLife;
            float dampingRatio = 1.0f;
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }
    }



    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct QuaternionSpring
    {
        public static readonly int Stride = 8 * sizeof(float);

        public Vector4 ValueVec;
        public Vector4 VelocityVec;
        private bool completed;

        public bool Completed { get => completed; set => completed = value; }

        public Quaternion ValueQuat
        {
            get { return QuaternionUtil.FromVector4(ValueVec); }
            set { ValueVec = QuaternionUtil.ToVector4(value); }
        }

        public void Reset()
        {
            ValueVec = QuaternionUtil.ToVector4(Quaternion.identity);
            VelocityVec = Vector4.zero;
            completed = false;
        }

        public void Reset(Vector4 initValue)
        {
            ValueVec = initValue;
            VelocityVec = Vector4.zero;
            completed = false;
        }

        public void Reset(Vector4 initValue, Vector4 initVelocity)
        {
            ValueVec = initValue;
            VelocityVec = initVelocity;
            completed = false;
        }

        public void Reset(Quaternion initValue)
        {
            ValueVec = QuaternionUtil.ToVector4(initValue);
            VelocityVec = Vector4.zero;
            completed = false;
        }

        public void Reset(Quaternion initValue, Quaternion initVelocity)
        {
            ValueVec = QuaternionUtil.ToVector4(initValue);
            VelocityVec = QuaternionUtil.ToVector4(initVelocity);
            completed = false;
        }

        public Quaternion TrackDampingRatio(Vector4 targetValueVec, float angularFrequency, float dampingRatio, float deltaTime)
        {
            if (angularFrequency < MathUtil.Epsilon)
            {
                VelocityVec = QuaternionUtil.ToVector4(Quaternion.identity);
                return QuaternionUtil.FromVector4(ValueVec);
            }

            // keep in same hemisphere for shorter track delta
            if (Vector4.Dot(ValueVec, targetValueVec) < 0.0f)
            {
                targetValueVec = -targetValueVec;
            }

            Vector4 delta = targetValueVec - ValueVec;

            float f = 1.0f + 2.0f * deltaTime * dampingRatio * angularFrequency;
            float oo = angularFrequency * angularFrequency;
            float hoo = deltaTime * oo;
            float hhoo = deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector4 detX = f * ValueVec + deltaTime * VelocityVec + hhoo * targetValueVec;
            Vector4 detV = VelocityVec + hoo * delta;

            VelocityVec = detV * detInv;
            ValueVec = detX * detInv;

            if (VelocityVec.magnitude < MathUtil.Epsilon && delta.magnitude < MathUtil.Epsilon)
            {
                VelocityVec = Vector4.zero;
                ValueVec = targetValueVec;
            }

            return QuaternionUtil.FromVector4(ValueVec);
        }

        public Quaternion TrackDampingRatio(Quaternion targetValue, float angularFrequency, float dampingRatio, float deltaTime)
        {
            return TrackDampingRatio(QuaternionUtil.ToVector4(targetValue), angularFrequency, dampingRatio, deltaTime);
        }

        public Quaternion TrackHalfLife(Vector4 targetValueVec, float frequencyHz, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                VelocityVec = Vector4.zero;
                ValueVec = targetValueVec;
                return QuaternionUtil.FromVector4(targetValueVec);
            }

            float angularFrequency = frequencyHz * MathUtil.TwoPi;
            float dampingRatio = 0.6931472f / (angularFrequency * halfLife);
            return TrackDampingRatio(targetValueVec, angularFrequency, dampingRatio, deltaTime);
        }

        public Quaternion TrackHalfLife(Quaternion targetValue, float frequencyHz, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                VelocityVec = QuaternionUtil.ToVector4(Quaternion.identity);
                ValueVec = QuaternionUtil.ToVector4(targetValue);
                return targetValue;
            }

            float angularFrequency = frequencyHz * MathUtil.TwoPi;
            float dampingRatio = 0.6931472f / (angularFrequency * halfLife);
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }

        public Quaternion TrackExponential(Vector4 targetValueVec, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                VelocityVec = Vector4.zero;
                ValueVec = targetValueVec;
                return QuaternionUtil.FromVector4(targetValueVec);
            }

            float angularFrequency = 0.6931472f / halfLife;
            float dampingRatio = 1.0f;
            return TrackDampingRatio(targetValueVec, angularFrequency, dampingRatio, deltaTime);
        }

        public Quaternion TrackExponential(Quaternion targetValue, float halfLife, float deltaTime)
        {
            if (halfLife < MathUtil.Epsilon)
            {
                VelocityVec = QuaternionUtil.ToVector4(Quaternion.identity);
                ValueVec = QuaternionUtil.ToVector4(targetValue);
                return targetValue;
            }

            float angularFrequency = 0.6931472f / halfLife;
            float dampingRatio = 1.0f;
            return TrackDampingRatio(targetValue, angularFrequency, dampingRatio, deltaTime);
        }
    }


    public class MathUtil
    {
        public static readonly float Pi = Mathf.PI;
        public static readonly float TwoPi = 2.0f * Mathf.PI;
        public static readonly float HalfPi = Mathf.PI / 2.0f;
        public static readonly float QuaterPi = Mathf.PI / 4.0f;
        public static readonly float SixthPi = Mathf.PI / 6.0f;

        public static readonly float Sqrt2 = Mathf.Sqrt(2.0f);
        public static readonly float Sqrt2Inv = 1.0f / Mathf.Sqrt(2.0f);
        public static readonly float Sqrt3 = Mathf.Sqrt(3.0f);
        public static readonly float Sqrt3Inv = 1.0f / Mathf.Sqrt(3.0f);

        public static readonly float Epsilon = 1.0e-3f;
        public static readonly float Rad2Deg = 180.0f / Mathf.PI;
        public static readonly float Deg2Rad = Mathf.PI / 180.0f;

        public static float AsinSafe(float x)
        {
            return Mathf.Asin(Mathf.Clamp(x, -1.0f, 1.0f));
        }

        public static float AcosSafe(float x)
        {
            return Mathf.Acos(Mathf.Clamp(x, -1.0f, 1.0f));
        }

        public static float InvSafe(float x)
        {
            return 1.0f / Mathf.Max(Epsilon, x);
        }

        public static float PointLineDist(Vector2 point, Vector2 linePos, Vector2 lineDir)
        {
            var delta = point - linePos;
            return (delta - Vector2.Dot(delta, lineDir) * lineDir).magnitude;
        }

        public static float PointSegmentDist(Vector2 point, Vector2 segmentPosA, Vector2 segmentPosB)
        {
            var segmentVec = segmentPosB - segmentPosA;
            float segmentDistInv = 1.0f / segmentVec.magnitude;
            var segmentDir = segmentVec * segmentDistInv;
            var delta = point - segmentPosA;
            float t = Vector2.Dot(delta, segmentDir) * segmentDistInv;
            var closest = segmentPosA + Mathf.Clamp(t, 0.0f, 1.0f) * segmentVec;
            return (closest - point).magnitude;
        }

        public static float Seek(float current, float target, float maxDelta)
        {
            float delta = target - current;
            delta = Mathf.Sign(delta) * Mathf.Min(maxDelta, Mathf.Abs(delta));
            return current + delta;
        }

        public static Vector2 Seek(Vector2 current, Vector2 target, float maxDelta)
        {
            Vector2 delta = target - current;
            float deltaMag = delta.magnitude;
            if (deltaMag < Epsilon)
                return target;

            delta = Mathf.Min(maxDelta, deltaMag) * delta.normalized;
            return current + delta;
        }

        public static float Remainder(float a, float b)
        {
            return a - (a / b) * b;
        }

        public static int Remainder(int a, int b)
        {
            return a - (a / b) * b;
        }

        public static float Modulo(float a, float b)
        {
            return Mathf.Repeat(a, b);
        }

        public static int Modulo(int a, int b)
        {
            int r = a % b;
            return r >= 0 ? r : r + b;
        }
    }

    public class QuaternionUtil
    {
        // basic stuff
        // ------------------------------------------------------------------------

        public static float Magnitude(Quaternion q)
        {
            return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        }

        public static float MagnitudeSqr(Quaternion q)
        {
            return q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
        }

        public static Quaternion Normalize(Quaternion q)
        {
            float magInv = 1.0f / Magnitude(q);
            return new Quaternion(magInv * q.x, magInv * q.y, magInv * q.z, magInv * q.w);
        }

        // axis must be normalized
        public static Quaternion AxisAngle(Vector3 axis, float angle)
        {
            float h = 0.5f * angle;
            float s = Mathf.Sin(h);
            float c = Mathf.Cos(h);

            return new Quaternion(s * axis.x, s * axis.y, s * axis.z, c);
        }

        public static Vector3 GetAxis(Quaternion q)
        {
            Vector3 v = new Vector3(q.x, q.y, q.z);
            float len = v.magnitude;
            if (len < MathUtil.Epsilon)
                return Vector3.left;

            return v / len;
        }

        public static float GetAngle(Quaternion q)
        {
            return 2.0f * Mathf.Acos(Mathf.Clamp(q.w, -1.0f, 1.0f));
        }

        public static Quaternion FromAngularVector(Vector3 v)
        {
            float len = v.magnitude;
            if (len < MathUtil.Epsilon)
                return Quaternion.identity;

            v /= len;

            float h = 0.5f * len;
            float s = Mathf.Sin(h);
            float c = Mathf.Cos(h);

            return new Quaternion(s * v.x, s * v.y, s * v.z, c);
        }

        public static Vector3 ToAngularVector(Quaternion q)
        {
            Vector3 axis = GetAxis(q);
            float angle = GetAngle(q);

            return angle * axis;
        }

        public static Quaternion Pow(Quaternion q, float exp)
        {
            Vector3 axis = GetAxis(q);
            float angle = GetAngle(q) * exp;
            return AxisAngle(axis, angle);
        }

        // v: derivative of q
        public static Quaternion Integrate(Quaternion q, Quaternion v, float dt)
        {
            return Pow(v, dt) * q;
        }

        // omega: angular velocity (direction is axis, magnitude is angle)
        // https://www.ashwinnarayan.com/post/how-to-integrate-quaternions/
        // https://gafferongames.com/post/physics_in_3d/
        public static Quaternion Integrate(Quaternion q, Vector3 omega, float dt)
        {
            omega *= 0.5f;
            Quaternion p = (new Quaternion(omega.x, omega.y, omega.z, 0.0f)) * q;
            return Normalize(new Quaternion(q.x + p.x * dt, q.y + p.y * dt, q.z + p.z * dt, q.w + p.w * dt));
        }

        public static Vector4 ToVector4(Quaternion q)
        {
            return new Vector4(q.x, q.y, q.z, q.w);
        }

        public static Quaternion FromVector4(Vector4 v, bool normalize = true)
        {
            if (normalize)
            {
                float magSqr = v.sqrMagnitude;
                if (magSqr < MathUtil.Epsilon)
                    return Quaternion.identity;

                v /= Mathf.Sqrt(magSqr);
            }

            return new Quaternion(v.x, v.y, v.z, v.w);
        }

        // ------------------------------------------------------------------------
        // end: basic stuff


        // swing-twist decomposition & interpolation
        // ------------------------------------------------------------------------

        public static void DecomposeSwingTwist
        (
          Quaternion q,
          Vector3 twistAxis,
          out Quaternion swing,
          out Quaternion twist
        )
        {
            Vector3 r = new Vector3(q.x, q.y, q.z); // (rotaiton axis) * cos(angle / 2)

            // singularity: rotation by 180 degree
            if (r.sqrMagnitude < MathUtil.Epsilon)
            {
                Vector3 rotatedTwistAxis = q * twistAxis;
                Vector3 swingAxis = Vector3.Cross(twistAxis, rotatedTwistAxis);

                if (swingAxis.sqrMagnitude > MathUtil.Epsilon)
                {
                    float swingAngle = Vector3.Angle(twistAxis, rotatedTwistAxis);
                    swing = Quaternion.AngleAxis(swingAngle, swingAxis);
                }
                else
                {
                    // more singularity: rotation axis parallel to twist axis
                    swing = Quaternion.identity; // no swing
                }

                // always twist 180 degree on singularity
                twist = Quaternion.AngleAxis(180.0f, twistAxis);
                return;
            }

            // formula & proof: 
            // http://www.euclideanspace.com/maths/geometry/rotations/for/decomposition/
            Vector3 p = Vector3.Project(r, twistAxis);
            twist = new Quaternion(p.x, p.y, p.z, q.w);
            twist = Normalize(twist);
            swing = q * Quaternion.Inverse(twist);
        }

        public enum SterpMode
        {
            // non-constant angular velocity, faster
            // use if interpolating across small angles or constant angular velocity is not important
            Nlerp,

            // constant angular velocity, slower
            // use if interpolating across large angles and constant angular velocity is important
            Slerp,
        };

        // same swing & twist parameters
        public static Quaternion Sterp
        (
          Quaternion a,
          Quaternion b,
          Vector3 twistAxis,
          float t,
          SterpMode mode = SterpMode.Slerp
        )
        {
            Quaternion swing;
            Quaternion twist;
            return Sterp(a, b, twistAxis, t, out swing, out twist, mode);
        }

        // same swing & twist parameters with individual interpolated swing & twist outputs
        public static Quaternion Sterp
        (
          Quaternion a,
          Quaternion b,
          Vector3 twistAxis,
          float t,
          out Quaternion swing,
          out Quaternion twist,
          SterpMode mode = SterpMode.Slerp
        )
        {
            return Sterp(a, b, twistAxis, t, t, out swing, out twist, mode);
        }

        // different swing & twist parameters
        public static Quaternion Sterp
        (
          Quaternion a,
          Quaternion b,
          Vector3 twistAxis,
          float tSwing,
          float tTwist,
          SterpMode mode = SterpMode.Slerp
        )
        {
            Quaternion swing;
            Quaternion twist;
            return Sterp(a, b, twistAxis, tSwing, tTwist, out swing, out twist, mode);
        }

        // master sterp function
        public static Quaternion Sterp
        (
          Quaternion a,
          Quaternion b,
          Vector3 twistAxis,
          float tSwing,
          float tTwist,
          out Quaternion swing,
          out Quaternion twist,
          SterpMode mode
        )
        {
            Quaternion q = b * Quaternion.Inverse(a);
            Quaternion swingFull;
            Quaternion twistFull;
            QuaternionUtil.DecomposeSwingTwist(q, twistAxis, out swingFull, out twistFull);

            switch (mode)
            {
                default:
                case SterpMode.Nlerp:
                    swing = Quaternion.Lerp(Quaternion.identity, swingFull, tSwing);
                    twist = Quaternion.Lerp(Quaternion.identity, twistFull, tTwist);
                    break;
                case SterpMode.Slerp:
                    swing = Quaternion.Slerp(Quaternion.identity, swingFull, tSwing);
                    twist = Quaternion.Slerp(Quaternion.identity, twistFull, tTwist);
                    break;
            }

            return twist * swing;
        }

    }
}
