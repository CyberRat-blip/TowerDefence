using Exoa.Cameras;
using Exoa.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/OnClickFollowObject")]

    public class OnClickFollowObject : MonoBehaviour
    {
        private Button btn;
        public GameObject playerGo;

        void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClickSetInitValues);
        }

        private void OnClickSetInitValues()
        {
            CameraEvents.OnRequestObjectFollow?.Invoke(playerGo, false, false);
        }

    }
}
