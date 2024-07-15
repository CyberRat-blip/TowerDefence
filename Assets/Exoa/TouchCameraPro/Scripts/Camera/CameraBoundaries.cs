using Exoa.Touch;
using UnityEngine;

namespace Exoa.Cameras
{
    public class CameraBoundaries : MonoBehaviour
    {
        public enum Type { Rectangle, Circle };
        public enum Mode { CameraCenter, CameraEdges };
        public enum MovementType
        {
            Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
            Clamped, // Restricted movement where it's not possible to go past the edges
            Unrestricted, // Unrestricted movement -- can scroll forever

        }

        public Type type;

        public MovementType movementType = MovementType.Elastic;

        public float elasticity = 0.05f; // Only used for MovementType.Elastic
        public float elasticSpace = 5f;


        [Header("Collider")]
        public Collider bounderiesCollider;
        public SphereCollider sphereCollider;
        public BoxCollider boxCollider;

        //private MeshRenderer debugMr;
        private Bounds bounds;
        private Bounds boundsElastic;

        public Mode mode;

        public float pitchThreshold = 60f;
        public float insideThreshold = .5f;
        private Vector3 lastAllowedPosition;
        private Vector3 lastPointInsideBounds;

        [Header("Debug")]
        public bool drawGizmos;
        public bool drawElasticGizmos;

        /// <summary>
        /// Clamp any given points inside the defined boundaries
        /// </summary>
        /// <param name="p"></param>
        /// <param name="isInBoundaries"></param>
        /// <param name="groundHeight"></param>
        /// <returns></returns>
        public Vector3 ClampInBoundsXZ(Vector3 p, out bool isInBoundaries, float groundHeight, bool anyFingerDown, bool forceStrictClamp = false)
        {
            isInBoundaries = true;

            if (type == Type.Rectangle && boxCollider == null)
                return p;

            if (type == Type.Circle && sphereCollider == null)
                return p;

            if (movementType == MovementType.Unrestricted)
                return p;

            if (type == Type.Rectangle)
            {
                if (boxCollider.enabled)
                {
                    bounds = boxCollider.bounds;

                    boundsElastic = new Bounds(bounds.center, bounds.size);
                    boundsElastic.Expand(elasticSpace);

                    boxCollider.enabled = false;
                }
                bounds.center = bounds.center.SetY(groundHeight);
                bounds.size = bounds.size.SetY(0);

                if (bounds.Contains(p.SetY(groundHeight)))
                    return p;

                isInBoundaries = false;

                Vector3 targetPointInBounds = bounds.ClosestPoint(p).SetY(p.y);

                if (movementType == MovementType.Clamped || forceStrictClamp)
                {
                    return targetPointInBounds;
                }
                else if (movementType == MovementType.Elastic)
                {

                    if (anyFingerDown)
                    {
                        lastPointInsideBounds = (targetPointInBounds - p).normalized * insideThreshold + targetPointInBounds;

                        Vector3 targetPointInElasticBounds = boundsElastic.ClosestPoint(p).SetY(p.y);

                        if (boundsElastic.Contains(p.SetY(groundHeight)))
                            return p;
                        else return targetPointInElasticBounds;
                    }
                    else
                    {
                        Vector3 currentVelocity = new Vector3(1, 1, 1);
                        return Vector3.SmoothDamp(p, lastPointInsideBounds, ref currentVelocity,
                                                    elasticity, Mathf.Infinity, Time.unscaledDeltaTime);
                    }
                }


            }
            else if (type == Type.Circle)
            {
                Vector3 globalCenter = sphereCollider.transform.TransformPoint(sphereCollider.center).SetY(groundHeight);
                float globalMagnitude = sphereCollider.transform.TransformVector(sphereCollider.radius * Vector3.up).magnitude;
                Vector3 centerToPoint = (p - globalCenter).SetY(0);
                if (centerToPoint.magnitude <= globalMagnitude)
                    return p;

                isInBoundaries = false;

                Vector3 targetPointInBounds = (globalCenter + centerToPoint.normalized * globalMagnitude).SetY(p.y);
                Vector3 targetPointInElasticBounds = (globalCenter + centerToPoint.normalized * (globalMagnitude + elasticSpace)).SetY(p.y);

                if (movementType == MovementType.Clamped || forceStrictClamp)
                    return targetPointInBounds;
                else if (movementType == MovementType.Elastic)
                {
                    if (anyFingerDown)
                    {
                        //Debug.Log("A");
                        //lastPointInsideBounds = (targetPointInBounds - p).normalized * insideThreshold + targetPointInBounds;

                        if (centerToPoint.magnitude <= (globalMagnitude + elasticSpace))
                            return p;
                        else return targetPointInElasticBounds;
                    }
                    else
                    {
                        Vector3 currentVelocity = new Vector3(1, 1, 1);
                        return Vector3.SmoothDamp(p, lastPointInsideBounds, ref currentVelocity,
                                                    elasticity, Mathf.Infinity, Time.unscaledDeltaTime);
                    }
                }


            }
            return p;
        }

