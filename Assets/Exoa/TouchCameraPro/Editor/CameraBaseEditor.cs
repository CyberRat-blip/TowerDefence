using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Exoa.Cameras
{
    public abstract class CameraBaseEditor : Editor
    {
        protected List<string> dontIncludeMe = new List<string>() { "m_Script" };
        protected bool debugFoldout;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HideProperties();

            DrawPropertiesExcluding(serializedObject, dontIncludeMe.ToArray());

            serializedObject.ApplyModifiedProperties();
#if DEBUG_TCP
            DrawDebug();
#endif
        }

        virtual protected void HideProperties()
        {

            CameraBase c = target as CameraBase;
            CameraModeSwitcher ms = c.gameObject.GetComponent<CameraModeSwitcher>();

            dontIncludeMe = new List<string>() { "m_Script", };

            if (ms == null)
            {
                dontIncludeMe.Add("defaultMode");
            }
            if (!c.allowYawRotation)
            {
                dontIncludeMe.Add("YawSensitivity");
                dontIncludeMe.Add("YawClamp");
                dontIncludeMe.Add("YawMinMax");
            }
            if (!c.allowPitchRotation)
            {
                dontIncludeMe.Add("PitchSensitivity");
                dontIncludeMe.Add("PitchClamp");
                dontIncludeMe.Add("PitchMinMax");
            }
            if (!c.allowYawRotation)
            {
                dontIncludeMe.Add("YawSensitivity");
                dontIncludeMe.Add("YawClamp");
                dontIncludeMe.Add("YawMinMax");
            }
            if (!c.PitchClamp)
            {
                dontIncludeMe.Add("PitchMinMax");
            }
            if (!c.YawClamp)
            {
                dontIncludeMe.Add("YawMinMax");
            }
            if (!c.enableTranslationInertia)
            {
                dontIncludeMe.Add("positionInertiaMove");
                dontIncludeMe.Add("positionAcceleration");
                dontIncludeMe.Add("positionInertiaForce");
            }
            if (!c.enableRotationInertia)
            {
                dontIncludeMe.Add("rotationInertiaMove");
                dontIncludeMe.Add("rotationAcceleration");
                dontIncludeMe.Add("rotationInertiaForce");
            }
        }
        virtual protected void DrawDebug()
        {
            debugFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(debugFoldout, "Debug Info");
            if (debugFoldout)
            {
                DrawDebugLines();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawDebugLines()
        {
            CameraBase c = target as CameraBase;
            EditorGUILayout.LabelField("Ground Height:" + c.groundHeight);

            if (c is CameraOrthoBase)
            {
                EditorGUILayout.LabelField("Size:" + (c as CameraOrthoBase).FinalSize);
            }
            else
            {
                EditorGUILayout.LabelField("Distance:" + c.FinalDistance);
            }

            EditorGUILayout.LabelField("Offset:" + c.FinalOffset);
            EditorGUILayout.LabelField("Rotation:" + c.FinalRotation);
            EditorGUILayout.LabelField("Position:" + c.FinalPosition);
            EditorGUILayout.LabelField("PitchAndYaw:" + c.PitchAndYaw);
        }
    }
}
