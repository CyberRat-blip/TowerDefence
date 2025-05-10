using System;
using Exoa.Common;
using Exoa.Events;
using Exoa.Maths;
using Exoa.Touch;
using UnityEngine;

namespace Exoa.Cameras
{
    public class CameraBase : MonoBehaviour, ITouchCamera
    {
        #region COMMON PARAMETERS

        public bool defaultMode;
        protected bool standalone;
        protected ScreenDepth HeightScreenDepth;

        protected Camera cam;
        protected CameraBoundaries camBounds;

        protected Vector3 initOffset;
        protected Quaternion initRotation;

        protected Vector3 finalOffset;
        protected Vector3 finalPosition;
        protected Quaternion finalRotation;
        protected float finalDistance;
        protected bool disableMoves;
        protected bool disableTranslations;
        protected bool disableRotations;

        protected float currentPitch;
        protected float currentYaw;
        protected float deltaYaw;
        protected float deltaPitch;

        protected Quaternion twistRot;

        protected Vector3 worldPointCameraCenter;
        protected Vector3 worldPointFingersCenter;
        protected Vector3 worldPointFingersDelta;

        [Header("INPUTS")]
        public InputMapFingerDrag rightClickDrag = InputMapFingerDrag.Translate;
        public InputMapFingerDrag middleClickDrag = InputMapFingerDrag.Translate;
        public InputMapFingerDrag oneFingerDrag = InputMapFingerDrag.RotateAround;
        public InputMapFingerDrag twoFingerDrag = InputMapFingerDrag.Translate;
        public InputMapFingerPinch twoFingerPinch = InputMapFingerPinch.ZoomAndRotate;
        public InputMapScrollWheel scrollWheel = InputMapScrollWheel.ZoomUnderMouse;

        [Header("GROUND HEIGHT")]
        public float groundHeight = 0f;
        public Springs groundHeightAnim;
        private FloatSpring groundHeightValue;

        public enum InputMapFingerDrag { Translate, RotateAround, RotateHead, None };
        public enum InputMapFingerPinch { ZoomAndRotate, RotateOnly, ZoomOnly, None };
        public enum InputMapScrollWheel { ZoomUnderMouse, ZoomInCenter, None };


        [Header("ROTATION")]
        public bool allowPitchRotation = true;
        public float PitchSensitivity = 0.25f;
        public bool PitchClamp = true;
        public Vector2 PitchMinMax = new Vector2(5.0f, 90.0f);
        protected Vector2 initialRotation = new Vector2(35, 0);
        public bool allowYawRotation = true;
        public float YawSensitivity = 0.25f;
        public bool YawClamp;
        public Vector2 YawMinMax = new Vector2(5.0f, 90.0f);

        [Header("MOVE")]
        public float maxTranslationSpeed = 3f;


        #endregion


        #region GETTER & SETTERS

        /// <summary>
        /// The final quaternion rotation of the camera transform
        /// </summary>
        public Quaternion FinalRotation
        {
            get => finalRotation;
        }
        /// <summary>
        /// The final position of the camera transform
        /// </summary>
        public Vector3 FinalPosition
        {
            get => finalPosition;
        }
        /// <summary>
        /// The final offset of the camera's center point on ground
        /// This is not the camera position. The center of the camera is projected 
        /// on the ground, and this is it's position.
        /// </summary>
        public Vector3 FinalOffset
        {
            get => finalOffset;
            set => finalOffset = value;
        }
        /// <summary>
        /// This is the final distance between the camera's center point on the ground,
        /// and the camera transform's position
        /// </summary>
        public float FinalDistance
        {
            get => finalDistance;
        }

        /// <summary>
        /// This blocks the user input moves (rotation and translation), in order to move/animate the camera by script
        /// </summary>
        public bool DisableMoves
        {
            get => disableMoves;
            set => disableMoves = value;
        }

        /// <summary>
        /// This blocks the user translation inputs  
        /// </summary>
        public bool DisableTranslations
        {
            get => disableTranslations;
            set => disableTranslations = value;
        }


        /// <summary>
        /// This blocks the user rotation inputs  
        /// </summary>
        public bool DisableRotations
        {
            get => disableRotations;
            set => disableRotations = value;
        }

        /// <summary>
        /// This returns the current Pitch and Yaw rotations of the camera
        /// </summary>
        public Vector2 PitchAndYaw
        {
            get => new Vector2(currentPitch, currentYaw);
        }

        /// <summary>
        /// Returns true if the camera is currently rotating
        /// </summary>
        /// <returns></returns>
        public bool IsRotating()
        {
            return !disableRotations && !disableMoves && (IsInputMatching(InputMapFingerDrag.RotateAround) ||
                IsInputMatching(InputMapFingerDrag.RotateHead) ||
                IsInputMatching(InputMapFingerPinch.RotateOnly));
        }

        /// <summary>
        /// Returns true if the camera is currently focusing or following an object
        /// </summary>
        public bool IsFocusingOrFollowing
        {
            get => isFocusingOrFollowing;
        }
        #endregion


        #region INIT / DESTROY / UPDATE

        /// <summary>
        /// Destroys the component
        /// </summary>
        virtual protected void OnDestroy()
        {
            CameraEvents.OnBeforeSwitchPerspective -= OnBeforeSwitchPerspective;
            CameraEvents.OnAfterSwitchPerspective -= OnAfterSwitchPerspective;
            CameraEvents.OnRequestButtonAction -= OnRequestButtonAction;
            CameraEvents.OnRequestObjectFocus -= FocusCameraOnGameObject;
            CameraEvents.OnRequestObjectFollow -= FollowGameObject;
            CameraEvents.OnRequestGroundHeightChange -= SetGroundHeightAnimated;
            CameraEvents.OnRequestStopFocusFollow -= StopFollow;
        }

