using System;
using System.Collections;
using System.Collections.Generic;
using Exoa.Cameras;
using Exoa.Events;
using UnityEngine;
using UnityEngine.UI;
using static CameraActionButton;

[AddComponentMenu("Exoa/Demo/CustomControlUIDemo")]
public class CustomControlUIDemo : MonoBehaviour
{
    public CameraActionButton zoomInBtn;
    public CameraActionButton zoomOutBtn;
    public CameraActionButton rotateLeftBtn;
    public CameraActionButton rotateRightBtn;
    public CameraActionButton rotateUpBtn;
    public CameraActionButton rotateDownBtn;
    public CameraActionButton viewMode1Btn;
    public CameraActionButton viewMode2Btn;
    public CameraActionButton viewMode3Btn;
    public CameraActionButton followBtn;

    private Camera cam;
    private CameraPerspective cam1;
    private CameraTopDownOrtho cam2;
    private CameraSideScrollOrtho2 cam3;
    //private CameraModeSwitcher cameraModeSwitcher;
    public GameObject targetGo;

    private Quaternion manualRotation;
    public float moveSpeed = 10f;
    public float rotationSpeed = 1f;
    public float zoomSpeed = 1f;

    IEnumerator Start()
    {
        cam = GetComponent<Camera>();
        cam1 = GetComponent<CameraPerspective>();
        cam2 = GetComponent<CameraTopDownOrtho>();
        cam3 = GetComponent<CameraSideScrollOrtho2>();

        zoomInBtn.onPressed += () =>
        {
            Zoom(-zoomSpeed);
        };
        zoomOutBtn.onPressed += () =>
        {
            Zoom(zoomSpeed);
        };
        rotateLeftBtn.onPressed += () =>
        {
            Rotate(rotationSpeed, 0);
        };
        rotateRightBtn.onPressed += () =>
        {
            Rotate(-rotationSpeed, 0);
        };
        rotateUpBtn.onPressed += () =>
        {
            Rotate(0, rotationSpeed);
        };
        rotateDownBtn.onPressed += () =>
        {
            Rotate(0, -rotationSpeed);
        };
        followBtn.onClicked += () =>
        {
            if (cam1.IsFocusingOrFollowing)
                Follow(null);
            else Follow(targetGo);
        };
        viewMode1Btn.onClicked += () =>
        {
            if (cam2.enabled)
            {
                cam1.FinalOffset = cam2.FinalOffset;
                cam1.SetPositionByDistance(cam2.GetDistanceFromSize());
            }
            else if (cam3.enabled)
            {
                cam1.FinalOffset = cam3.FinalOffset;
                cam1.SetPositionByDistance(cam3.GetDistanceFromSize());

            }

            cam1.enabled = true;
            cam.orthographic = false;
            cam2.enabled = false;
            cam3.enabled = false;
        };
        viewMode2Btn.onClicked += () =>
        {
            if (cam1.enabled)
            {
                cam2.FinalOffset = cam1.FinalOffset;
                cam2.SetSizeByDistance(cam1.FinalDistance);
                cam2.SetPositionByOffset();
            }
            else if (cam3.enabled)
            {
                cam2.FinalOffset = cam3.FinalOffset.SetY(0);
                cam2.FinalSize = cam3.FinalSize;
                cam2.SetPositionByOffset();

            }

            cam1.enabled = false;

            cam3.enabled = false;
            cam2.enabled = true;
            cam.orthographic = true;
        };
        viewMode3Btn.onClicked += () =>
        {
            if (cam1.enabled)
            {

                cam3.FinalOffset = cam1.FinalOffset;
                cam3.SetSizeByDistance(cam1.FinalDistance);
                cam3.SetPositionByOffset();
            }
            else if (cam2.enabled)
            {
                cam3.FinalOffset = cam2.FinalOffset;
                cam3.FinalSize = cam2.FinalSize;
                cam3.SetPositionByOffset();
            }

            cam1.enabled = false;
            cam2.enabled = false;
            cam3.enabled = true;
            cam.orthographic = true;
        };

        yield return new WaitForEndOfFrame();

        cam2.enabled = cam3.enabled = false;

    }
    private void Update()
    {
        if (isFollowing)
        {
            cam1.SetGroundHeight(targetGo.transform.position.y);
            cam1.MoveCameraToInstant(targetGo.transform.position);
            cam2.SetGroundHeight(targetGo.transform.position.y);
            cam2.MoveCameraToInstant(targetGo.transform.position);
            cam3.SetGroundHeight(targetGo.transform.position.y);
            cam3.MoveCameraToInstant(targetGo.transform.position);
        }
    }
    private void Zoom(float v)
    {
        cam1.MoveCameraToInstant(cam1.FinalDistance + v);
        cam2.MoveCameraToInstant(cam2.FinalSize + v);
        cam3.MoveCameraToInstant(cam3.FinalSize + v);
    }

    private bool isFollowing;
    private void Follow(GameObject targetGo)
    {
        isFollowing = !isFollowing;
    }



    private void Rotate(float v, float h)
    {
        manualRotation = cam1.FinalRotation;
        manualRotation = Quaternion.Euler(0, v, 0) * manualRotation * Quaternion.Euler(h, 0, 0);
        cam1.MoveCameraToInstant(manualRotation);
    }

}
