using Exoa.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/OnClickMoveToPosition")]
    public class OnClickMoveToPosition : MonoBehaviour
    {
        private Button btn;

        public ITouchCamera camMode;
        private Vector3 randomPosition;
        private Quaternion randomRotation;
        private float randomDistance;

        public bool addRandomPosition;
        public bool addRandomDistance;
        public bool addRandomRotation;
        public bool isInstant;

        void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClickSetInitValues);
            if (camMode == null)
                camMode = GameObject.FindObjectOfType<CameraModeSwitcher>();
            if (camMode == null)
                camMode = GameObject.FindObjectOfType<CameraBase>();

        }

        private void OnClickSetInitValues()
        {
            randomPosition = new Vector3(UnityEngine.Random.Range(-15, 15), 0, UnityEngine.Random.Range(-15, 15));
            randomDistance = UnityEngine.Random.Range(4, 20);

            // if the current camera mode is top down, then we only play with the y axis
            if (camMode is CameraTopDownOrtho || (camMode is CameraModeSwitcher && CameraModeSwitcher.Instance.CurrentCameraMode is CameraTopDownOrtho))
            {
                randomRotation = Quaternion.Euler(90, UnityEngine.Random.Range(0, 360), 0);
            }
            else
            {
                randomRotation = Quaternion.Euler(UnityEngine.Random.Range(5, 80), UnityEngine.Random.Range(0, 360), 0);
            }

            if (camMode != null)
            {
                if (addRandomPosition && !addRandomDistance && !addRandomRotation && !isInstant)
                    camMode.MoveCameraTo(randomPosition);
                else if (addRandomPosition && !addRandomDistance && !addRandomRotation && isInstant)
                    camMode.MoveCameraToInstant(randomPosition);
                else if (addRandomPosition && !addRandomDistance && addRandomRotation && !isInstant)
                    camMode.MoveCameraTo(randomPosition, randomRotation);
                else if (addRandomPosition && !addRandomDistance && addRandomRotation && isInstant)
                    camMode.MoveCameraToInstant(randomPosition, randomRotation);
                else if (addRandomPosition && addRandomDistance && !addRandomRotation && !isInstant)
                    camMode.MoveCameraTo(randomPosition, randomDistance);
                else if (addRandomPosition && addRandomDistance && !addRandomRotation && isInstant)
                    camMode.MoveCameraToInstant(randomPosition, randomDistance);
                else if (addRandomPosition && addRandomDistance && addRandomRotation && !isInstant)
                    camMode.MoveCameraTo(randomPosition, randomDistance, randomRotation);
                else if (addRandomPosition && addRandomDistance && addRandomRotation && isInstant)
                    camMode.MoveCameraToInstant(randomPosition, randomDistance, randomRotation);
                else if (!addRandomPosition && addRandomDistance && !addRandomRotation && isInstant)
                    camMode.MoveCameraToInstant(randomDistance);
                else if (!addRandomPosition && !addRandomDistance && addRandomRotation && isInstant)
                    camMode.MoveCameraToInstant(randomRotation);
                else if (!addRandomPosition && addRandomDistance && !addRandomRotation && !isInstant)
                    camMode.MoveCameraTo(randomDistance);
                else if (!addRandomPosition && !addRandomDistance && addRandomRotation && !isInstant)
                    camMode.MoveCameraTo(randomRotation);

            }
        }

    }
}