        /// <summary>
        /// Starts the camera script
        /// </summary>
        virtual protected void Start()
        {
            cam = GetCamera();
            // Fix for jittering issues when camera is really high
            cam.nearClipPlane = 1f;

            camBounds = GetComponent<CameraBoundaries>();
            standalone = GetComponent<CameraModeSwitcher>() == null;
            Init();
            CameraEvents.OnBeforeSwitchPerspective += OnBeforeSwitchPerspective;
            CameraEvents.OnAfterSwitchPerspective += OnAfterSwitchPerspective;

            if (standalone)
            {
                CameraEvents.OnRequestButtonAction += OnRequestButtonAction;
                CameraEvents.OnRequestObjectFocus += FocusCameraOnGameObject;
                CameraEvents.OnRequestObjectFollow += FollowGameObject;
                CameraEvents.OnRequestGroundHeightChange += SetGroundHeightAnimated;
                CameraEvents.OnRequestStopFocusFollow += StopFollow;
            }
        }

        public Camera GetCamera()
        {
            if (cam != null)
                return cam;
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            return cam;
        }

        /// <summary>
        /// Init some camera parameters
        /// </summary>
        virtual protected void Init()
        {
            CreateConverter();
        }
        /// <summary>
        /// Create the depth converter between fingers on screen and the 3D World
        /// By default we use the scene y plan, to convert our finger's position
        /// </summary>
        virtual protected void CreateConverter()
        {
            HeightScreenDepth = new ScreenDepth(ScreenDepth.ConversionType.HeightIntercept, -5, groundHeight);
        }


        private bool firstUpdateDone;
        /// <summary>
        /// Some elements require a first Update call, so we need to know if it has been done
        /// </summary>
        virtual protected void LateUpdate()
        {
            if (!firstUpdateDone)
            {
                enabled = defaultMode || standalone;
                firstUpdateDone = true;
            }
#if DEBUG_TCP
            DebugInputs();
#endif
        }

        /// <summary>
        /// In case the camera is standalone (no CameraModeSwitcher) then this is apply 
        /// the position and rotation to the camera
        /// </summary>
        virtual protected void ApplyToCamera()
        {
            if (standalone)
            {
                try
                {
                    if (!float.IsNaN(FinalPosition.x) && !float.IsNaN(FinalPosition.y) && !float.IsNaN(FinalPosition.z))
                    {
                        transform.position = FinalPosition;
                    }
                }
                catch (System.Exception) { };

                if (!float.IsNaN(FinalRotation.x) && !float.IsNaN(FinalRotation.y) && !float.IsNaN(FinalRotation.z))
                {
                    transform.rotation = FinalRotation;
                }
            }
        }

        /// <summary>
        /// Return the matrix of the camera transform, in order to blend it when switching modes
        /// </summary>
        /// <returns></returns>
        virtual public Matrix4x4 GetMatrix()
        {
            return new Matrix4x4();
        }

        #endregion


        #region GROUND HEIGHT
        /// <summary>
        /// This let you change the ground height at any moment in order to
        /// change at which y position the fingers will be intercepted.
        /// This version lets you animate it
        /// </summary>
        /// <param name="newHeight"></param>
        /// <param name="animate"></param>
        /// <param name="duration"></param>
        public void SetGroundHeightAnimated(float newHeight, bool animate, float duration)
        {
            if (animate)
            {
                groundHeightValue.Reset(groundHeight);
                //groundHeight = groundHeightAnim.Update(ref groundHeightValue, newHeight);
                //SetGroundHeight(groundHeight);
            }
            else
            {
                SetGroundHeight(newHeight);
            }
        }

        /// <summary>
        /// This let you change the ground height at any moment in order to
        /// change at which y position the fingers will be intercepted
        /// </summary>
        /// <param name="y"></param>
        public void SetGroundHeight(float y)
        {
            groundHeight = y;
            HeightScreenDepth.Distance = groundHeight;
            finalOffset.y = groundHeight;
        }
        #endregion


        #region INPUTS
        /// <summary>
        /// Converts user inputs to actions
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected bool IsInputMatching(InputMapScrollWheel action)
        {
            if (scrollWheel == action && CameraInputs.GetScroll() != 1)
                return true;
            return false;
        }

        /// <summary>
        /// Converts user inputs to actions
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected bool IsInputMatching(InputMapFingerPinch action)
        {
            if (twoFingerPinch == action && CameraInputs.GetFingerCount() == 2)
                return true;
            return false;
        }

        /// <summary>
        /// Converts user inputs to actions
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected bool IsInputMatching(InputMapFingerDrag action)
        {
            if (action == InputMapFingerDrag.Translate && DisableTranslations)
                return false;
            if (action == InputMapFingerDrag.RotateAround && DisableRotations)
                return false;
            if (action == InputMapFingerDrag.RotateHead && DisableRotations)
                return false;

            if (middleClickDrag == action && BaseTouchInput.GetMouseIsHeld(2) && !Application.isMobilePlatform)
                return true;
            if (rightClickDrag == action && BaseTouchInput.GetMouseIsHeld(1) && !Application.isMobilePlatform)
                return true;
            if (twoFingerDrag == action && CameraInputs.GetFingerCount() == 2)
                return true;
            if (oneFingerDrag == action &&
                CameraInputs.GetFingerCount() == 1 &&
                !BaseTouchInput.GetMouseIsHeld(1) &&
                !BaseTouchInput.GetMouseIsHeld(2) &&
                !BaseTouchInput.GetMouseWentUp(1) &&
                !BaseTouchInput.GetMouseWentUp(2))
                return true;
            return false;
        }
        #endregion


