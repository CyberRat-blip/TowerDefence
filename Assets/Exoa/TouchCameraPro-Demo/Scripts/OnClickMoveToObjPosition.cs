using Exoa.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/OnClickMoveToObjPosition")]
    public class OnClickMoveToObjPosition : MonoBehaviour
    {
        private Button btn;
        public List<Transform> targetObjs;
        private int index;
        public ITouchCamera camMode;

        void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClickMoveTo);
            if (camMode == null)
                camMode = GameObject.FindObjectOfType<CameraModeSwitcher>();
            if (camMode == null)
                camMode = GameObject.FindObjectOfType<CameraBase>();
        }

        private void OnClickMoveTo()
        {
            if (index > targetObjs.Count - 1)
                index = 0;
            Transform targetObj = targetObjs[index];
            index++;

            Vector3 targetPostion = targetObj.position;
            float targetDistance = UnityEngine.Random.Range(4, 20);
            Quaternion targetRotation = Quaternion.LookRotation(-targetObj.forward);

            if (camMode != null)
            {
                camMode.MoveCameraTo(targetPostion, targetDistance, targetRotation);
            }
        }

    }
}
