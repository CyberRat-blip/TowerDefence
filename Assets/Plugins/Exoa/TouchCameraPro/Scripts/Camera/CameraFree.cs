using Exoa.Designer;
using Exoa.Common;
using Exoa.Touch;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras
{
    public class CameraFree : CameraPerspective, ITouchPerspCamera
    {
        private RaycastHit hitInfo;
        private bool isHitting;
        public float maxDistance = 100f;
        public LayerMask layerMask;
        public Exoa.Common.Plane plane;
        public Transform sphere;



        /// <summary>
        /// Create the depth converter between fingers on screen and the 3D World
        /// this one tries to find a collider if front of the camera
        /// </summary>
        override protected void CreateConverter()
        {
            HeightScreenDepth = new ScreenDepth(ScreenDepth.ConversionType.PlaneIntercept, -5, groundHeight);
            Vector2 screenCenter = cam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
            FindGround(screenCenter);
        }
        /*
        /// <summary>
        /// Update the camera parameters based on user's input
        /// </summary>
        override protected void Update()
        {
            if (disableMoves)
                return;

            List<TouchFinger> twoFingers = Inputs.TwoFingerFilter.UpdateAndGetFingers();
            List<TouchFinger> oneFinger = Inputs.OneFingerFilter.UpdateAndGetFingers();

            Vector2 screenCenter = cam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));

            worldPointCameraCenter = ClampInCameraBoundaries(HeightScreenDepth.Convert(screenCenter));
            float zoomRatio = 1;
            bool anyInteraction = false;

            worldPointFingersDelta = Vector3.zero;
            worldPointFingersCenter = ClampInCameraBoundaries(HeightScreenDepth.Convert(Inputs.screenPointAnyFingerCountCenter));
            twistRot = Quaternion.identity;

            if (IsInputMatching(InputMapFingerDrag.RotateAround))
            {
                RotateFromVector(Inputs.GetAnyPixelScaledDelta());
                finalRotation = GetRotationFromPitchYaw();
                finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
                anyInteraction = true;
            }
            if (IsInputMatching(InputMapFingerDrag.RotateHead))
            {
                RotateFromVector(Inputs.GetAnyPixelScaledDelta());
                finalRotation = GetRotationFromPitchYaw();
                anyInteraction = true;
            }

            if (IsInputMatching(InputMapFingerPinch.ZoomAndRotate) || IsInputMatching(InputMapFingerPinch.ZoomOnly))
            {
                zoomRatio = Inputs.pinchRatio;
                anyInteraction = true;
            }

            if (IsInputMatching(InputMapScrollWheel.ZoomInCenter))
            {
                zoomRatio = Inputs.GetScroll();
                worldPointFingersCenter = ClampInCameraBoundaries(HeightScreenDepth.Convert(screenCenter));
                anyInteraction = true;
            }

            if (IsInputMatching(InputMapScrollWheel.ZoomUnderMouse))
            {
                zoomRatio = Inputs.GetScroll();
                worldPointFingersCenter = ClampInCameraBoundaries(HeightScreenDepth.Convert(BaseTouchInput.GetMousePosition()));
                anyInteraction = true;
            }

            if (minMaxDistance.y == finalDistance && zoomRatio > 1 || minMaxDistance.x == finalDistance && zoomRatio < 1)
            {
                zoomRatio = 1;
            }

            finalDistance = CalculateClampedDistance(finalPosition, worldPointCameraCenter, minMaxDistance, zoomRatio);

            if (IsInputMatching(InputMapFingerPinch.ZoomAndRotate) || IsInputMatching(InputMapFingerPinch.RotateOnly))
            {
                twistRot = Quaternion.AngleAxis(allowYawRotation ? Inputs.twistDegrees : 0, GetRotateAroundVector());
                anyInteraction = true;
            }
            if (!isFocusingOrFollowing && IsInputMatching(InputMapFingerDrag.Translate))
            {
                worldPointFingersDelta = Vector3.ClampMagnitude(HeightScreenDepth.ConvertDelta(Inputs.lastScreenPointAnyFingerCountCenter, Inputs.screenPointAnyFingerCountCenter, gameObject), maxTranslationSpeed);
                anyInteraction = true;
            }
            if (anyInteraction)
            {
                if (!isHitting)
                {
                    FindGround(Inputs.screenPointAnyFingerCountCenter);
                }
                if (isHitting)
                {
                }
            }
            else
            {
                isHitting = false;
            }

            Vector3 vecFingersCenterToCamera = (finalPosition - worldPointFingersCenter);
            float vecFingersCenterToCameraDistance = vecFingersCenterToCamera.magnitude * zoomRatio;
            vecFingersCenterToCamera = vecFingersCenterToCamera.normalized * vecFingersCenterToCameraDistance;

            Vector3 targetPosition = worldPointFingersCenter + vecFingersCenterToCamera;
            Vector3 offsetFromFingerCenter = worldPointFingersCenter - worldPointFingersDelta;

            finalPosition = twistRot * (targetPosition - worldPointFingersCenter) + offsetFromFingerCenter;
            finalRotation = twistRot * finalRotation;

            currentPitch = NormalizeAngle(finalRotation.eulerAngles.x);
            currentYaw = (finalRotation.eulerAngles.y);

            Vector3 newWorldPointCameraCenter = CalculateOffset(finalPosition, finalRotation);
            Vector3 newWorldPointCameraCenterClamped = ClampInCameraBoundaries(newWorldPointCameraCenter);

            finalOffset = newWorldPointCameraCenterClamped;
            finalDistance = CalculateClampedDistance(finalPosition, newWorldPointCameraCenter, minMaxDistance);

            if (isFocusingOrFollowing)
            {
                HandleFocusAndFollow();
            }
            if (anyInteraction)
            {
                CalculateInertia();
            }
            else
            {
                ApplyInertia();
            }
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);


            // Apply Edge Boundaries
            if (IsUsingCameraEdgeBoundaries())
            {
                finalPosition = ClampCameraCorners(finalPosition, out bool clampApplied, true, currentPitch > 60);
                finalOffset = CalculateOffset(finalPosition, finalRotation, finalDistance, groundHeight);
                if (clampApplied) CalculateInertia();
            }

            ApplyToCamera();
        }

        */
        override protected void Update()
        {

            if (disableMoves)
                return;

            List<TouchFinger> twoFingers = CameraInputs.TwoFingerFilter.UpdateAndGetFingers();
            List<TouchFinger> oneFinger = CameraInputs.OneFingerFilter.UpdateAndGetFingers();

            Vector2 screenCenter = cam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));

            worldPointCameraCenter = ClampInCameraBoundaries(HeightScreenDepth.Convert(screenCenter));
            float pinchRatio = CameraInputs.pinchRatio;
            float scrollRatio = CameraInputs.GetScroll();

            //if (isFocusingOrFollowing)
            //HandleFocusAndFollow();


            if (IsInputMatching(InputMapFingerDrag.Translate))
            {
                print("Translate");
                if (!isHitting)
                {
                    FindGround(CameraInputs.screenPointAnyFingerCountCenter);
                }
                if (isHitting)
                {

                    worldPointCameraCenter = ClampInCameraBoundaries(HeightScreenDepth.Convert(screenCenter));
                    worldPointFingersCenter = ClampInCameraBoundaries(HeightScreenDepth.Convert(CameraInputs.screenPointAnyFingerCountCenter));

                    worldPointFingersDelta = Vector3.ClampMagnitude(HeightScreenDepth.ConvertDelta(CameraInputs.lastScreenPointAnyFingerCountCenter,
                        CameraInputs.screenPointAnyFingerCountCenter, gameObject), maxTranslationSpeed);

                    //Debug.Log("worldPointFingersCenter:" + worldPointFingersCenter + " worldPointFingersDelta:" + worldPointFingersDelta);

                    //if (disableTranslation)
                    //worldPointFingersDelta = Vector3.zero;

                    // pinch scale
                    Vector3 vecFingersCenterToCamera = (finalPosition - worldPointFingersCenter);
                    float vecFingersCenterToCameraDistance = vecFingersCenterToCamera.magnitude * pinchRatio;
                    vecFingersCenterToCamera = vecFingersCenterToCamera.normalized * vecFingersCenterToCameraDistance;

                    Vector3 targetPosition = worldPointFingersCenter + vecFingersCenterToCamera;

                    //Debug.Log("vecFingersCenterToCamera:" + vecFingersCenterToCamera + " targetPosition:" + targetPosition);

                    float belowGroundMultiplier = NormalizeAngle(finalRotation.eulerAngles.x) < 0 ? -1 : 1;
                    twistRot = Quaternion.AngleAxis(allowYawRotation ? CameraInputs.twistDegrees : 0, Vector3.up * belowGroundMultiplier);

                    Vector3 offsetFromFingerCenter = worldPointFingersCenter - worldPointFingersDelta;
                    //sphere.position = offsetFromFingerCenter;

                    finalPosition = twistRot * (targetPosition - worldPointFingersCenter) + offsetFromFingerCenter;
                    finalRotation = twistRot * finalRotation;

                    currentPitch = NormalizeAngle(finalRotation.eulerAngles.x);
                    currentYaw = (finalRotation.eulerAngles.y);
#if DAMPING_FEATURE_PREVIEW
                    dampedCurrentPitch = currentPitch;
                    dampedCurrentYaw = currentYaw;
#endif
                    //Vector3 newWorldPointCameraCenter = CalculateOffsetFromPosition(finalPosition, finalRotation, vecFingersCenterToCamera.magnitude, groundHeight);// CalculateNewCenter(finalPosition, finalRotation);
                    //Vector3 newWorldPointCameraCenter = CalculateNewCenter(finalPosition, finalRotation);
                    Vector3 newWorldPointCameraCenter = worldPointCameraCenter - worldPointFingersDelta * 1;
                    Vector3 newWorldPointCameraCenterClamped = ClampInCameraBoundaries(newWorldPointCameraCenter);



                    finalOffset = newWorldPointCameraCenterClamped;
                    finalDistance = CalculateClampedDistance(finalPosition, newWorldPointCameraCenter, minMaxDistance);
                    finalPosition = CalculatePosition(newWorldPointCameraCenterClamped, finalRotation, finalDistance);


                    CalculateInertia();

                }
            }
            else if (scrollRatio != 1)
            {
                finalOffset = worldPointCameraCenter;
                finalDistance = CalculateClampedDistance(finalPosition, worldPointCameraCenter, minMaxDistance, scrollRatio);
                finalPosition = CalculatePosition(worldPointCameraCenter, finalRotation, finalDistance);
            }
            else
            {
                isHitting = false;

                if (IsInputMatching(InputMapFingerDrag.RotateHead))
                {
                    RotateFromVector(CameraInputs.GetAnyPixelScaledDelta());
                    finalRotation = GetRotationFromPitchYaw();
                }
                else if (IsInputMatching(InputMapFingerDrag.RotateAround))
                {
                    RotateFromVector(CameraInputs.oneFingerScaledPixelDelta);
                    CalculateInertia();
                    finalRotation = GetRotationFromPitchYaw();
                    finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
                }
                else
                {
                    ApplyInertia();
                    finalRotation = GetRotationFromPitchYaw();
                    //finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
                }
            }

            if (isFocusingOrFollowing)
            {
                HandleFocusAndFollow();
            }


            ApplyToCamera();




        }


        protected override Vector3 GetRotateAroundVector()
        {
            float belowGroundMultiplier = NormalizeAngle(finalRotation.eulerAngles.x) < 0 ? -1 : 1;
            return Vector3.up * belowGroundMultiplier;
        }


        /// <summary>
        /// Try to find a collider as "ground" using the layer mask parameter
        /// </summary>
        /// <param name="screenPoint"></param>
        private void FindGround(Vector2 screenPoint)
        {
            Ray r = cam.ScreenPointToRay(screenPoint);
            isHitting = Physics.Raycast(r, out hitInfo, maxDistance, layerMask.value);
            if (isHitting)
            {
                plane.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
                plane.transform.position = hitInfo.point;
                groundHeight = hitInfo.point.y;
                HeightScreenDepth.Object = plane;
            }
            //print("isHitting:" + isHitting + " groundHeight:" + groundHeight);
        }

        private Vector3 FindHitPoint(Vector2 screenPoint)
        {
            Ray r = cam.ScreenPointToRay(screenPoint);
            if (Physics.Raycast(r, out hitInfo, maxDistance, layerMask.value))
            {
                return hitInfo.point;
            }
            return cam.transform.position + cam.transform.forward * 10;
        }
    }
}
