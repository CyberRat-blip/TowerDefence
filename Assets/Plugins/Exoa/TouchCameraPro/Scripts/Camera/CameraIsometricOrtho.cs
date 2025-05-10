using Exoa.Designer;
using Exoa.Events;
using Exoa.Touch;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras
{
    public class CameraIsometricOrtho : CameraOrthoBase, ITouchOrthoCamera
    {
        /// <summary>
        /// Init some camera parameters
        /// </summary>
        override protected void Init()
        {
            base.Init();
            initSize = finalSize = cam.orthographicSize;
            initialRotation.y = transform.rotation.eulerAngles.y;
            currentPitch = initialRotation.x;
            currentYaw = initialRotation.y;
            GetInitialRotation();

            initDistance = CalculateDistance(transform.position, transform.rotation);

            finalOffset = CalculateOffset(transform.position, transform.rotation, initDistance, groundHeight);
            finalRotation = GetRotationFromPitchYaw();
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
            finalDistance = initDistance;
        }


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

        /// <summary>
        /// Gives the initial rotation of the camera to be able to reset it later
        /// </summary>
        /// <returns></returns>
        override protected Quaternion GetInitialRotation()
        {
            initRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, 0);
            return initRotation;
        }

        #region EVENTS

        /// <summary>
        /// Called just before the perspective switch happens
        /// </summary>
        /// <param name="orthoMode"></param>
        override protected void OnBeforeSwitchPerspective(bool orthoMode)
        {
            if (!orthoMode)
            {
                currentPitch = initialRotation.x;
                currentYaw = initialRotation.y;
                finalRotation = GetRotationFromPitchYaw();
                finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
            }
        }

        #endregion


    }
}
