using Exoa.Designer;
using Exoa.Events;
using Exoa.Maths;
using UnityEngine;

namespace Exoa.Cameras
{
    public class CameraModeSwitcher : MonoBehaviour, ITouchCamera
    {
        public static CameraModeSwitcher Instance;

        private CameraOrthoBase camOrtho;
        private CameraPerspBase camPersp;
        private Camera cam;

        private float matrixLerp; // 0 for ortho, 1 for perspective
        private bool orthoMode = true;
        private Matrix4x4 orthoMatrix, perspectiveMatrix;

        public Springs switchMove;
        public FloatSpring switchMoveLerp;

        /// <summary>
        /// Returns the current active camera mode
        /// </summary>
        public ITouchCamera CurrentCameraMode
        {
            get
            {
                if (orthoMode)
                    return camOrtho;
                return camPersp;
            }
        }

        /// <summary>
        /// This blocks the user input moves, in order to animate the camera by script
        /// </summary>
        public bool DisableMoves
        {
            get
            {
                return CurrentCameraMode.DisableMoves;
            }
            set
            {
                camOrtho.DisableMoves = value;
                camPersp.DisableMoves = value;
            }
        }


        /// <summary>
        /// This blocks the user translation inputs
        /// </summary>
        public bool DisableTranslations
        {
            get
            {
                return CurrentCameraMode.DisableTranslations;
            }
            set
            {
                camOrtho.DisableTranslations = value;
                camPersp.DisableTranslations = value;
            }
        }


        /// <summary>
        /// This blocks the user rotation inputs
        /// </summary>
        public bool DisableRotations
        {
            get
            {
                return CurrentCameraMode.DisableRotations;
            }
            set
            {
                camOrtho.DisableRotations = value;
                camPersp.DisableRotations = value;
            }
        }

        /// <summary>
        /// The final quaternion rotation of the camera transform
        /// </summary>
        public Quaternion FinalRotation => CurrentCameraMode.FinalRotation;

        /// <summary>
        /// The final position of the camera transform
        /// </summary>
        public Vector3 FinalPosition => CurrentCameraMode.FinalPosition;

        /// <summary>
        /// The final offset of the camera's center point on ground
        /// This is not the camera position. The center of the camera is projected 
        /// on the ground, and this is it's position.
        /// </summary>
        public Vector3 FinalOffset => CurrentCameraMode.FinalOffset;

        /// <summary>
        /// This is the final distance between the camera's center point on the ground,
        /// and the camera transform's position
        /// </summary>
        public float FinalDistance => CurrentCameraMode.FinalDistance;

        /// <summary>
        /// This returns the current Pitch and Yaw rotations of the camera
        /// </summary>
        public Vector2 PitchAndYaw => CurrentCameraMode.PitchAndYaw;

        /// <summary>
        /// Returns true if the camera is currently rotating
        /// </summary>
        public bool IsRotating() => CurrentCameraMode.IsRotating();

        void OnDestroy()
        {
            CameraEvents.OnRequestButtonAction -= OnRequestButtonAction;
            CameraEvents.OnRequestObjectFocus -= FocusCameraOnGameObject;
            CameraEvents.OnRequestObjectFollow -= FollowGameObject;
            CameraEvents.OnRequestGroundHeightChange -= SetGroundHeightAnimated;
            CameraEvents.OnRequestStopFocusFollow -= StopFollow;
        }



        void Awake()
        {
            Instance = this;
            camOrtho = GetComponent<CameraOrthoBase>();
            camPersp = GetComponent<CameraPerspBase>();
            cam = camPersp.GetCamera();

            if (camOrtho.defaultMode && camPersp.defaultMode)
            {
                Debug.LogError("Error: only one camera mode can be marked as defaultMode!");
            }
            if (!camOrtho.defaultMode && !camPersp.defaultMode)
            {
                Debug.LogError("Error: no camera mode is marked as defaultMode!");
                camOrtho.defaultMode = true;
            }

            orthoMode = camOrtho.defaultMode;
            matrixLerp = orthoMode ? 0 : 1;
            switchMoveLerp.Reset(matrixLerp);

            CameraEvents.OnBeforeSwitchPerspective?.Invoke(orthoMode);
            CameraEvents.OnAfterSwitchPerspective?.Invoke(orthoMode);
            CameraEvents.OnRequestButtonAction += OnRequestButtonAction;
            CameraEvents.OnRequestObjectFocus += FocusCameraOnGameObject;
            CameraEvents.OnRequestObjectFollow += FollowGameObject;
            CameraEvents.OnRequestGroundHeightChange += SetGroundHeightAnimated;
            CameraEvents.OnRequestStopFocusFollow += StopFollow;
        }


