using UnityEngine;
using Exoa.Common;

namespace Exoa.Touch
{
    /// <summary>This component can be added alongside the <b>LeanTouch</b> component to add simulated multi touch controls using the mouse and keyboard.</summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InputTouch))]

    [AddComponentMenu("Exoa/Touch/Touch Simulator")]
    public class TouchSimulator : MonoBehaviour
    {
        /// <summary>This allows you to set which key is required to simulate multi key twisting.</summary>
		public KeyCode PinchTwistKey { set { pinchTwistKey = value; } get { return pinchTwistKey; } }
        [SerializeField] private KeyCode pinchTwistKey = KeyCode.LeftControl;

        /// <summary>This allows you to set which key is required to change the pivot point of the pinch twist gesture.</summary>
        public KeyCode MovePivotKey { set { movePivotKey = value; } get { return movePivotKey; } }
        [SerializeField] private KeyCode movePivotKey = KeyCode.LeftAlt;

        /// <summary>This allows you to set which key is required to simulate multi key dragging.</summary>
        public KeyCode MultiDragKey { set { multiDragKey = value; } get { return multiDragKey; } }
        [SerializeField] private KeyCode multiDragKey = KeyCode.LeftAlt;

        /// <summary>This allows you to set which texture will be used to show the simulated fingers.</summary>
        public Texture2D FingerTexture { set { fingerTexture = value; } get { return fingerTexture; } }
        [SerializeField] private Texture2D fingerTexture;

        // The current pivot (0,0 = bottom left, 1,1 = top right)
        private Vector2 pivot = new Vector2(0.5f, 0.5f);

        [System.NonSerialized]
        private InputTouch cachedTouch;

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            // Set the finger texture?
            if (FingerTexture == null)
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("FingerVisualization t:texture2d");

                if (guids.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);

                    FingerTexture = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path) as Texture2D;
                }
            }
        }
#endif

        protected virtual void OnEnable()
        {
            cachedTouch = GetComponent<InputTouch>();

            cachedTouch.OnSimulateFingers += HandleSimulateFingers;
        }

        protected virtual void OnDisable()
        {
            cachedTouch.OnSimulateFingers -= HandleSimulateFingers;
        }

        protected virtual void OnGUI()
        {
            // Show simulated multi fingers?
            if (FingerTexture != null)
            {
                var count = 0;

                foreach (var finger in InputTouch.Fingers)
                {
                    if (finger.Index < 0 && finger.Index != InputTouch.HOVER_FINGER_INDEX)
                    {
                        count += 1;
                    }
                }

                if (count > 1)
                {
                    foreach (var finger in InputTouch.Fingers)
                    {
                        // Simulated fingers have a negative index
                        if (finger.Index < 0)
                        {
                            var screenPosition = finger.ScreenPosition;
                            var screenRect = new Rect(0, 0, FingerTexture.width, FingerTexture.height);

                            screenRect.center = new Vector2(screenPosition.x, Screen.height - screenPosition.y);

                            GUI.DrawTexture(screenRect, FingerTexture);
                        }
                    }
                }
            }
        }

        private void HandleSimulateFingers()
        {
            // Simulate pinch & twist?
            if (BaseTouchInput.GetMouseExists() == true && BaseTouchInput.GetKeyboardExists() == true)
            {
                var mousePosition = BaseTouchInput.GetMousePosition();
                var mouseSet = false;
                var mouseUp = false;

                for (var i = 0; i < 5; i++)
                {
                    mouseSet |= BaseTouchInput.GetMouseIsHeld(i);
                    mouseUp |= BaseTouchInput.GetMouseWentUp(i);
                }

                if (mouseSet == true || mouseUp == true)
                {
                    if (BaseTouchInput.GetKeyIsHeld(MovePivotKey) == true)
                    {
                        pivot.x = mousePosition.x / Screen.width;
                        pivot.y = mousePosition.y / Screen.height;
                    }

                    if (BaseTouchInput.GetKeyIsHeld(PinchTwistKey) == true)
                    {
                        var center = new Vector2(Screen.width * pivot.x, Screen.height * pivot.y);

                        cachedTouch.AddFinger(-2, center - (mousePosition - center), 1.0f, mouseSet);
                    }
                    // Simulate multi drag?
                    else if (BaseTouchInput.GetKeyIsHeld(MultiDragKey) == true)
                    {
                        cachedTouch.AddFinger(-2, mousePosition, 1.0f, mouseSet);
                    }
                }
            }
        }
    }
}
