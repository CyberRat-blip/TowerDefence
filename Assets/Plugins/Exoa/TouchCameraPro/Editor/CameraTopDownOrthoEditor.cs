using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Exoa.Cameras
{
    [CustomEditor(typeof(CameraTopDownOrtho))]
    public class CameraTopDownOrthoEditor : CameraBaseEditor
    {
        protected override void HideProperties()
        {
            base.HideProperties();


            dontIncludeMe.Add("allowPitchRotation");
            dontIncludeMe.Add("PitchSensitivity");
            dontIncludeMe.Add("PitchClamp");
            dontIncludeMe.Add("PitchMinMax");
            dontIncludeMe.Add("YawSensitivity");
        }

    }
}
