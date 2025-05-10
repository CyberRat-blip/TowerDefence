using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Maths
{
    public abstract class BaseVelocityIntegral
    {
        protected float a, b, c, d, logbase;
        virtual public void Init(float a = 1, float b = 1, float c = 0, float d = 1, float logbase = 10)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.logbase = logbase;
        }
        abstract public float GetIntegral(float x);
        abstract public float GetVelocity(float pointOnCurve);
    }




    public class Bx_C : BaseVelocityIntegral
    {

        public override float GetIntegral(float x)
        {
            float x2 = x * x;
            return ((b * x2) / 2.0f) + c * x;
        }
        override public float GetVelocity(float pointOnCurve)
        {
            return b * pointOnCurve + c;
        }

    }




    public class Ax3_Bx2_Dx_C : BaseVelocityIntegral
    {

        override public float GetIntegral(float x)
        {
            float x2 = x * x;
            float x3 = x2 * x;
            float x4 = x2 * x2;
            return ((a * x4) / 4.0f) + ((b * x3) / 3.0f) + ((d * x2) / 2.0f) + c * x;
        }
        override public float GetVelocity(float pointOnCurve)
        {
            float x2 = pointOnCurve * pointOnCurve;
            float x3 = x2 * pointOnCurve;
            return a * x3 + b * x2 + d * pointOnCurve + c;
        }

    }




    public class Ax2_Bx_C : BaseVelocityIntegral
    {

        override public float GetIntegral(float x)
        {
            //Figure out the area under the curve
            float x2 = x * x;
            float x3 = x2 * x;
            return ((a * x3) / 3.0f) + ((b * x2) / 2.0f) + c * x;
        }
        override public float GetVelocity(float pointOnCurve)
        {
            float x2 = pointOnCurve * pointOnCurve;
            return a * x2 + b * pointOnCurve + c;
        }

    }






    public class Ax2_C : BaseVelocityIntegral
    {

        override public float GetIntegral(float x)
        {
            //Figure out the area under the curve
            return ((a * x * x * x) / 3.0f) + c * x;
        }
        override public float GetVelocity(float pointOnCurve)
        {
            return a * pointOnCurve * pointOnCurve + c;
        }

    }



    public class AlogXplusB_C : BaseVelocityIntegral
    {

        override public float GetIntegral(float x)
        {
            return (a * (b + x) * UnityEngine.Mathf.Log(b + x) - a * x + c * x * UnityEngine.Mathf.Log(logbase)) / UnityEngine.Mathf.Log(logbase);
        }
        override public float GetVelocity(float pointOnCurve)
        {
            return (a * UnityEngine.Mathf.Log(pointOnCurve + b)) / UnityEngine.Mathf.Log(logbase) + c;
        }
    }
}