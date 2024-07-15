using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


namespace Exoa.Common
{
    /// <summary>This component allows you make the current GameObject selectable.</summary>

    [AddComponentMenu("Exoa/Touch/Selectable")]
    public class TouchSelectable : MonoBehaviour
    {
        [System.Serializable] public class LeanSelectEvent : UnityEvent<TouchSelect> { }

        public static LinkedList<TouchSelectable> Instances = new LinkedList<TouchSelectable>(); [System.NonSerialized] private LinkedListNode<TouchSelectable> instancesNode;

        public bool SelfSelected { set { if (selfSelected != value) { selfSelected = value; if (value == true) InvokeOnSelected(null); else InvokeOnDeslected(null); } } get { return selfSelected; } }
        [SerializeField] private bool selfSelected;

        /// <summary>This is invoked every time this object is selected.
        /// LeanSelect = The component that caused the selection (null = self selection).
        /// NOTE: This may occur multiple times.</summary>
        public LeanSelectEvent OnSelected { get { if (onSelected == null) onSelected = new LeanSelectEvent(); return onSelected; } }
        [SerializeField] private LeanSelectEvent onSelected;

        /// <summary>This is invoked every time this object is deselected.
        /// LeanSelect = The component that caused the deselection (null = self deselection).
        /// NOTE: This may occur multiple times.</summary>
        public LeanSelectEvent OnDeselected { get { if (onDeselected == null) onDeselected = new LeanSelectEvent(); return onDeselected; } }
        [SerializeField] private LeanSelectEvent onDeselected;

        public static event System.Action<TouchSelectable> OnAnyEnabled;

        public static event System.Action<TouchSelectable> OnAnyDisabled;

        public static event System.Action<TouchSelect, TouchSelectable> OnAnySelected;

        public static event System.Action<TouchSelect, TouchSelectable> OnAnyDeselected;

        protected static List<TouchSelectable> tempSelectables = new List<TouchSelectable>();

        /// <summary>This will tell you how many <b>LeanSelect</b> components in the scene currently have this object selected.</summary>
        public int SelectedCount
        {
            get
            {
                var count = 0;

                if (selfSelected == true)
                {
                    count += 1;
                }

                foreach (var select in TouchSelect.Instances)
                {
                    if (select.IsSelected(this) == true)
                    {
                        count += 1;
                    }
                }

                return count;
            }
        }

        /// <summary>This will tell you if this object is self selected, or selected by any <b>LeanSelect</b> components in the scene.</summary>
        public bool IsSelected
        {
            get
            {
                if (selfSelected == true)
                {
                    return true;
                }

                foreach (var select in TouchSelect.Instances)
                {
                    if (select.IsSelected(this) == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static int IsSelectedCount
        {
            get
            {
                var count = 0;

                foreach (var selectable in Instances)
                {
                    if (selectable.IsSelected == true)
                    {
                        count += 1;
                    }
                }

                return count;
            }
        }

        [ContextMenu("Deselect")]
        public void Deselect()
        {
            SelfSelected = false;

            foreach (var select in TouchSelect.Instances)
            {
                select.Deselect(this);
            }
        }

        /// <summary>This deselects all objects in the scene.</summary>
        public static void DeselectAll()
        {
            foreach (var select in TouchSelect.Instances)
            {
                select.DeselectAll();
            }

            foreach (var selectable in TouchSelectable.Instances)
            {
                selectable.SelfSelected = false;
            }
        }

        public void InvokeOnSelected(TouchSelect select)
        {
            if (onSelected != null)
            {
                onSelected.Invoke(select);
            }

            if (OnAnySelected != null)
            {
                OnAnySelected.Invoke(select, this);
            }
        }

        public void InvokeOnDeslected(TouchSelect select)
        {
            if (onDeselected != null)
            {
                onDeselected.Invoke(select);
            }

            if (OnAnyDeselected != null)
            {
                OnAnyDeselected.Invoke(select, this);
            }
        }

        protected virtual void OnEnable()
        {
            instancesNode = Instances.AddLast(this);

            if (OnAnyEnabled != null)
            {
                OnAnyEnabled.Invoke(this);
            }
        }

        protected virtual void OnDisable()
        {
            Instances.Remove(instancesNode); instancesNode = null;

            if (OnAnyDisabled != null)
            {
                OnAnyDisabled.Invoke(this);
            }
        }

        protected virtual void OnDestroy()
        {
            Deselect();
        }
    }
}