        #region BOUNDARIES
        /// <summary>
        /// Is the camera using camera edge boundaries instead of center mode
        /// </summary>
        /// <returns></returns>
        protected bool IsUsingCameraEdgeBoundaries()
        {
            return camBounds != null && camBounds.IsUsingCameraEdgeBoundaries();
        }

        /// <summary>
        /// Method used to clamp the camera inside the boundaries collider
        /// </summary>
        /// <param name="finalPosition"></param>
        /// <param name="clampApplied"></param>
        /// <param name="bottomEdges"></param>
        /// <param name="topEdges"></param>
        /// <returns></returns>
        protected Vector3 ClampCameraCorners(Vector3 targetPosition, out bool clampApplied, float currentPitch)
        {

            bool anyFingerDown = CameraInputs.GetFingerCount() > 0;
            clampApplied = false;
            if (camBounds != null)
                return camBounds.ClampCameraCorners(targetPosition, out clampApplied,
                    currentPitch, HeightScreenDepth, cam, groundHeight, anyFingerDown);
            return targetPosition;
        }

        /// <summary>
        /// Clamp any given point inside the boundaries collider on XZ plan. Y will be unchanged
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        virtual protected Vector3 ClampInCameraBoundaries(Vector3 targetPosition)
        {
            return ClampInCameraBoundaries(targetPosition, out bool isInBoundaries);
        }

        /// <summary>
        /// Clamp any given point inside the boundaries collider on XZ plan. Y will be unchanged
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        virtual protected Vector3 ClampInCameraBoundaries(Vector3 targetPosition, out bool isInBoundaries)
        {
            bool anyFingerDown = CameraInputs.GetFingerCount() > 0;
            if (anyFingerDown) isApplyingMoveInertia = false;

            isInBoundaries = true;
            if (camBounds != null)
                return camBounds.ClampInBoundsXZ(targetPosition, out isInBoundaries, groundHeight, anyFingerDown || isApplyingMoveInertia);
            return targetPosition;
        }
        #endregion


        #region EVENTS
        /// <summary>
        /// Called just before the perspective switch happens
        /// </summary>
        /// <param name="orthoMode"></param>
        virtual protected void OnBeforeSwitchPerspective(bool orthoMode)
        {

        }

        /// <summary>
        /// Called just after the perspective switch happened
        /// </summary>
        /// <param name="orthoMode"></param>

        virtual protected void OnAfterSwitchPerspective(bool orthoMode)
        {

        }

        /// <summary>
        /// Catch event actions and interpret them
        /// </summary>
        /// <param name="action"></param>
        /// <param name="active"></param>
        protected void OnRequestButtonAction(CameraEvents.Action action, bool active)
        {
            if (action == CameraEvents.Action.ResetCameraPositionRotation)
                ResetCamera();
            else if (action == CameraEvents.Action.DisableCameraMoves)
                DisableMoves = (active);
        }
        #endregion


        #region POSITION
        /// <summary>
        /// Calculate the offset position on the ground, given the camera's position and rotation
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        virtual protected Vector3 CalculateOffset(Vector3 pos, Quaternion rot)
        {
            float adj = (pos.y - groundHeight) / Mathf.Tan(Mathf.Deg2Rad * rot.eulerAngles.x);
            Vector3 camForward = Quaternion.Euler(0, rot.eulerAngles.y, 0) * Vector3.forward;
            Vector3 camOffset = pos.SetY(groundHeight) + camForward.normalized * adj;
            return camOffset;
        }

        /// <summary>
        /// Calculate the offset position on the ground, given the camera's position, rotation, distance from ground and ground height
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="distance"></param>
        /// <param name="groundHeight"></param>
        /// <returns></returns>
        virtual protected Vector3 CalculateOffset(Vector3 pos, Quaternion rot, float distance, float groundHeight)
        {
            Vector3 offset = pos - rot * (Vector3.back * distance);
            return offset.SetY(groundHeight);
        }

        /// <summary>
        /// Calculates the camera transform's position giving the offset, rotation and distance
        /// </summary>
        /// <param name="center"></param>
        /// <param name="rot"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        virtual protected Vector3 CalculatePosition(Vector3 center, Quaternion rot, float distance)
        {
            return rot * (Vector3.back * distance) + center;
        }


        #endregion


        #region DISTANCE
        /// <summary>
        ///  Calculates the camera's distance from the ground, giving his transform position and rotation
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        protected float CalculateDistance(Vector3 pos, Quaternion rot)
        {
            float distance = Mathf.Abs((pos.y - groundHeight) / Mathf.Cos(Mathf.Deg2Rad * (90 - rot.eulerAngles.x)));
            return distance;
        }

        /// <summary>
        /// Calculates a clamped distance after a zoom
        /// </summary>
        /// <param name="camPos"></param>
        /// <param name="worldPoint"></param>
        /// <param name="minMaxDistance"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected float CalculateClampedDistance(Vector3 camPos, Vector3 worldPoint, Vector2 minMaxDistance, float multiplier = 1)
        {
            Vector3 distance = (camPos - worldPoint);
            return Mathf.Clamp(distance.magnitude * multiplier, minMaxDistance.x, minMaxDistance.y);
        }

