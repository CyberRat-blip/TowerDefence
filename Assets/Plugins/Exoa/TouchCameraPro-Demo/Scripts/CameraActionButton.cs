using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("Exoa/Demo/CameraActionButton")]
public class CameraActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
{
    public delegate void CameraActionButtonHandler();
    public CameraActionButtonHandler onPressed;
    public CameraActionButtonHandler onClicked;
    private bool isPressed;
    private float lastPressedDown;
    private float pressDownDelay = .1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        //lastPressedDown = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }



    void Update()
    {
        //if (isPressed && lastPressedDown < Time.time - pressDownDelay)
        //    isPressed = false;

        if (isPressed)
        {
            onPressed?.Invoke();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");
        isPressed = false;
    }
}
