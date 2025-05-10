using Exoa.Designer;
using Exoa.Events;
using Exoa.Touch;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras
{
    public class CameraTopDownOrtho : CameraOrthoBase, ITouchOrthoCamera
    {

        /// <summary>
        /// Init some camera parameters
        /// </summary>
        override protected void Init()
        {
            base.Init();
            initSize = finalSize = cam.orthographicSize;
            initialRotation.y = transform.rotation.eulerAngles.y;
            initOffset = transform.position.SetY(groundHeight);
            initDistance = CalculateDistance(transform.position, transform.rotation);

            finalOffset = initOffset;
            finalPosition = transform.position.SetY(fixedDistance);
            finalRotation = GetInitialRotation();
            finalDistance = initDistance;

            currentPitch = initialRotation.x;
            currentYaw = initialRotation.y;

            allowPitchRotation = false;
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

            currentPitch = rotation.eulerAngles.x;
            currentYaw = rotation.eulerAngles.y;

            GetInitialRotation();
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

        #region UTILS
        /// <summary>
        /// Gives the initial rotation of the camera to be able to reset it later
        /// </summary>
        /// <returns></returns>
        override protected Quaternion GetInitialRotation()
        {
            initRotation = Quaternion.Euler(90, initialRotation.y, 0);
            return initRotation;
        }



        /// <summary>
        /// Converts a camera offset on ground to a camera position
        /// </summary>
        override public void SetPositionByOffset()
        {
            finalPosition = finalOffset.SetY(fixedDistance);
        }
        #endregion




    }
}