        /// <summary>
        /// Process the camera mode switch 
        /// and apply the mode's param to the camera using its matrix
        /// </summary>
        void LateUpdate()
        {
            orthoMatrix = camOrtho.GetMatrix();
            perspectiveMatrix = camPersp.GetMatrix();

            if (CameraInputs.Instance != null && CameraInputs.Instance.ChangePerspective())
            {
                TogglePerspective();
            }
            matrixLerp = switchMove.Update(ref switchMoveLerp, (orthoMode ? 0 : 1), () => OnAfterSwitch(orthoMode));
            Matrix4x4 mergedMatrix = MatrixLerp(orthoMatrix, perspectiveMatrix, matrixLerp);
            cam.projectionMatrix = mergedMatrix;


            try
            {
                Quaternion rotation = Quaternion.Lerp(camOrtho.FinalRotation, camPersp.FinalRotation, matrixLerp);
                transform.rotation = rotation;
            }
            catch (System.Exception) { };

            Vector3 position = Vector3.Lerp(camOrtho.FinalPosition, camPersp.FinalPosition, matrixLerp);
            if (!float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z))
            {
                transform.position = position;
            }
        }

        /// <summary>
        /// Alias for DisableMoves setter
        /// </summary>
        /// <param name="active"></param>
        public void DisableCameraMoves(bool active)
        {
            camOrtho.DisableMoves = active;
            camPersp.DisableMoves = active;
        }

        /// <summary>
        /// Alias for DisableTranslations setter
        /// </summary>
        /// <param name="active"></param>
        public void DisableCameraTranslations(bool active)
        {
            camOrtho.DisableTranslations = active;
            camPersp.DisableTranslations = active;
        }

        /// <summary>
        /// Alias for DisableRotations setter
        /// </summary>
        /// <param name="active"></param>
        public void DisableCameraRotations(bool active)
        {
            camOrtho.DisableRotations = active;
            camPersp.DisableRotations = active;
        }

        #region RESET
        /// <summary>
        /// Reset the camera to initial values
        /// </summary>
        public void ResetCamera()
        {
            StopFollow();
            CurrentCameraMode.ResetCamera();
        }

        /// <summary>
        /// Set the initial values for the reset function
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="rotation"></param>
        /// <param name="distanceOrSize"></param>
        public void SetResetValues(Vector3 offset, Quaternion rotation, float distance, float size)
        {
            camOrtho.SetResetValues(offset, rotation, size);
            camPersp.SetResetValues(offset, rotation, distance);
        }
        #endregion


        #region EVENTS
        /// <summary>
        /// Called just before the perspective switch happens
        /// </summary>
        /// <param name="orthoMode"></param>
        private void OnBeforeSwitch(bool orthoOn)
        {
            if (orthoOn)
            {
                camOrtho.FinalOffset = camPersp.FinalOffset;
                camOrtho.SetSizeByDistance(camPersp.FinalDistance);
                camOrtho.SetPositionByOffset();
                camPersp.enabled = false;
            }
            else
            {
                camPersp.FinalOffset = camOrtho.FinalOffset;
                camPersp.SetPositionByDistance(camOrtho.GetDistanceFromSize());
                camOrtho.enabled = false;
            }
            CameraEvents.OnBeforeSwitchPerspective?.Invoke(orthoMode);
        }

