using Exoa.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/DemoMenu")]
    public class DemoMenu : MonoBehaviour
    {
        private int index;
        private int total;
        public Button nextBtn;
        public Button prevBtn;

        public GameObject tooltipPrefab;
        private FocusOnClick[] focusObjs;
        private List<TooltipObject> tooltips;

        void Start()
        {
            index = SceneManager.GetActiveScene().buildIndex;
            total = SceneManager.sceneCountInBuildSettings;
            prevBtn.onClick.AddListener(OnClickPrev);
            nextBtn.onClick.AddListener(OnClickNext);

            focusObjs = GameObject.FindObjectsOfType<FocusOnClick>();
            CreateTooltips();
        }
        private void Update()
        {
            if (BaseTouchInput.GetKeyWentDown(KeyCode.I))
            {
                Canvas c = transform.root.GetComponent<Canvas>();
                c.enabled = !c.enabled;
            }
        }
        private void CreateTooltips()
        {
            tooltips = new List<TooltipObject>();
            for (int i = 0; i < focusObjs.Length; i++)
            {
                GameObject inst = Instantiate(tooltipPrefab, transform.parent);
                inst.transform.SetSiblingIndex(0);
                TooltipObject to = inst.GetComponent<TooltipObject>();
                to.Follow(focusObjs[i]);
                tooltips.Add(to);
            }
        }

        private void OnClickNext()
        {
            SceneManager.LoadScene(index < total - 1 ? index + 1 : 0);
        }

        private void OnClickPrev()
        {
            SceneManager.LoadScene(index > 0 ? index - 1 : total - 1);
        }

    }
}
