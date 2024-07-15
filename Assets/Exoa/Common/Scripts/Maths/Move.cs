using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Exoa.Maths
{
    [System.Serializable]
    public class Move
    {
        public enum MoveType
        {
            Hyperbolic_Ax3_Bx2_Dx_C,
            Parabolic_Ax2_Bx_C,
            Parabolic2_Ax2_C,
            Linear_Bx_C,
            Logarithmic_Alog_xplusB_C
        }
        public MoveType moveType;
        private Vector3 customForceVector = Vector3.up;

        private Vector3 lastPositionOffset = Vector3.zero;

        //We may want to put a hard cap on the velocity,
        // so we have a max and min which you'll use depending on
        // whether your function trends towards positive or negative infinity
        public float maxVelocity = -1;
        public float minVelocity = 9999;
        private bool hasReachedMaxVelocity = false;
        private float maxVelocityReturn = 0;

        //Used to calculate both the velocity and the area under the curve
        protected float pointOnCurve = 0;
        protected float integralNought = 0;

        public float A = 1;
        public float B = 1;
        public float D = 1;
        public float logBase = 10;

        protected float debugVelocity;
        protected float debugIntegral;
        protected bool active;

        protected BaseVelocityIntegral currentCalc;

        public Move(Move groundMove)
        {
            this.moveType = groundMove.moveType;
            this.maxVelocity = groundMove.maxVelocity;
            this.minVelocity = groundMove.minVelocity;
            this.A = groundMove.A;
            this.B = groundMove.B;
            this.D = groundMove.D;
            this.logBase = groundMove.logBase;
        }

        public bool Active { get => active; set => active = value; }

        public void Init(float startingSpeed, Vector3 dir)
        {
            //Debug.Log("Move Init startingSpeed:" + startingSpeed + " dir:" + dir);

            customForceVector = dir;

            //Call whenever we want to start accelerating
            lastPositionOffset = Vector3.zero;

            pointOnCurve = 0;


            switch (moveType)
            {
                case MoveType.Linear_Bx_C: currentCalc = new Bx_C(); break;
                case MoveType.Hyperbolic_Ax3_Bx2_Dx_C: currentCalc = new Ax3_Bx2_Dx_C(); break;
                case MoveType.Parabolic_Ax2_Bx_C: currentCalc = new Ax2_Bx_C(); break;
                case MoveType.Parabolic2_Ax2_C: currentCalc = new Ax2_C(); break;
                case MoveType.Logarithmic_Alog_xplusB_C: currentCalc = new AlogXplusB_C(); break;
            }
            currentCalc.Init(A, B, startingSpeed, D, logBase);

            integralNought = CalculateIntegral(pointOnCurve);
            if (float.IsNaN(integralNought))
            {
                pointOnCurve += 0.0001f;
                integralNought = CalculateIntegral(pointOnCurve);

            }
            hasReachedMaxVelocity = false;
            active = true;

            GetVelocity();

        }

        public float GetVelocity()
        {
            if (hasReachedMaxVelocity) return maxVelocityReturn;
            float result = CalculateVelocity();
            if (maxVelocity > 0 && result > maxVelocity) { return SetMaxVelocityReached(maxVelocity); }
            if (minVelocity < 9999 && result < minVelocity) { return SetMaxVelocityReached(minVelocity); }
            return result;
        }

        public Vector3 GetVelocityOnVector()
        {
            if (hasReachedMaxVelocity) return customForceVector * maxVelocityReturn;
            return GetVelocity() * customForceVector;
        }

        public Vector3 GetOffsetPosition()
        {
            float result = CalculateIntegral(pointOnCurve) - integralNought;
            return result * customForceVector;
        }



        protected float CalculateIntegral(float x)
        {
            return currentCalc == null ? 0 : currentCalc.GetIntegral(x);
        }
        protected float CalculateVelocity()
        {
            return currentCalc == null ? 0 : currentCalc.GetVelocity(pointOnCurve);
        }





        public Vector3 Update()
        {
            Vector3 result;
            if (!hasReachedMaxVelocity)
                pointOnCurve += Time.unscaledDeltaTime;

            Vector3 v = GetVelocityOnVector();

            if (hasReachedMaxVelocity)
            {
                result = v * Time.unscaledDeltaTime;
            }
            else
            {
                Vector3 currentOffset = GetOffsetPosition();
                result = currentOffset - lastPositionOffset;
                lastPositionOffset = GetOffsetPosition();
            }
            debugVelocity = CalculateVelocity();
            debugIntegral = CalculateIntegral(pointOnCurve);

            //Debug.Log("debugVelocity:" + debugVelocity +
            //    " debugIntegral:" + debugIntegral);

            //Debug.Log("Move Update result:" + result +
            //    " hasReachedMaxVelocity:" +
            //    hasReachedMaxVelocity +
            //    " pointOnCurve:" + pointOnCurve + " v:" + v + " result:" + result);

            if (float.IsNaN(result.x))
                return Vector3.zero;
            return result;
        }



        private float SetMaxVelocityReached(float val)
        {
            hasReachedMaxVelocity = true;
            active = false;
            maxVelocityReturn = val;
            return maxVelocityReturn;
        }
    }
}
