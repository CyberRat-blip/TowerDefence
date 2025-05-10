using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras
{
    public interface ITouchPerspCamera : ITouchCamera
    {

        /// <summary>
        /// Get the Field of view of the camera
        /// </summary>
        float Fov { get; }

        /// <summary>
        /// Converts a distance to a camera position
        /// </summary>
        /// <param name="distance"></param>
        void SetPositionByDistance(float distance);

    }
}
