using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras
{
    public interface ITouchCamera
    {
        /// <summary>
        /// This blocks the user input moves, in order to animate the camera by script
        /// </summary>
        bool DisableMoves { get; set; }

        /// <summary>
        /// This blocks the user translation inputs
        /// </summary>
        bool DisableTranslations { get; set; }

        /// <summary>
        /// This blocks the user translation inputs
        /// </summary>
        bool DisableRotations { get; set; }

        /// <summary>
        /// The final quaternion rotation of the camera transform
        /// </summary>
        Quaternion FinalRotation { get; }

        /// <summary>
        /// The final position of the camera transform
        /// </summary>
        Vector3 FinalPosition { get; }

        /// <summary>
        /// The final offset of the camera's center point on ground
        /// This is not the camera position. The center of the camera is projected 
        /// on the ground, and this is it's position.
        /// </summary>
        Vector3 FinalOffset { get; }

        /// <summary>
        /// Returns true if the camera is currently rotating
        /// </summary>
        /// <returns></returns>
        bool IsRotating();

        /// <summary>
        /// This is the final distance between the camera's center point on the ground,
        /// and the camera transform's position
        /// </summary>
        float FinalDistance { get; }

        /// <summary>
        /// This returns the current Pitch and Yaw rotations of the camera
        /// </summary>
        Vector2 PitchAndYaw { get; }

        /// <summary>
        /// Return the matrix of the camera transform, in order to blend it when switching modes
        /// </summary>
        /// <returns></returns>
        Matrix4x4 GetMatrix();

        /// <summary>
        /// This let you change the ground height at any moment in order to
        /// change at which y position the fingers will be intercepted.
        /// This version lets you animate it
        /// </summary>
        /// <param name="newHeight"></param>
        /// <param name="animate"></param>
        /// <param name="duration"></param>
        void SetGroundHeightAnimated(float newHeight, bool animate, float duration);

        /// <summary>
        /// This let you change the ground height at any moment in order to
        /// change at which y position the fingers will be intercepted
        /// </summary>
        /// <param name="y"></param>
        void SetGroundHeight(float y);

        /// <summary>
        /// Rotate the camera manually
        /// </summary>
        /// <param name="delta">the increment values (pitch, yaw)</param>
        void RotateFromVector(Vector2 delta);

        /// <summary>
        /// Reset the camera to initial values
        /// </summary>
        void ResetCamera();

        /// <summary>
        /// Set the initial values for the reset function
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="rotation"></param>
        /// <param name="distanceOrSize"></param>
        void SetResetValues(Vector3 offset, Quaternion rotation, float distanceOrSize);

        /// <summary>
        /// Moves the Camera offset position on ground to a new position, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        void MoveCameraToInstant(Vector3 targetPosition);

        /// <summary>
        /// Changes the Camera distance from the position on ground, in 1 frame
        /// </summary>
        /// <param name="targetDistanceOrSize"></param>
        void MoveCameraToInstant(float targetDistanceOrSize);

        /// <summary>
        /// Changes the Camera rotation, in 1 frame
        /// </summary>
        /// <param name="targetRotation"></param>
        void MoveCameraToInstant(Quaternion targetRotation);

        /// <summary>
        /// Moves the Camera offset position on ground and distance from it, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        void MoveCameraToInstant(Vector3 targetPosition, float targetDistance);

        /// <summary>
        /// Moves the Camera offset position on ground and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        void MoveCameraToInstant(Vector3 targetPosition, Quaternion targetRotation);

        /// <summary>
        /// Moves the Camera offset position on ground and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        void MoveCameraToInstant(Vector3 targetPosition, Vector2 targetRotation);

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        void MoveCameraToInstant(Vector3 targetPosition, float targetDistance, Vector2 targetRotation);

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, in 1 frame
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        void MoveCameraToInstant(Vector3 targetPosition, float targetDistance, Quaternion targetRotation);

        /// <summary>
        /// Moves the Camera offset position on ground, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        void MoveCameraTo(Vector3 targetPosition);

        /// <summary>
        /// Changes the Camera rotation, animated
        /// </summary>
        /// <param name="targetRotation"></param>
        void MoveCameraTo(Quaternion targetRotation);

        /// <summary>
        /// Moves the Camera distance from the ground, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        void MoveCameraTo(float targetDistanceOrSize);

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        void MoveCameraTo(Vector3 targetPosition, float targetDistance);

        /// <summary>
        /// Moves the Camera offset position on ground, and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        void MoveCameraTo(Vector3 targetPosition, Quaternion targetRotation);

        /// <summary>
        /// Moves the Camera offset position on ground, and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        void MoveCameraTo(Vector3 targetPosition, Vector2 targetRotation);

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        void MoveCameraTo(Vector3 targetPosition, float targetDistance, Vector2 targetRotation);

        /// <summary>
        /// Moves the Camera offset position on ground, distance from it and rotation, animated
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        void MoveCameraTo(Vector3 targetPosition, float targetDistance, Quaternion targetRotation);


        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="instant"></param>
        void FocusCamera(Vector3 targetPosition, bool instant = false);

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="instant"></param>
        void FocusCamera(Vector3 targetPosition, float targetDistance, bool instant = false);

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        void FocusCamera(Vector3 targetPosition, Quaternion targetRotation, bool instant = false);

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        void FocusCamera(Vector3 targetPosition, Vector2 targetRotation, bool instant = false);

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        void FocusCamera(Vector3 targetPosition, float targetDistance, Vector2 targetRotation, bool instant = false);

        /// <summary>
        /// Alias of MoveCameraTo
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetDistance"></param>
        /// <param name="targetRotation"></param>
        /// <param name="instant"></param>
        void FocusCamera(Vector3 targetPosition, float targetDistance, Quaternion targetRotation, bool instant = false);



        /// <summary>
        /// Focus the camera on a GameObject (distance animation)
        /// </summary>
        /// <param name="go">The gameObject to get closer to</param>
        /// <param name="allowYOffsetFromGround">Allow offseting the camera from the ground to match the object's pivot y position and height</param>
        void FocusCameraOnGameObject(GameObject go, bool allowYOffsetFromGround = false);

        /// <summary>
        /// Follow a game object
        /// </summary>
        /// <param name="go">The game object to follow</param>
        /// <param name="doFocus">Also focus on it (distance animation)</param>
        /// <param name="allowYOffsetFromGround">Allow offseting the camera from the ground to match the object's pivot y position and height</param>
        void FollowGameObject(GameObject go, bool doFocus, bool allowYOffsetFromGround = false);

        /// <summary>
        /// Stop follow/focus/moveto animations
        /// </summary>
        void StopFollow();

    }
}
