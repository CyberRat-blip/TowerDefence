using Exoa.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/FPSFixer")]

    public class FPSFixer : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (BaseTouchInput.GetKeyWentDown(KeyCode.F1))
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
            }
            if (BaseTouchInput.GetKeyWentDown(KeyCode.F2))
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = 60;
            }
            if (BaseTouchInput.GetKeyWentDown(KeyCode.F3))
            {
                QualitySettings.vSyncCount = 2;
                Application.targetFrameRate = 30;
            }
            if (BaseTouchInput.GetKeyWentDown(KeyCode.F4))
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 30;
            }
        }
    }
}
