using System;
using System.Collections;
using System.Collections.Generic;
using Exoa.Designer;
using TMPro;
using UnityEngine;

namespace Exoa.Cameras.Demos
{
    [AddComponentMenu("Exoa/Demo/TooltipObject")]
    public class TooltipObject : MonoBehaviour
    {
        private FocusOnClick tracked;
        public TMP_Text tooltipTxt;
        private Canvas canvas;
        private RectTransform rt;
        private RectTransform canvasRt;

        void Start()
        {
            canvas = transform.root.GetComponent<Canvas>();
            canvasRt = canvas.transform as RectTransform;
            rt = transform as RectTransform;
        }

        // Update is called once per frame
        void Update()
        {
            if (tracked != null && tracked.gameObject != null && canvas != null)
            {
                tooltipTxt.text = "Click Me!\nfocus:" + tracked.Focus + " follow:" + tracked.Follow;
                rt.anchoredPosition = canvasRt.WorldPointToRectTransformPosition(tracked.transform.position, Camera.main);
            }
        }

        internal void Follow(FocusOnClick tracked)
        {
            this.tracked = tracked;
        }
    }
}