        /// <summary>
        /// Called just after the perspective switch happened
        /// </summary>
        /// <param name="orthoMode"></param>
        private void OnAfterSwitch(bool orthoOn)
        {
            if (!orthoOn)
            {
                camPersp.enabled = true;
                cam.orthographic = false;
            }
            else
            {
                camOrtho.enabled = true;
                cam.orthographic = true;
            }
            CameraEvents.OnAfterSwitchPerspective?.Invoke(orthoMode);
        }

        /// <summary>
        /// Catch event actions and interpret them
        /// </summary>
        /// <param name="action"></param>
        /// <param name="active"></param>
        private void OnRequestButtonAction(CameraEvents.Action action, bool active)
        {
            if (action == CameraEvents.Action.ForcePerspectiveMode && orthoMode)
                TogglePerspective();
            else if (action == CameraEvents.Action.SwitchPerspective)
                TogglePerspective();
            else if (action == CameraEvents.Action.ResetCameraPositionRotation)
                ResetCamera();
            else if (action == CameraEvents.Action.DisableCameraMoves)
                DisableCameraMoves(active);
        }

        /// <summary>
        /// Toggle from one mode to the other
        /// </summary>
        private void TogglePerspective()
        {
            orthoMode = !orthoMode;
            OnBeforeSwitch(orthoMode);

        }
        #endregion

