using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Exoa.Cameras
{
    [CustomEditor(typeof(CameraBoundaries))]
    public class CameraBounderiesEditor : CameraBaseEditor
    {
        protected override void HideProperties()
        {
            CameraBoundaries cb = target as CameraBoundaries;
            dontIncludeMe = new List<string>() { "m_Script", };
            if (cb.mode != CameraBoundaries.Mode.CameraEdges)
            {
                dontIncludeMe.Add("pitchThreshold");
            }
            if (cb.movementType != CameraBoundaries.MovementType.Elastic)
            {
                dontIncludeMe.Add("elasticity");
                dontIncludeMe.Add("elasticSpace");
                dontIncludeMe.Add("drawElasticGizmos");
            }
            if (cb.bounderiesCollider != null && cb.bounderiesCollider is SphereCollider && cb.sphereCollider == null)
            {
                cb.sphereCollider = cb.bounderiesCollider as SphereCollider;
                dontIncludeMe.Add("bounderiesCollider");
            }
            else if (cb.sphereCollider != null)
            {
                dontIncludeMe.Add("bounderiesCollider");
            }
            if (cb.bounderiesCollider != null && cb.bounderiesCollider is BoxCollider && cb.boxCollider == null)
            {
                cb.boxCollider = cb.bounderiesCollider as BoxCollider;
                dontIncludeMe.Add("bounderiesCollider");
            }
            else if (cb.boxCollider != null)
            {
                dontIncludeMe.Add("bounderiesCollider");
            }
            if (cb.type == CameraBoundaries.Type.Circle)
            {
                dontIncludeMe.Add("boxCollider");
            }
            else if (cb.type == CameraBoundaries.Type.Rectangle)
            {
                dontIncludeMe.Add("sphereCollider");
            }
        }
        protected override void DrawDebug()
        {

        }
    }
}
