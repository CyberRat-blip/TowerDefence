using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Exoa.Cameras
{
    [CustomEditor(typeof(CameraSideScrollOrtho))]
    public class CameraSideScrollOrthoEditor : CameraBaseEditor
    {
        protected override void HideProperties()
        {
            base.HideProperties();


            dontIncludeMe.Add("allowPitchRotation");
            dontIncludeMe.Add("pitchSensitivity");
            dontIncludeMe.Add("pitchClamp");
            dontIncludeMe.Add("pitchMinMax");
            dontIncludeMe.Add("yawSensitivity");
            dontIncludeMe.Add("groundHeight");
            dontIncludeMe.Add("groundHeightAnim");
        }

    }
}
