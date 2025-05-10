using Exoa.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/ChangeGroundHeight")]
    public class TestChangeGroundHeight : MonoBehaviour
    {
        private CameraBase cb;
        public Transform transparentGrid;
        public Button level1Btn;
        public Button level2Btn;
        public Button level3Btn;
        private float targetGroundHeight;
        private float groundHeight;

        void Start()
        {
            cb = GetComponent<CameraBase>();
            groundHeight = targetGroundHeight = cb.groundHeight;
            level1Btn.onClick.AddListener(() => ChangeLevel(0));
            level2Btn.onClick.AddListener(() => ChangeLevel(10));
            level3Btn.onClick.AddListener(() => ChangeLevel(20));
        }

        private void ChangeLevel(int v)
        {
            targetGroundHeight = v;
        }

        void Update()
        {
            groundHeight = Mathf.Lerp(groundHeight, targetGroundHeight, .05f);
            if (groundHeight != cb.groundHeight)
            {
                cb.SetGroundHeight(groundHeight);
                transparentGrid.position = transparentGrid.position.SetY(groundHeight + .01f);
            }
        }
    }
}
