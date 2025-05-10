using Exoa.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/OnClickSetResetValues")]
    public class OnClickSetResetValues : MonoBehaviour
    {
        private Button btn;


        void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClickSetInitValues);
        }

        private void OnClickSetInitValues()
        {
            CameraPerspective camMode1 = GameObject.FindObjectOfType<CameraPerspective>();
            CameraIsometricOrtho camMode2 = GameObject.FindObjectOfType<CameraIsometricOrtho>();
            CameraTopDownOrtho camMode3 = GameObject.FindObjectOfType<CameraTopDownOrtho>();

            if (camMode1 != null && camMode1.enabled) camMode1.SetResetValues(camMode1.FinalOffset, camMode1.FinalRotation, camMode1.FinalDistance);
            if (camMode2 != null && camMode2.enabled) camMode2.SetResetValues(camMode2.FinalOffset, camMode2.FinalRotation, camMode2.FinalSize);
            if (camMode3 != null && camMode3.enabled) camMode3.SetResetValues(camMode3.FinalOffset, camMode3.FinalRotation, camMode3.FinalSize);
        }

    }
}