        /// <summary>
        /// Clamp any given points inside the defined boundaries
        /// </summary>
        /// <param name="p"></param>
        /// <param name="isInBoundaries"></param>
        /// <param name="groundHeight"></param>
        /// <returns></returns>
        public Vector3 ClampInBoundsXY(Vector3 p, out bool isInBoundaries, float groundHeight, bool anyFingerDown, bool forceStrictClamp = false)
        {
            isInBoundaries = true;

            if (type == Type.Rectangle && boxCollider == null)
                return p;

            if (type == Type.Circle && sphereCollider == null)
                return p;


            if (type == Type.Rectangle)
            {
                if (boxCollider.enabled)
                {
                    bounds = boxCollider.bounds;

                    boundsElastic = new Bounds(bounds.center, bounds.size);
                    boundsElastic.Expand(elasticSpace);

                    boxCollider.enabled = false;
                }
                bounds.center = bounds.center.SetZ(groundHeight);
                bounds.size = bounds.size.SetZ(0);


                if (bounds.Contains(p.SetZ(groundHeight)))
                    return p;
                isInBoundaries = false;



                Vector3 targetPointInBounds = bounds.ClosestPoint(p).SetZ(p.z);

                if (movementType == MovementType.Clamped || forceStrictClamp)
                    return targetPointInBounds;
                else if (movementType == MovementType.Elastic)
                {
                    if (anyFingerDown)
                    {
                        Vector3 targetPointInElasticBounds = boundsElastic.ClosestPoint(p).SetZ(p.z);
                        lastPointInsideBounds = (targetPointInBounds - p).normalized * insideThreshold + targetPointInBounds;

                        if (boundsElastic.Contains(p.SetY(groundHeight)))
                            return p;
                        else return targetPointInElasticBounds;
                    }
                    else
                    {
                        Vector3 currentVelocity = new Vector3(1, 1, 1);
                        return Vector3.SmoothDamp(p, lastPointInsideBounds, ref currentVelocity,
                                                    elasticity, Mathf.Infinity, Time.unscaledDeltaTime);
                    }
                }
            }
            else if (type == Type.Circle)
            {
                Vector3 globalCenter = sphereCollider.transform.TransformPoint(sphereCollider.center).SetZ(groundHeight);
                float globalMagnitude = sphereCollider.transform.TransformVector(sphereCollider.radius * Vector3.up).magnitude;
                Vector3 centerToPoint = (p - globalCenter).SetZ(0);
                if (centerToPoint.magnitude <= globalMagnitude)
                    return p;
                isInBoundaries = false;
                Vector3 targetPointInBounds = (globalCenter + centerToPoint.normalized * globalMagnitude).SetZ(p.z);
                Vector3 targetPointInElasticBounds = (globalCenter + centerToPoint.normalized * (globalMagnitude + elasticSpace)).SetZ(p.z);

                if (movementType == MovementType.Clamped || forceStrictClamp)
                    return targetPointInBounds;
                else if (movementType == MovementType.Elastic)
                {
                    if (anyFingerDown)
                    {
                        lastPointInsideBounds = (targetPointInBounds - p).normalized * insideThreshold + targetPointInBounds;

                        if (centerToPoint.magnitude <= (globalMagnitude + elasticSpace))
                            return p;
                        else return targetPointInElasticBounds;
                    }
                    else
                    {
                        Vector3 currentVelocity = new Vector3(1, 1, 1);
                        return Vector3.SmoothDamp(p, lastPointInsideBounds, ref currentVelocity,
                                                    elasticity, Mathf.Infinity, Time.unscaledDeltaTime);
                    }
                }
            }
            return p;
        }