        #region UTILS
        /// <summary>
        /// Process the merge between both modes matrices
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time)
        {
            Matrix4x4 ret = new Matrix4x4();
            for (int i = 0; i < 16; i++)
                ret[i] = Mathf.Lerp(from[i], to[i], time);
            return ret;
        }
        #endregion

        #region FOLLOW
        /// <summary>
        /// Stop follow/focus/moveto animations
        /// </summary>
        public void StopFollow()
        {
            camOrtho.StopFollow();
            camPersp.StopFollow();
        }

        /// <summary>
        /// Follow a game object
        /// </summary>
        /// <param name="go">The game object to follow</param>
        /// <param name="doFocus">Also focus on it (distance animation)</param>
        /// <param name="allowYOffsetFromGround">Allow offseting the camera from the ground to match the object's pivot y position and height</param>
        public void FollowGameObject(GameObject go, bool focusOnFollow, bool allowYOffsetFromGround = false)
        {
            CurrentCameraMode.FollowGameObject(go, focusOnFollow, allowYOffsetFromGround);

        }
        #endregion

        #region FOCUS
        /// <summary>
        /// Focus the camera on a GameObject (distance animation)
        /// </summary>
        /// <param name="go">The gameObject to get closer to</param>
        /// <param name="allowYOffsetFromGround">Allow offseting the camera from the ground to match the object's pivot y position and height</param>
        public void FocusCameraOnGameObject(GameObject go, bool allowYOffsetFromGround = false)
        {
            StopFollow();
            CurrentCameraMode.FocusCameraOnGameObject(go, allowYOffsetFromGround);
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
            camPersp.SetGroundHeightAnimated(newHeight, animate, duration);
        }

        /// <summary>
        /// Return the matrix of the camera transform, in order to blend it when switching modes
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetMatrix()
        {
            return CurrentCameraMode.GetMatrix();
        }

        /// <summary>
        /// This let you change the ground height at any moment in order to
        /// change at which y position the fingers will be intercepted
        /// </summary>
        /// <param name="y"></param>
        public void SetGroundHeight(float y)
        {
            camPersp.SetGroundHeight(y);
        }

        /// <summary>
        /// Rotate the camera manually
        /// </summary>
        /// <param name="delta">the increment values (pitch, yaw)</param>
        public void RotateFromVector(Vector2 delta)
        {
            CurrentCameraMode.RotateFromVector(delta);
        }

        /// <summary>
        /// Set the initial values for the reset function
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="rotation"></param>
        /// <param name="distanceOrSize"></param>
        public void SetResetValues(Vector3 offset, Quaternion rotation, float distanceOrSize)
        {
            CurrentCameraMode.SetResetValues(offset, rotation, distanceOrSize);
        }

        /// <summary>
        /// Changes the Camera distance from the ground to a new position, in 1 frame
        /// </summary>
        /// <param name="targetDistance"></param>
        public void MoveCameraToInstant(float targetDistance)
        {
            CurrentCameraMode.MoveCameraToInstant(targetDistance);
        }

        /// <summary>
        /// Moves the Camera rotation, in 1 frame
        /// </summary>
        /// <param name="targetRotation"></param>
        public void MoveCameraToInstant(Quaternion targetRotation)
        {
            CurrentCameraMode.MoveCameraToInstant(targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground to a new position, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        public void MoveCameraToInstant(Vector3 targetPosition)
        {
            CurrentCameraMode.MoveCameraToInstant(targetPosition);
        }

        /// <summary>
        /// Moves the Camera offset position on ground and distance from it, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        public void MoveCameraToInstant(Vector3 targetPosition, float targetDistance)
        {
            CurrentCameraMode.MoveCameraToInstant(targetPosition, targetDistance);
        }

        /// <summary>
        /// Moves the Camera offset position on ground and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraToInstant(Vector3 targetPosition, Quaternion targetRotation)
        {
            CurrentCameraMode.MoveCameraToInstant(targetPosition, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraToInstant(Vector3 targetPosition, Vector2 targetRotation)
        {
            CurrentCameraMode.MoveCameraToInstant(targetPosition, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraToInstant(Vector3 targetPosition, float targetDistance, Vector2 targetRotation)
        {
            CurrentCameraMode.MoveCameraToInstant(targetPosition, targetDistance, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraToInstant(Vector3 targetPosition, float targetDistance, Quaternion targetRotation)
        {
            CurrentCameraMode.MoveCameraToInstant(targetPosition, targetDistance, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        public void MoveCameraTo(Vector3 targetPosition)
        {
            CurrentCameraMode.MoveCameraTo(targetPosition);
        }

        /// <summary>
        /// Changes the Camera rotation, animated
        /// </summary>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Quaternion targetRotation)
        {
            CurrentCameraMode.MoveCameraTo(targetRotation);
        }

        /// <summary>
        /// Moves the Camera distance from the ground, animated
        /// </summary>
        /// <param name="targetDistanceOrSize"></param>
        public void MoveCameraTo(float targetDistanceOrSize)
        {
            CurrentCameraMode.MoveCameraTo(targetDistanceOrSize);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        public void MoveCameraTo(Vector3 targetPosition, float targetDistance)
        {
            CurrentCameraMode.MoveCameraTo(targetPosition, targetDistance);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Vector3 targetPosition, Quaternion targetRotation)
        {
            CurrentCameraMode.MoveCameraTo(targetPosition, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Vector3 targetPosition, Vector2 targetRotation)
        {
            CurrentCameraMode.MoveCameraTo(targetPosition, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Vector3 targetPosition, float targetDistance, Vector2 targetRotation)
        {
            CurrentCameraMode.MoveCameraTo(targetPosition, targetDistance, targetRotation);
        }

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        public void MoveCameraTo(Vector3 targetPosition, float targetDistance, Quaternion targetRotation)
        {
            CurrentCameraMode.MoveCameraTo(targetPosition, targetDistance, targetRotation);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, bool instant = false)
        {
            CurrentCameraMode.FocusCamera(targetPosition, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, float targetDistance, bool instant = false)
        {
            CurrentCameraMode.FocusCamera(targetPosition, targetDistance, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, Quaternion targetRotation, bool instant = false)
        {
            CurrentCameraMode.FocusCamera(targetPosition, targetRotation, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, Vector2 targetRotation, bool instant = false)
        {
            CurrentCameraMode.FocusCamera(targetPosition, targetRotation, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, float targetDistance, Vector2 targetRotation, bool instant = false)
        {
            CurrentCameraMode.FocusCamera(targetPosition, targetDistance, targetRotation, instant);
        }

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        public void FocusCamera(Vector3 targetPosition, float targetDistance, Quaternion targetRotation, bool instant = false)
        {
            CurrentCameraMode.FocusCamera(targetPosition, targetDistance, targetRotation, instant);
        }

        #endregion
    }
}
