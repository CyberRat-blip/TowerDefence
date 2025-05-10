using Exoa.Designer;
using Exoa.Events;
using Exoa.Touch;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras
{
    public class CameraSideScrollOrtho : CameraOrthoBase, ITouchOrthoCamera
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
            initialRotation.y = transform.rotation.eulerAngles.z;
            initOffset = transform.position.SetZ(plane.transform.position.z);
            initDistance = fixedDistance;

            finalOffset = initOffset;
            finalPosition = transform.position.SetZ(fixedDistance);
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
        /// Calculate the offset position on the ground, given the camera's position and rotation
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        override protected Vector3 CalculateOffset(Vector3 pos, Quaternion rot)
        {
            Vector3 camOffset = pos.SetZ(plane.transform.position.z);
            return camOffset;
        }

        /// <summary>
        /// Calculates the camera transform's position giving the offset, rotation and distance
        /// </summary>
        /// <param name="center"></param>
        /// <param name="rot"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        override protected Vector3 CalculatePosition(Vector3 center, Quaternion rot, float distance)
        {
            return center.SetZ(center.z + fixedDistance);
        }

        /// <summary>
        /// Calculate the offset position on the ground, given the camera's position, rotation, distance from ground and ground height
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="distance"></param>
        /// <param name="groundHeight"></param>
        /// <returns></returns>
        override protected Vector3 CalculateOffset(Vector3 pos, Quaternion rot, float distance, float groundHeight)
        {
            Vector3 offset = pos - rot * (Vector3.back * distance);
            return offset.SetZ(plane.transform.position.z);
        }


        /// <summary>
        /// Gives the initial rotation of the camera to be able to reset it later
        /// </summary>
        /// <returns></returns>
        override protected Quaternion GetInitialRotation()
        {
            initRotation = Quaternion.Euler(0, 0, initialRotation.y);
            return initRotation;
        }


        /// <summary>
        /// Converts pitch/yaw rotations to a quaternion
        /// </summary>
        /// <returns></returns>
        override protected Quaternion GetRotationFromPitchYaw()
        {
            return Quaternion.Euler(0, 0, currentYaw);
        }

        /// <summary>
        /// Converts pitch/yaw rotations to a quaternion
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        /// <returns></returns>
        override protected Quaternion GetRotationFromPitchYaw(float pitch, float yaw)
        {
            return Quaternion.Euler(0, 0, yaw);
        }

        /// <summary>
        /// Converts pitch/yaw rotations to a quaternion
        /// </summary>
        /// <param name="pitchYawVec"></param>
        /// <returns></returns>
        override protected Quaternion GetRotationFromPitchYaw(Vector2 pitchYawVec)
        {
            return Quaternion.Euler(0, 0, pitchYawVec.y);
        }

        /// <summary>
        /// Converts the quaternion rotation to pitch/yaw values
        /// </summary>
        /// <returns></returns>
        override protected Vector2 GetRotationToPitchYaw()
        {
            return new Vector2(0, finalRotation.eulerAngles.z);
        }

        /// <summary>
        /// Converts the quaternion rotation to pitch/yaw values
        /// </summary>
        /// <param name="rot"></param>
        /// <returns></returns>
        override protected Vector2 GetRotationToPitchYaw(Quaternion rot)
        {
            return new Vector2(0, rot.eulerAngles.z);
        }

        /// <summary>
        /// indicates the vector for AngleAxis, to rotate around
        /// </summary>
        /// <returns></returns>
        override protected Vector3 GetRotateAroundVector()
        {
            return -Vector3.forward;
        }

        /// <summary>
        /// Converts a camera offset on ground to a camera position
        /// </summary>
        override public void SetPositionByOffset()
        {
            //finalPosition = finalOffset + plane.transform.forward * fixedDistance;
            finalPosition = finalOffset.SetZ(finalOffset.z + fixedDistance);
        }

        /// <summary>
        /// Clamp any given point inside the boundaries collider on XY plan. Z will be unchanged
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        override protected Vector3 ClampInCameraBoundaries(Vector3 targetPosition)
        {
            return ClampInCameraBoundaries(targetPosition, out bool isInBoundaries);
        }


        /// <summary>
        /// Clamp any given point inside the boundaries collider on XY plan. Z will be unchanged
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        override protected Vector3 ClampInCameraBoundaries(Vector3 targetPosition, out bool isInBoundaries)
        {
            bool anyFingerDown = CameraInputs.GetFingerCount() > 0;
            isInBoundaries = true;
            if (camBounds != null)
                return camBounds.ClampInBoundsXY(targetPosition, out isInBoundaries, groundHeight, anyFingerDown || isApplyingMoveInertia);
            return targetPosition;
        }
        #endregion


        #region FOLLOW & FOCUS

        /// <summary>
        /// Handle the camera focus/follow/moveto
        /// </summary>
        override protected void HandleFocusAndFollow()
        {
            if (!isFocusingOrFollowing)
                return;

            if (focusTargetGo != null)
            {
                Bounds b = focusTargetGo.GetBoundsRecursive();

                if (b.size == Vector3.zero && b.center == Vector3.zero)
                    return;

                // offseting the bounding box
                b.center = b.center.SetZ(groundHeight);

                Vector3 max = b.size;
                // Get the radius of a sphere circumscribing the bounds
                float radius = max.magnitude * focusRadiusMultiplier;

                focusTargetPosition = b.center;
                focusTargetDistanceOrSize = Mathf.Clamp(radius, sizeMinMax.x, sizeMinMax.y);

            }

            if (enableDistanceChange)
            {
                finalSize = followMove.Update(ref followMoveDistanceOrSize, focusTargetDistanceOrSize);
            }
            if (enableRotationChange)
            {
                finalRotation = followMove.Update(ref followMoveRotation, focusTargetRotation);
                currentPitch = finalRotation.eulerAngles.x;
                currentYaw = finalRotation.eulerAngles.z;
            }
            finalOffset = worldPointCameraCenter = followMove.Update(ref followMoveOffset, focusTargetPosition, OnFollowFocusCompleted);

            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);

        }
        #endregion



    }
}
