﻿using Exoa.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/HelpPopup")]
    public class HelpPopup : MonoBehaviour
    {
        public RectTransform popup;
        public Button closeBtn;
        private bool shown;

        void OnDestroy()
        {
            CameraEvents.OnRequestButtonAction -= OnRequestButtonAction;
        }

        void Start()
        {
            closeBtn?.onClick.AddListener(OnClickClose);
            Show(false);
            CameraEvents.OnRequestButtonAction += OnRequestButtonAction;
        }

        private void OnRequestButtonAction(CameraEvents.Action action, bool active)
        {
            if (action == CameraEvents.Action.Help)
            {
                shown = !shown;
                Show(shown);
            }
        }

        private void OnClickClose()
        {
            Show(false);
        }

        private void Show(bool v)
        {
            shown = v;
            if (popup != null) popup.gameObject.SetActive(v);
            //print("shown:" + shown);
        }

    }
}
