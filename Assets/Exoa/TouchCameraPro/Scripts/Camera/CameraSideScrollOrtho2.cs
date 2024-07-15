using Exoa.Designer;
using Exoa.Events;
using Exoa.Touch;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras
{
    public class CameraSideScrollOrtho2 : CameraOrthoBase, ITouchOrthoCamera
    {
        [Header("PLANE")]
        public Exoa.Common.Plane plane;

        override protected void CreateConverter()
        {
            HeightScreenDepth = new ScreenDepth(ScreenDepth.ConversionType.PlaneIntercept);
            HeightScreenDepth.Object = plane;
        }

        /// <summary>
        /// Init some camera parameters
        /// </summary>
        override protected void Init()
        {
            base.Init();

            fixedDistance = -Mathf.Abs(fixedDistance);

            initSize = finalSize = cam.orthographicSize;
            initialRotation = new Vector2(0, plane.transform.rotation.eulerAngles.y);

            currentPitch = initialRotation.x;
            currentYaw = initialRotation.y;
            GetInitialRotation();
            initDistance = fixedDistance;
            //initDistance = CalculateDistance(transform.position, transform.rotation);

            finalOffset = CalculateOffset(transform.position, transform.rotation, initDistance, groundHeight);
            finalRotation = GetRotationFromPitchYaw();
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
            finalDistance = initDistance;

            allowPitchRotation = false;
        }




        protected override void Update()
        {
            currentPitch = plane.transform.rotation.eulerAngles.x;
            currentYaw = plane.transform.rotation.eulerAngles.y;
            //Debug.Log("currentYaw:" + currentYaw);
            RotateFromVector(CameraInputs.GetAnyPixelScaledDelta());
            finalRotation = GetRotationFromPitchYaw();

            base.Update();

        }


        #region RESET


        /// <summary>
        /// Set the initial values for the reset function
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="rotation"></param>
        /// <param name="distanceOrSize"></param>
        override public void SetResetValues(Vector3 offset, Quaternion rotation, float size)
        {
            initOffset = offset;

            initialRotation = rotation.eulerAngles;
            GetInitialRotation();
            //Debug.Log("initialRotation:" + initialRotation + " init rot:" + initRotation.eulerAngles);
            initSize = size;
        }


        /// <summary>
        /// Reset the camera to initial values
        /// </summary>
        override public void ResetCamera()
        {
            StopFollow();
            FocusCamera(initOffset, initSize, initRotation.eulerAngles);
        }
        #endregion

        /// <summary>
        /// Gives the initial rotation of the camera to be able to reset it later
        /// </summary>
        /// <returns></returns>
        override protected Quaternion GetInitialRotation()
        {
            //initialRotation = new Vector2(0, plane.transform.rotation.eulerAngles.y);
            initRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, 0);
            return initRotation;
        }

        #region EVENTS
        override protected Vector2 GetRotationToPitchYaw(Quaternion rot)
        {
            rot = plane.transform.rotation;
            //Log("GetRotationToPitchYaw2b");
            return new Vector2(NormalizeAngle(rot.eulerAngles.x), rot.eulerAngles.y);
        }

        override protected Quaternion GetRotationFromPitchYaw()
        {
            //Log("GetRotationFromPitchYaw b");
            currentPitch = plane.transform.rotation.eulerAngles.x;
            currentYaw = plane.transform.rotation.eulerAngles.y;
            currentPitch = NormalizeAngle(currentPitch);
            currentYaw = NormalizeAngle(currentYaw);
            return Quaternion.Euler(currentPitch, currentYaw, 0);

        }
        /// <summary>
        /// Called just before the perspective switch happens
        /// </summary>
        /// <param name="orthoMode"></param>
        override protected void OnBeforeSwitchPerspective(bool orthoMode)
        {
            if (!orthoMode)
            {
                initialRotation = new Vector2(0, plane.transform.rotation.eulerAngles.y);
                currentPitch = initialRotation.x;
                currentYaw = initialRotation.y;
                finalRotation = GetRotationFromPitchYaw();
                finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
            }
        }

        #endregion


    }
}