        /// <summary>
        /// Method used to clamp the camera inside the boundaries collider
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="isInBoundaries"></param>
        /// <param name="bottomEdges"></param>
        /// <param name="topEdges"></param>
        /// <returns></returns>
        public Vector3 ClampCameraCorners(Vector3 currentPosition, out bool isInBoundaries,
            float currentPitch, ScreenDepth HeightScreenDepth,
            Camera cam, float groundHeight, bool anyFingerDown)
        {
            isInBoundaries = true;
            bool bottomEdges = true;
            bool topEdges = currentPitch > pitchThreshold;

            if (!IsUsingCameraEdgeBoundaries())
            {
                return currentPosition;
            }

            Vector3 diffBL = Vector3.zero,
                 diffBR = Vector3.zero,
                 diffTR = Vector3.zero,
                 diffTL = Vector3.zero;

            bool isBLInBounds = true,
                isBRInBounds = true,
                isTLInBounds = true,
                isTRInBounds = true;

            if (bottomEdges)
            {
                Vector3 worldPointBottomLeft = HeightScreenDepth.Convert(cam.ViewportToScreenPoint(new Vector3(0, 0, 0)));
                Vector3 worldPointBottomLeftClamped = ClampInBoundsXZ(worldPointBottomLeft, out isBLInBounds, groundHeight, anyFingerDown, true);
                diffBL = worldPointBottomLeftClamped - worldPointBottomLeft;

                Vector3 worldPointBottomRight = HeightScreenDepth.Convert(cam.ViewportToScreenPoint(new Vector3(1, 0, 0)));
                Vector3 worldPointBottomRightClamped = ClampInBoundsXZ(worldPointBottomRight, out isBRInBounds, groundHeight, anyFingerDown, true);
                diffBR = worldPointBottomRightClamped - worldPointBottomRight;
            }
            if (topEdges)
            {
                Vector3 worldPointTopRight = HeightScreenDepth.Convert(cam.ViewportToScreenPoint(new Vector3(1, 1, 0)));
                Vector3 worldPointTopRightClamped = ClampInBoundsXZ(worldPointTopRight, out isTRInBounds, groundHeight, anyFingerDown, true);
                diffTR = worldPointTopRightClamped - worldPointTopRight;

                Vector3 worldPointTopLeft = HeightScreenDepth.Convert(cam.ViewportToScreenPoint(new Vector3(0, 1, 0)));
                Vector3 worldPointTopLeftClamped = ClampInBoundsXZ(worldPointTopLeft, out isTLInBounds, groundHeight, anyFingerDown, true);
                diffTL = worldPointTopLeftClamped - worldPointTopLeft;
            }
            float m = Mathf.Abs((diffTR + diffBR + diffBL + diffTL).magnitude);
            //bool needsAdjustment = m > .3f;
            isInBoundaries = isBLInBounds && isBRInBounds && isTRInBounds && isTLInBounds;
            //isInBoundaries = !needsAdjustment;

            if (isInBoundaries)
            {
                lastAllowedPosition = currentPosition;
                return currentPosition;
            }
            //Debug.Log("m:" + m + " needsAdjustment:" + needsAdjustment);
            //Debug.Log("diffBL:" + diffBL
            //    + " diffBR:" + diffBR
            //    + " diffTR:" + diffTR
            //    + " diffTL:" + diffTL);

            //Debug.Log("isBLInBounds:" + isBLInBounds +
            //    " isBRInBounds:" + isBRInBounds +
            //    " isTRInBounds:" + isTRInBounds +
            //    " isTLInBounds:" + isTLInBounds);

            //Debug.Log("isInBoundaries2:" + isInBoundaries);

            Vector3 currentVelocity = new Vector3(1, 1, 1);
            Vector3 targetPointInBounds = currentPosition + diffTR + diffBR + diffBL + diffTL;
            Vector3 currentPositionToBoundsPosition = targetPointInBounds - currentPosition;

            if (movementType == MovementType.Unrestricted)
                return currentPosition;
            if (movementType == MovementType.Clamped)
                return targetPointInBounds;
            else if (movementType == MovementType.Elastic)
            {
                if (anyFingerDown)
                {
                    lastPointInsideBounds = (targetPointInBounds - currentPosition).normalized * insideThreshold + targetPointInBounds;

                    if (currentPositionToBoundsPosition.magnitude <= elasticSpace)
                    {
                        lastAllowedPosition = currentPosition;
                        return currentPosition;
                    }
                    else return lastAllowedPosition;
                    //else return currentPositionToBoundsPosition.normalized * elasticSpace + targetPointInBounds;
                }
                else if (!isInBoundaries)
                {
                    //lastPointInsideBounds.y = currentPosition.y;
                    return Vector3.SmoothDamp(currentPosition, lastPointInsideBounds, ref currentVelocity,
                      elasticity, Mathf.Infinity, Time.unscaledDeltaTime);
                }
            }


            return currentPosition;
        }


        public bool IsUsingCameraEdgeBoundaries()
        {
            return mode == CameraBoundaries.Mode.CameraEdges;
        }

        void OnDrawGizmos()
        {
            //print("bounderiesCollider:" + bounderiesCollider);
            if (!drawGizmos && !drawElasticGizmos)
                return;

            if (type == Type.Rectangle && boxCollider == null)
                return;

            if (type == Type.Circle && sphereCollider == null)
                return;


            Color c = Color.yellow;
            c.a = .5f;

            Color c2 = Color.green;
            c2.a = .3f;



            if (type == Type.Rectangle && bounderiesCollider is BoxCollider)
            {
                Bounds b = boxCollider.bounds;
                b.center = bounds.center.SetY(0);
                b.size = bounds.size.SetY(0);
                Gizmos.matrix = boxCollider.transform.localToWorldMatrix;
                Gizmos.color = c;
                if (drawGizmos) Gizmos.DrawCube(b.center, b.size);
                Gizmos.color = c2;
                if (drawElasticGizmos) Gizmos.DrawCube(boundsElastic.center, boundsElastic.size);
            }
            else if (type == Type.Circle && bounderiesCollider is SphereCollider)
            {
                Gizmos.matrix = sphereCollider.transform.localToWorldMatrix;
                Gizmos.color = c;
                if (drawGizmos) Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
                Gizmos.color = c2;
                if (drawElasticGizmos) Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius + elasticSpace);
            }
            /*if (debugMr != null)
            {
                debugMr.transform.position = ClampPointsXZ(debugMr.transform.position, out bool isInBoundaries);
                debugMr.sharedMaterial.color = isInBoundaries ? Color.green : Color.red;
            }*/
        }
    }
}