        /// <summary>
        /// Calculates a clamped distance after a zoom
        /// </summary>
        /// <param name="camPos"></param>
        /// <param name="worldPoint"></param>
        /// <param name="minMaxDistance"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected float CalculateClampedDistance(Vector3 camPos, Vector3 worldPoint, float minMaxDistance, float multiplier = 1)
        {
            Vector3 distance = (camPos - worldPoint);
            return Mathf.Clamp(distance.magnitude * multiplier, minMaxDistance, minMaxDistance);
        }

        /// <summary>
        /// Calculates a clamped distance
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="minMaxDistance"></param>
        /// <returns></returns>
        protected float CalculateClampedDistance(float distance, Vector2 minMaxDistance)
        {
            return Mathf.Clamp(distance, minMaxDistance.x, minMaxDistance.y);
        }

        /// <summary>
        /// Calculates a clamped distance after a zoom
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="minMaxDistance"></param>
        /// <returns></returns>
        protected float CalculateClampedDistance(float distance, float minMaxDistance)
        {
            return Mathf.Clamp(distance, minMaxDistance, minMaxDistance);
        }

        /// <summary>
        /// Calculates a clamped distance, meant to be overriden
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="minMaxDistance"></param>
        /// <returns></returns>
        virtual protected float CalculateClampedDistance(float distance)
        {
            Debug.Log("CalculateClampedDistance needs to be overridden");
            return distance;
        }
        #endregion


        #region ROTATION
        /// <summary>
        /// Gives the initial rotation of the camera to be able to reset it later
        /// </summary>
        /// <returns></returns>
        virtual protected Quaternion GetInitialRotation()
        {
            return initRotation;
        }

        /// <summary>
        /// Gives the initial rotation as pitch/yaw vector of the camera to be able to reset it later
        /// </summary>
        /// <returns></returns>
        virtual protected Vector2 GetInitialRotationVec()
        {
            return initRotation.eulerAngles;
        }

        /// <summary>
        /// Calculate the sensivity of the rotation from user's input
        /// </summary>
        /// <returns></returns>
        protected float GetRotationSensitivity()
        {

            // Adjust sensitivity by FOV?
            if (cam.orthographic == false)
            {
                return cam.fieldOfView / 90.0f;
            }

            return 1.0f;
        }

        virtual protected Quaternion ClampRotation(Quaternion rot)
        {
            //Log("ClampRotation");
            Vector2 pitchYaw = GetRotationToPitchYaw(rot);
            pitchYaw = ClampPitchYaw(pitchYaw);
            return GetRotationFromPitchYaw(pitchYaw);
        }
        /// <summary>
        /// Clamp the rotation if needed
        /// </summary>
        virtual protected void ClampPitchYaw()
        {
            //Log("ClampPitchYaw2");
            if (PitchClamp)
                currentPitch = Mathf.Clamp(NormalizeAngle(currentPitch), PitchMinMax.x, PitchMinMax.y);
            if (YawClamp)
                currentYaw = Mathf.Clamp(NormalizeAngle(currentYaw), YawMinMax.x, YawMinMax.y);
        }

        /// <summary>
        /// Clamp the rotation if needed
        /// </summary>
        virtual protected Vector2 ClampPitchYaw(Vector2 pitchYaw)
        {
            //Log("ClampPitchYaw");
            if (PitchClamp)
                pitchYaw.x = Mathf.Clamp(NormalizeAngle(pitchYaw.x), PitchMinMax.x, PitchMinMax.y);
            if (YawClamp)
                pitchYaw.y = Mathf.Clamp(NormalizeAngle(pitchYaw.y), YawMinMax.x, YawMinMax.y);
            return pitchYaw;
        }


        /// <summary>
        /// Rotate the camera manually
        /// </summary>
        /// <param name="delta">the increment values (pitch, yaw)</param>
        public void RotateFromVector(Vector2 delta)
        {
            var sensitivity = GetRotationSensitivity();
            if (allowYawRotation)
            {
                deltaYaw = delta.x * YawSensitivity * sensitivity;
                currentYaw += deltaYaw;
            }
            if (allowPitchRotation)
            {
                deltaPitch = -delta.y * PitchSensitivity * sensitivity;
                currentPitch += deltaPitch;
            }
            ClampPitchYaw();
        }

        /// <summary>
        /// Useful to clamp a rotation between two angles
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="rangemin"></param>
        /// <param name="rangemax"></param>
        /// <returns></returns>
        protected float ModularClamp(float val, float min, float max, float rangemin = -180f, float rangemax = 180f)
        {
            var modulus = Mathf.Abs(rangemax - rangemin);
            if ((val %= modulus) < 0f) val += modulus;
            return Mathf.Clamp(val + Mathf.Min(rangemin, rangemax), min, max);
        }


        /// <summary>
        /// Normalize an angle between -180 and 180
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        protected float NormalizeAngle(float a)
        {
            if (a > 180) a -= 360;
            if (a < -180) a += 360;
            return a;
        }

        /// <summary>
        /// Converts pitch/yaw rotations to a quaternion
        /// </summary>
        /// <returns></returns>
#if DAMPING_FEATURE_PREVIEW
        protected float dampedCurrentPitch;
        protected float dampedCurrentYaw;
        protected float previousYaw;
        public float rotationDamping = .02f;
#endif
        virtual protected Quaternion GetRotationFromPitchYaw()
        {
            //Log("GetRotationFromPitchYaw");
#if DAMPING_FEATURE_PREVIEW
            // DAMPING TESTS
            if (dampedCurrentPitch - currentPitch > 180) dampedCurrentPitch -= 360;
            else if (dampedCurrentPitch - currentPitch < -180) dampedCurrentPitch += 360;
            if (dampedCurrentYaw - currentYaw > 180) dampedCurrentYaw -= 360;
            else if (dampedCurrentYaw - currentYaw < -180) dampedCurrentYaw += 360;

            if (dampedCurrentPitch == 0) dampedCurrentPitch = currentPitch;
            if (dampedCurrentYaw == 0) dampedCurrentYaw = currentYaw;

            dampedCurrentPitch = (Mathf.Lerp(dampedCurrentPitch, (currentPitch), rotationDamping));
            dampedCurrentYaw = (Mathf.Lerp(dampedCurrentYaw, (currentYaw), rotationDamping));
            return Quaternion.Euler(dampedCurrentPitch, dampedCurrentYaw, 0);
#else
            currentPitch = NormalizeAngle(currentPitch);
            currentYaw = NormalizeAngle(currentYaw);
            return Quaternion.Euler(currentPitch, currentYaw, 0);

#endif


        }

