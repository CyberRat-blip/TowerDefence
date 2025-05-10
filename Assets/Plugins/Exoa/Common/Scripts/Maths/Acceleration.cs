using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Maths
{
    /// <summary>
    /// Class to calculate acceleration other a given sample array
    /// </summary>
    [System.Serializable]
    public class Acceleration
    {

        private Vector3[] positionRegister;
        private float[] posTimeRegister;
        private int positionSamplesTaken = 0;
        public int samples = 22;
        public float maxMagnitude;
        public float accelerationFactor = 100f;
        private Vector3 outputAcceleration;

        /// <summary>
        /// Get the output acceleration using the samples
        /// </summary>
        public Vector3 OutputAcceleration
        {
            get
            {
                if (maxMagnitude > 0 && outputAcceleration.magnitude > maxMagnitude)
                    return outputAcceleration.normalized * maxMagnitude;
                return outputAcceleration;
            }
        }

        /// <summary>
        ///  Clear the samples
        /// </summary>
        public void Clear()
        {
            posTimeRegister = null;
            positionRegister = null;
            positionSamplesTaken = 0;
            outputAcceleration = Vector3.zero;
        }

        /// <summary>
        /// Add a position to the samples
        /// </summary>
        /// <param name="position"></param>
        /// <param name="debug"></param>
        /// <param name="mouseUp"></param>
        /// <returns></returns>
        public bool AddPosition(Vector3 position, bool debug = false, bool mouseUp = false, bool isRotationValues = false)
        {
            Vector3 averageSpeedChange = Vector3.zero;
            outputAcceleration = Vector3.zero;
            float averageDeltaTime = 0f;
            Vector3 deltaDistance;
            float deltaTime;
            Vector3 speedA;
            float deltaTimeA;
            Vector3 speedB;

            //Clamp sample amount. In order to calculate acceleration we need at least 2 changes
            //in speed, so we need at least 3 position samples.
            if (samples < 3)
            {
                samples = 3;
            }

            //Initialize
            if (positionRegister == null)
            {

                positionRegister = new Vector3[samples];
                posTimeRegister = new float[samples];
            }

            if (isRotationValues && positionRegister.Length > 0)
            {
                position = FixRotationValue(position, positionRegister[positionRegister.Length - 1]);
            }
            if (mouseUp && positionRegister.Length > 0)
            {
                position = positionRegister[positionRegister.Length - 1];
            }
            else if (!mouseUp && positionRegister.Length > 0 && positionRegister[positionRegister.Length - 1] == position)
            {
                return false;
            }
            //Fill the position and time sample array and shift the location in the array to the left
            //each time a new sample is taken. This way index 0 will always hold the oldest sample and the
            //highest index will always hold the newest sample. 
            for (int i = 0; i < positionRegister.Length - 1; i++)
            {
                positionRegister[i] = positionRegister[i + 1];
                posTimeRegister[i] = posTimeRegister[i + 1];
            }
            positionRegister[positionRegister.Length - 1] = position;
            posTimeRegister[posTimeRegister.Length - 1] = Time.unscaledTime;

            positionSamplesTaken++;
            if (debug) Debug.Log("Linear positionSamplesTaken:" + positionSamplesTaken + " positionRegister.Length:" + positionRegister.Length);

            //The output acceleration can only be calculated if enough samples are taken.
            if (positionSamplesTaken >= 3)
            {
                List<float> timeList = new List<float>();
                //Calculate average speed change.
                for (int i = 0; i < positionRegister.Length - 1; i++)
                {
                    if (debug) Debug.Log("Linear " + i + " t:" + posTimeRegister[i] + " p:" + positionRegister[i].Dump());


                    deltaDistance = positionRegister[i + 1] - positionRegister[i];
                    deltaTime = posTimeRegister[i + 1] - posTimeRegister[i];

                    //If deltaTime is 0, the output is invalid.
                    if (deltaTime == 0 || posTimeRegister[i] == 0 || deltaDistance == Vector3.zero)
                    {
                        if (debug) Debug.Log("Skipping");
                        continue;
                    }
                    if (debug)
                    {
                        Debug.Log("Linear " + i + " deltaDistance:" + deltaDistance.magnitude + " deltaTime:" + deltaTime);
                    }
                    speedA = deltaDistance / deltaTime;
                    deltaTimeA = deltaTime;

                    if (debug) Debug.Log("speedA:" + speedA);
                    if (debug) Debug.Log("deltaTimeA:" + deltaTimeA);

                    //This is the accumulated speed change at this stage, not the average yet.
                    averageSpeedChange += speedA;
                    averageDeltaTime += deltaTimeA;

                    timeList.Add(posTimeRegister[i]);
                }
                if (timeList.Count == 0)
                    return false;

                if (debug) Debug.Log("Linear accumulated SpeedChange:" + averageSpeedChange);
                if (debug) Debug.Log("Linear accumulated DeltaTime:" + averageDeltaTime);

                //Now this is the average speed change.
                averageSpeedChange /= timeList.Count - 1;
                averageDeltaTime /= timeList.Count - 1;

                //Get the total time difference.
                if (debug)
                {
                    Debug.Log("Time last point:" + timeList[timeList.Count - 1]);
                    Debug.Log("Time first point:" + timeList[0]);
                }
                float deltaTimeTotal = timeList[timeList.Count - 1] - timeList[0];
                if (deltaTimeTotal == 0)
                    return false;

                //Now calculate the acceleration, which is an average over the amount of samples taken.
                outputAcceleration = averageSpeedChange / averageDeltaTime / accelerationFactor;

                if (debug)
                {
                    Debug.Log("Linear count:" + timeList.Count);
                    Debug.Log("Linear deltaTimeTotal:" + deltaTimeTotal);
                    Debug.Log("Linear averageDeltaTime:" + averageDeltaTime);
                    Debug.Log("Linear averageSpeedChange:" + averageSpeedChange.Dump());
                    Debug.Log("Linear vector:" + outputAcceleration.Dump());
                }
                return true;
            }

            else
            {

                return false;
            }
        }

        private static float FixRotationValue(float v, float prev)
        {
            float distance = Mathf.Abs(prev - v);
            float distanceUp = Mathf.Abs(prev - (v + 360f));
            float distanceDown = Mathf.Abs(prev - (v - 360f));

            if (distanceUp < distance && distanceUp < distanceDown)
            {
                return v + 360f;
            }
            else if (distanceDown < distance && distanceDown < distanceUp)
            {
                return v - 360f;
            }
            return v;
        }
        private Vector3 FixRotationValue(Vector3 position, Vector3 prev)
        {
            position.x = FixRotationValue(position.x, prev.x);
            position.y = FixRotationValue(position.y, prev.y);
            position.z = FixRotationValue(position.z, prev.z);
            return position;
        }
    }
}
