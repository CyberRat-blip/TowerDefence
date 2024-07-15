using Exoa.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/StopFocusingBtn")]
    public class StopFocusingBtn : MonoBehaviour
    {
        private Button btn;

        void OnDestroy()
        {
            CameraEvents.OnFocusStart -= OnFocusStart;
            CameraEvents.OnFocusComplete -= OnFocusCompleteHandler;
        }

        void Start()
        {
            btn = GetComponentInChildren<Button>();
            btn.onClick.AddListener(OnClickBtn);

            CameraEvents.OnFocusStart += OnFocusStart;
            CameraEvents.OnFocusComplete += OnFocusCompleteHandler;
            Display(false);
        }

        private void OnFocusStart()
        {
            Display(true);
        }


        private void OnFocusCompleteHandler(GameObject obj)
        {
            Display(false);
        }

        private void Display(bool display)
        {
            btn.gameObject.SetActive(display);
        }

        private void OnClickBtn()
        {
            CameraEvents.OnRequestStopFocusFollow?.Invoke();
            Display(false);
        }

    }
}