        /// <summary>
        /// Converts pitch/yaw rotations to a quaternion
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        /// <returns></returns>
        virtual protected Quaternion GetRotationFromPitchYaw(float pitch, float yaw)
        {
            //Log("GetRotationFromPitchYaw2");

            pitch = NormalizeAngle(pitch);
            yaw = NormalizeAngle(yaw);

            return Quaternion.Euler(pitch, yaw, 0);
        }

        /// <summary>
        /// Converts pitch/yaw rotations to a quaternion
        /// </summary>
        /// <param name="pitchYawVec"></param>
        /// <returns></returns>
        virtual protected Quaternion GetRotationFromPitchYaw(Vector2 pitchYawVec)
        {
            //Log("GetRotationFromPitchYaw3");

            pitchYawVec.x = NormalizeAngle(pitchYawVec.x);
            pitchYawVec.y = NormalizeAngle(pitchYawVec.y);

            return Quaternion.Euler(pitchYawVec.x, pitchYawVec.y, 0);
        }

        /// <summary>
        /// Converts the quaternion rotation to pitch/yaw values
        /// </summary>
        /// <returns></returns>
        virtual protected Vector2 GetRotationToPitchYaw()
        {
            //Log("GetRotationToPitchYaw");
            return new Vector2(finalRotation.eulerAngles.x, finalRotation.eulerAngles.y);
        }

        /// <summary>
        /// Converts the quaternion rotation to pitch/yaw values
        /// </summary>
        /// <param name="rot"></param>
        /// <returns></returns>
        virtual protected Vector2 GetRotationToPitchYaw(Quaternion rot)
        {
            //Log("GetRotationToPitchYaw2");
            return new Vector2(NormalizeAngle(rot.eulerAngles.x), rot.eulerAngles.y);
        }
        #endregion


        #region RESET

        /// <summary>
        /// Reset the camera to initial values
        /// </summary>
        virtual public void ResetCamera()
        {
            throw new Exception("ResetCamera needs to be overridden!");
        }

        /// <summary>
        /// Set the initial values for the reset function
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="rotation"></param>
        /// <param name="distanceOrSize"></param>
        virtual public void SetResetValues(Vector3 offset, Quaternion rotation, float distanceOrSize)
        {
            throw new Exception("SetResetValues needs to be overridden!");
        }

        /// <summary>
        /// indicates the vector for AngleAxis, to rotate around
        /// </summary>
        /// <returns></returns>
        virtual protected Vector3 GetRotateAroundVector()
        {
            return Vector3.up;
        }
        #endregion


        #region MOVE

        /// <summary>
        /// Moves the Camera transform position to a new position, in 1 frame
        /// </summary>
        /// <param name="targetCamPos"></param>
        public void RelocateCameraInstant(Vector3 targetCamPos)
        {
            finalPosition = targetCamPos;
            finalOffset = CalculateOffset(finalPosition, finalRotation);
            finalDistance = CalculateDistance(finalPosition, finalRotation);

            ApplyToCamera();
        }

        public void RotateAround(Vector2 viewportPoint, float angle)
        {
            Quaternion twistRot = Quaternion.AngleAxis(angle, GetRotateAroundVector());
            Vector2 screenPoint = cam.ViewportToScreenPoint(viewportPoint);
            Vector3 worldPointFingersCenter = ClampInCameraBoundaries(HeightScreenDepth.Convert(screenPoint));
            Vector3 vecFingersCenterToCamera = (finalPosition - worldPointFingersCenter);
            float vecFingersCenterToCameraDistance = vecFingersCenterToCamera.magnitude * 1f;
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

            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);

            ApplyToCamera();
        }

        /// <summary>
        /// Moves the Camera offset position on ground to a new position, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        public void MoveCameraToInstant(Vector3 targetOffset)
        {
            //FocusCamera(targetPosition, true);
            finalOffset = ClampInCameraBoundaries(targetOffset);
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);

