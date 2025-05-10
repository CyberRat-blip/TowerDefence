using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras
{
    public interface ITouchOrthoCamera : ITouchCamera
    {
        /// <summary>
        /// Get the camera orthographic's size
        /// </summary>
        float FinalSize { get; }

        /// <summary>
        /// Converts a distance to a camera orthographic size
        /// </summary>
        /// <param name="d"></param>
        void SetSizeByDistance(float d);

        /// <summary>
        /// Converts the orthographic size to a distance to pass to a perspective camera
        /// </summary>
        /// <returns></returns>
        float GetDistanceFromSize();

        /// <summary>
        /// Converts a camera offset on ground to a camera position
        /// </summary>
        void SetPositionByOffset();

    }
}
