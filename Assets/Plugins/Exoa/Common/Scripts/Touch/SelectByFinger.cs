using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Exoa.Common;

namespace Exoa.Touch
{
    /// <summary>This component allows you to select LeanSelectable components.
    /// To use it, you can call the SelectScreenPosition method from somewhere (e.g. the LeanFingerTap.OnTap event).</summary>

    [AddComponentMenu("Exoa/Touch/Select By Finger")]
    public class SelectByFinger : TouchSelect
    {
        [System.Serializable] public class LeanSelectableLeanFingerEvent : UnityEvent<TouchSelectable, TouchFinger> { }

        public ScreenQuery ScreenQuery = new ScreenQuery(ScreenQuery.MethodType.Raycast);

        /// <summary>If you enable this then any selected object will automatically be deselected if the finger used to select it is no longer touching the screen.</summary>
        public bool DeselectWithFingers { set { deselectWithFingers = value; } get { return deselectWithFingers; } }
        [SerializeField] private bool deselectWithFingers;

        /// <summary>This is invoked when an object is selected.</summary>
        public LeanSelectableLeanFingerEvent OnSelectedFinger { get { if (onSelectedFinger == null) onSelectedFinger = new LeanSelectableLeanFingerEvent(); return onSelectedFinger; } }
        [SerializeField] private LeanSelectableLeanFingerEvent onSelectedFinger;

        public static event System.Action<SelectByFinger, TouchSelectable, TouchFinger> OnAnySelectedFinger;

        /// <summary>This method allows you to initiate selection at the finger's <b>StartScreenPosition</b>.
        /// NOTE: This method be called from somewhere for this component to work (e.g. LeanFingerTap).</summary>
        public void SelectStartScreenPosition(TouchFinger finger)
        {
            SelectScreenPosition(finger, finger.StartScreenPosition);
        }

        /// <summary>This method allows you to initiate selection at the finger's current <b>ScreenPosition</b>.
        /// NOTE: This method be called from somewhere for this component to work (e.g. LeanFingerTap).</summary>
        public void SelectScreenPosition(TouchFinger finger)
        {
            SelectScreenPosition(finger, finger.ScreenPosition);
        }

        /// <summary>This method allows you to initiate selection of a finger at a custom screen position.
        /// NOTE: This method be called from a custom script for this component to work.</summary>
        public void SelectScreenPosition(TouchFinger finger, Vector2 screenPosition)
        {
            var result = ScreenQuery.Query<TouchSelectable>(gameObject, screenPosition);

            Select(result, finger);
        }

        /// <summary>This method allows you to manually select an object with the specified finger using this component's selection settings.</summary>
        public void Select(TouchSelectable selectable, TouchFinger finger)
        {
            var pair = new SelectableByFinger.SelectedPair() { Finger = finger, Select = this };

            if (TrySelect(selectable) == true)
            {
                var selectableByFinger = selectable as SelectableByFinger;

                if (selectableByFinger != null)
                {
                    if (selectableByFinger.SelectingPairs.Contains(pair) == false)
                    {
                        selectableByFinger.SelectingPairs.Add(pair);
                    }

                    selectableByFinger.OnSelectedFinger.Invoke(finger);
                    selectableByFinger.OnSelectedSelectFinger.Invoke(this, finger);

                    SelectableByFinger.InvokeAnySelectedFinger(this, selectableByFinger, finger);

                    if (finger.Up == true)
                    {
                        selectableByFinger.OnSelectedFingerUp.Invoke(finger);
                        selectableByFinger.OnSelectedSelectFingerUp.Invoke(this, finger);

                        selectableByFinger.SelectingPairs.Remove(pair);
                    }
                }

                if (onSelectedFinger != null) onSelectedFinger.Invoke(selectable, finger);

                if (OnAnySelectedFinger != null) OnAnySelectedFinger.Invoke(this, selectable, finger);
            }
            else
            {
                if (finger.Up == false)
                {
                    var selectableByFinger = selectable as SelectableByFinger;

                    if (selectableByFinger != null)
                    {
                        if (selectableByFinger.SelectingPairs.Contains(pair) == false)
                        {
                            selectableByFinger.SelectingPairs.Add(pair);
                        }
                    }
                }
            }
        }

        protected virtual void Update()
        {
            if (deselectWithFingers == true && selectables != null)
            {
                for (var i = selectables.Count - 1; i >= 0; i--)
                {
                    var selectable = selectables[i];

                    if (ShouldRemoveSelectable(selectable) == true)
                    {
                        Deselect(selectable);
                    }
                }
            }
        }

        private bool ShouldRemoveSelectable(TouchSelectable selectable)
        {
            var selectableByFinger = selectable as SelectableByFinger;

            if (selectableByFinger != null)
            {
                foreach (var pair in selectableByFinger.SelectingPairs)
                {
                    if (pair.Finger.Up == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>This allows you to replace the currently selected objects with the ones in the specified list. This is useful if you're doing box selection or switching selection groups.</summary>
        public void ReplaceSelection(List<TouchSelectable> newSelectables, TouchFinger finger)
        {
            if (newSelectables != null)
            {
                // Deselect missing selectables
                if (selectables != null)
                {
                    for (var i = selectables.Count - 1; i >= 0; i--)
                    {
                        var selectable = selectables[i];

                        if (newSelectables.Contains(selectable) == false)
                        {
                            Deselect(selectable);
                        }
                    }
                }

                // Select new selectables
                foreach (var selectable in newSelectables)
                {
                    if (selectables == null || selectables.Contains(selectable) == false)
                    {
                        var selectableByFinger = selectable as SelectableByFinger;

                        if (selectableByFinger != null)
                        {
                            Select(selectableByFinger, finger);
                        }
                        else
                        {
                            Select(selectable);
                        }
                    }
                }
            }
            else
            {
                DeselectAll();
            }
        }
    }
}