            ApplyToCamera();
        }


        /// <summary>
        /// Moves the Camera offset position on ground and distance from it, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistanceOrSize"></param>
        virtual public void MoveCameraToInstant(float targetDistanceOrSize)
        {
            //FocusCamera(targetPosition, targetDistanceOrSize, true);
            finalDistance = CalculateClampedDistance(targetDistanceOrSize);
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
            //Debug.Log("MoveCameraToInstant targetDistanceOrSize:" + targetDistanceOrSize +
            //    " finalDistance:" + finalDistance +
            //    " finalPosition:" + FinalPosition);

            ApplyToCamera();
        }

        /// <summary>
        /// Moves the Camera offset position on ground and distance from it, in 1 frame
        /// </summary>
        /// <param name="targetOffset"></param>
        /// <param name="targetDistanceOrSize"></param>
        virtual public void MoveCameraToInstant(Vector3 targetOffset, float targetDistanceOrSize)
        {
            //FocusCamera(targetPosition, targetDistanceOrSize, true);
            finalOffset = ClampInCameraBoundaries(targetOffset);
            finalDistance = CalculateClampedDistance(targetDistanceOrSize);
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);

            ApplyToCamera();
        }



        /// <summary>
        /// Moves the Camera offset position on ground and rotation, in 1 frame
        /// </summary>
        /// <param name="targetOffset"></param>
        /// <param name="targetRotation"></param>
        virtual public void MoveCameraToInstant(Vector3 targetOffset, Vector2 targetRotation)
        {
            //FocusCamera(targetPosition, targetRotation, true);
            finalRotation = GetRotationFromPitchYaw(targetRotation);
            finalOffset = ClampInCameraBoundaries(targetOffset);
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
            finalDistance = CalculateDistance(finalPosition, finalRotation);

            ApplyToCamera();
        }

        /// <summary>
        /// Moves the Camera offset position on ground and rotation, in 1 frame
        /// </summary>
        /// <param name="targetOffset"></param>
        /// <param name="targetRotation"></param>
        virtual public void MoveCameraToInstant(Vector3 targetOffset, Quaternion targetRotation)
        {
            finalRotation = ClampRotation(targetRotation);
            finalOffset = ClampInCameraBoundaries(targetOffset);
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);
            finalDistance = CalculateDistance(finalPosition, finalRotation);

            ApplyToCamera();
        }

        /// <summary>
        /// Moves the Camera offset position on ground and rotation, in 1 frame
        /// </summary>
        /// <param name="targetOffset"></param>
        /// <param name="targetRotation"></param>
        virtual public void MoveCameraToInstant(Quaternion targetRotation)
        {
            finalRotation = ClampRotation(targetRotation);
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);

            ApplyToCamera();
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, in 1 frame
        /// </summary>
        /// <param name="targetOffset"></param>
        /// <param name="targetDistanceOrSize"></param>
        /// <param name="targetRotation"></param>
        virtual public void MoveCameraToInstant(Vector3 targetOffset, float targetDistanceOrSize, Vector2 targetRotation)
        {
            //FocusCamera(targetPosition, targetDistanceOrSize, targetRotation, true);
            finalRotation = GetRotationFromPitchYaw(targetRotation);
            finalRotation = ClampRotation(finalRotation);
            finalOffset = ClampInCameraBoundaries(targetOffset);
            finalDistance = CalculateClampedDistance(targetDistanceOrSize);
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);

            ApplyToCamera();
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistanceOrSize"></param>
        /// <param name="targetRotation"></param>
        virtual public void MoveCameraToInstant(Vector3 targetOffset, float targetDistanceOrSize, Quaternion targetRotation)
        {
            //FocusCamera(targetOffset, targetDistanceOrSize, targetRotation, true);
            finalRotation = ClampRotation(targetRotation);
            finalOffset = ClampInCameraBoundaries(targetOffset);
            finalDistance = CalculateClampedDistance(targetDistanceOrSize);
            finalPosition = CalculatePosition(finalOffset, finalRotation, finalDistance);

            ApplyToCamera();
        }

        /// <summary>
        /// Moves the Camera offset position on ground, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        public void MoveCameraTo(Vector3 targetPosition)
        {
            FocusCamera(targetPosition);
        }

        /// <summary>
        /// Moves the Camera rotation, animated
        /// </summary>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Quaternion targetRotation)
        {
            FocusCamera(false, Vector3.zero, false, 0, true, targetRotation);
        }

        /// <summary>
        /// Moves the Camera distance form the ground, animated
        /// </summary>
        /// <param name="targetDistanceOrSize"></param>
        public void MoveCameraTo(float targetDistanceOrSize)
        {
            FocusCamera(false, Vector3.zero, true, targetDistanceOrSize, false, Quaternion.identity);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistanceOrSize"></param>
        public void MoveCameraTo(Vector3 targetPosition, float targetDistanceOrSize)
        {
            FocusCamera(targetPosition, targetDistanceOrSize);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Vector3 targetPosition, Vector2 targetRotation)
        {
            FocusCamera(targetPosition, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Vector3 targetPosition, Quaternion targetRotation)
        {
            FocusCamera(targetPosition, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistanceOrSize"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Vector3 targetPosition, float targetDistanceOrSize, Vector2 targetRotation)
        {
            FocusCamera(targetPosition, targetDistanceOrSize, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistanceOrSize"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Vector3 targetPosition, float targetDistanceOrSize, Quaternion targetRotation)
        {
            FocusCamera(targetPosition, targetDistanceOrSize, targetRotation);
        }

        #endregion

        #region FOCUS & FOLLOW
        [Header("FOCUS")]
        public float focusRadiusMultiplier = 1f;
        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, bool instant = false)
        {
            FocusCamera(true, targetPosition, false, -1, false, Quaternion.identity, true, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistanceOrSize"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, float targetDistanceOrSize, bool instant = false)
        {
            FocusCamera(true, targetPosition, true, targetDistanceOrSize, false, Quaternion.identity, true, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, Vector2 targetRotation, bool instant = false)
        {
            FocusCamera(true, targetPosition, false, -1, true, GetRotationFromPitchYaw(targetRotation), true, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, Quaternion targetRotation, bool instant = false)
        {
            FocusCamera(true, targetPosition, false, -1, true, targetRotation, true, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistanceOrSize"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, float targetDistanceOrSize, Vector2 targetRotation, bool instant = false)
        {
            FocusCamera(true, targetPosition, true, targetDistanceOrSize, true, GetRotationFromPitchYaw(targetRotation), true, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistanceOrSize"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, float targetDistanceOrSize, Quaternion targetRotation, bool instant = false)
        {
            FocusCamera(true, targetPosition, true, targetDistanceOrSize, true, targetRotation, true, instant);
        }

        /// <summary>
        /// Setup the camera move animation
        /// </summary>
        /// <param name="targetOffsetPosition"></param>
        /// <param name="changeDistance"></param>
        /// <param name="targetDistanceOrSize"></param>
        /// <param name="changeRotation"></param>
        /// <param name="targetRotation"></param>
        /// <param name="allowYOffsetFromGround"></param>
        /// <param name="instant"></param>
        virtual protected void FocusCamera(
           bool changeOffsetPostion, Vector3 targetOffsetPosition,
           bool changeDistance, float targetDistanceOrSize,
           bool changeRotation, Quaternion targetRotation,
           bool allowYOffsetFromGround = false,
           bool instant = false)
        {
            Log("FocusCamera(offset:" + targetOffsetPosition +
                " changeDistance:" + changeDistance + " dist:" + targetDistanceOrSize +
                " changeRotation:" + changeRotation + " rot:" + targetRotation.eulerAngles +
           " allowYOffset:" + allowYOffsetFromGround +
           " instant:" + instant, this);

            this.focusTargetPosition = ClampInCameraBoundaries(targetOffsetPosition);
            this.focusTargetDistanceOrSize = targetDistanceOrSize;
            this.focusTargetRotation = ClampRotation(targetRotation);

            this.isFocusingOrFollowing = true;
            this.enableFollowGameObject = false;
            this.enablePositionChange = changeOffsetPostion;
            this.enableDistanceChange = changeDistance;
            this.enableRotationChange = changeRotation;
            this.allowYOffsetFromGround = allowYOffsetFromGround;


            if (!enablePositionChange) followMoveOffset.Completed = true;
            if (!enableDistanceChange) followMoveDistanceOrSize.Completed = true;
            if (!enableRotationChange) followMoveRotation.Completed = true;


            CameraEvents.OnFocusStart?.Invoke();
        }

        /// <summary>
        /// Focus the camera on a GameObject (distance animation)
        /// </summary>
        /// <param name="go">The gameObject to get closer to</param>
        /// <param name="allowYOffsetFromGround">Allow offseting the camera from the ground to match the object's pivot y position and height</param>
        virtual public void FocusCameraOnGameObject(GameObject go, bool allowYOffsetFromGround = false)
        {
            Log("FocusCameraOnGameObject(go:" + go +
               " allowYOffsetFromGround:" + allowYOffsetFromGround, this);

            this.focusTargetGo = go;
            this.isFocusingOrFollowing = this.focusTargetGo != null;
            this.enableFollowGameObject = this.focusTargetGo != null;
            this.enablePositionChange = true;
            this.enableDistanceChange = true;
            this.enableRotationChange = false;
            this.allowYOffsetFromGround = allowYOffsetFromGround;


            if (!enablePositionChange) followMoveOffset.Completed = true;
            if (!enableDistanceChange) followMoveDistanceOrSize.Completed = true;
            if (!enableRotationChange) followMoveRotation.Completed = true;

            if (go != null) CameraEvents.OnFocusStart?.Invoke();
        }

        [Header("FOLLOW")]
        public Springs followMove;
        public FloatSpring followMoveDistanceOrSize;
        public Vector3Spring followMoveOffset;
        public QuaternionSpring followMoveRotation;

        protected GameObject focusTargetGo;
        protected Vector3 focusTargetPosition;
        protected float focusTargetDistanceOrSize;
        protected Quaternion focusTargetRotation;

        protected bool enableDistanceChange;
        protected bool enablePositionChange;
        protected bool enableRotationChange;
        protected bool enableFollowGameObject;
        protected bool isFocusingOrFollowing;
        protected bool allowYOffsetFromGround;

        /// <summary>
        /// Follow a game object
        /// </summary>
        /// <param name="go">The game object to follow</param>
        /// <param name="doFocus">Also focus on it (distance animation)</param>
        /// <param name="allowYOffsetFromGround">Allow offseting the camera from the ground to match the object's pivot y position and height</param>
        virtual public void FollowGameObject(GameObject go, bool doFocus, bool allowYOffsetFromGround = false)
        {
            Log("FollowGameObject(go:" + go + " doFocus:" + doFocus +
               " allowYOffsetFromGround:" + allowYOffsetFromGround, this);

            this.focusTargetGo = go;
            this.isFocusingOrFollowing = this.focusTargetGo != null;
            this.enableFollowGameObject = this.focusTargetGo != null;
            this.enablePositionChange = true;
            this.enableDistanceChange = doFocus;
            this.enableRotationChange = false;
            this.allowYOffsetFromGround = allowYOffsetFromGround;


            if (!enablePositionChange) followMoveOffset.Completed = true;
            if (!enableDistanceChange) followMoveDistanceOrSize.Completed = true;
            if (!enableRotationChange) followMoveRotation.Completed = true;

            if (go != null) CameraEvents.OnFocusStart?.Invoke();
        }

        /// <summary>
        /// Stop follow/focus/moveto animations
        /// </summary>
        public void StopFollow()
        {
            FollowGameObject(null, false);
        }


        virtual protected void OnFollowFocusCompleted()
        {
            bool distanceCompleted = !enableDistanceChange || followMoveDistanceOrSize.Completed;
            bool rotationCompleted = !enableRotationChange || followMoveRotation.Completed;
            bool moveCompleted = !enablePositionChange || followMoveOffset.Completed;

            Log("OnFollowFocusCompleted move:" + moveCompleted +
                " rotation:" + rotationCompleted +
                " distance:" + distanceCompleted);


            if (distanceCompleted && rotationCompleted && moveCompleted)
            {
                StopFollow();
                CameraEvents.OnFocusComplete?.Invoke(focusTargetGo);
            }
        }

        #endregion


        #region INERTIA
        [Header("INERTIA")]
        public bool enableTranslationInertia;
        protected bool prevFrameInterction;
        public Move positionInertiaMove;
        public Acceleration positionAcceleration;
        [Range(0.01f, 50)]
        public float positionInertiaForce = 1f;
        protected bool isApplyingMoveInertia;


        public bool enableRotationInertia;

        public Move rotationInertiaMove;
        public Acceleration rotationAcceleration;
        [Range(0.01f, 100)]
        public float rotationInertiaForce = 1f;


        /// <summary>
        /// Calculate the inertia of the camera base on previous frames
        /// </summary>
        virtual protected void CalculateInertia()
        {
            //print("CalculateInertia prevFrameInterction:" + prevFrameInterction);
            if (prevFrameInterction == false)
            {
                positionAcceleration.Clear();
                rotationAcceleration.Clear();
                prevFrameInterction = true;
            }
            positionAcceleration.AddPosition(finalOffset);
            rotationAcceleration.AddPosition(new Vector2(currentPitch, currentYaw), false, false, true);
            isApplyingMoveInertia = false;
        }


        /// <summary>
        /// Clear the inertia of the camera
        /// </summary>
        virtual protected void ClearInertia()
        {
            positionAcceleration.Clear();
            rotationAcceleration.Clear();
            prevFrameInterction = true;
            isApplyingMoveInertia = false;
        }



        /// <summary>
        /// Apply the inertia animation
        /// </summary>
        virtual protected void ApplyInertia()
        {
            //print("ApplyInertia prevFrameInterction:" + prevFrameInterction);
            if (prevFrameInterction == true)
            {
                positionAcceleration.AddPosition(finalOffset, false, true);
                rotationAcceleration.AddPosition(new Vector2(currentPitch, currentYaw), false, true);
                //print("positionAcceleration.OutputAcceleration:" + positionAcceleration.OutputAcceleration +
                //" magnitude:" + positionAcceleration.OutputAcceleration.magnitude);

                //print("rotationAcceleration.OutputAcceleration:" + rotationAcceleration.OutputAcceleration +
                //" magnitude:" + rotationAcceleration.OutputAcceleration.magnitude);

                positionInertiaMove.Init(positionAcceleration.OutputAcceleration.magnitude * positionInertiaForce,
                    positionAcceleration.OutputAcceleration.normalized);

                rotationInertiaMove.Init(rotationAcceleration.OutputAcceleration.magnitude * rotationInertiaForce,
                    rotationAcceleration.OutputAcceleration.normalized);

                prevFrameInterction = false;
            }

            if (enableTranslationInertia)
            {
                Vector3 posInertia = positionInertiaMove.Update();
                if (posInertia != Vector3.zero)
                {
                    isApplyingMoveInertia = true;
                    finalOffset += posInertia;
                    finalOffset = ClampInCameraBoundaries(finalOffset, out bool IsInBoundaries);

                }
                else
                {
                    isApplyingMoveInertia = false;
                }
            }
            else
            {
                isApplyingMoveInertia = false;
            }

            if (enableRotationInertia)
            {

                Vector3 rotInertia = rotationInertiaMove.Update();
                currentPitch += rotInertia.x;
                currentYaw += rotInertia.y;
                ClampPitchYaw();
                finalRotation = GetRotationFromPitchYaw();
                //print("rotInertia:" + rotInertia);

            }


        }
        #endregion


        #region DEBUG
#if DEBUG_TCP
        [Header("DEBUG")]
        protected string dbg;
#endif
        protected float lastPrint;
        /// <summary>
        /// Debug feature if DEBUG_TCP compilation variable is added
        /// </summary>
        protected void DebugInputs()
        {
#if DEBUG_TCP
            if (lastPrint > Time.time - 1)
                return;

            lastPrint = Time.time;

            dbg = ("[TCP] INPUT DETAILS ");
            dbg += "\n" + ("GetFingerCount " + CameraInputs.GetFingerCount());
            dbg += "\n" + ("GetMouseButton(0) " + BaseTouchInput.GetMouseIsHeld(0));
            dbg += "\n" + ("GetMouseButton(1) " + BaseTouchInput.GetMouseIsHeld(1));
            dbg += "\n" + ("GetMouseButton(2) " + BaseTouchInput.GetMouseIsHeld(2));
            dbg += "\n" + ("IsInputMatching Rotate " + IsInputMatching(InputMapFingerDrag.RotateAround));
            dbg += "\n" + ("IsInputMatching Translate " + IsInputMatching(InputMapFingerDrag.Translate));
            dbg += "\n" + ("oneFingerDrag " + oneFingerDrag);
            dbg += "\n" + ("twoFingerDrag " + twoFingerDrag);
            dbg += "\n" + ("isMobilePlatform " + Application.isMobilePlatform);
            dbg += "\n" + ("UseMouse " + InputTouch.Instance.UseMouse);
            dbg += "\n" + ("UseSimulator " + InputTouch.Instance.UseSimulator);

            if (!string.IsNullOrEmpty(dbg)) Debug.Log(dbg);
#endif
        }

        protected void Log(string msg, UnityEngine.Object context = null)
        {
#if DEBUG_TCP
            Debug.Log("[TCP] " + msg, context);
#endif
        }
        #endregion
    }
}

