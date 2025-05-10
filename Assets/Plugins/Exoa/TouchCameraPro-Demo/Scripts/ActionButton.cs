using Exoa.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Designer.UI
{
    [AddComponentMenu("Exoa/Demo/ActionButton")]
    public class ActionButton : MonoBehaviour
    {
        public CameraEvents.Action action;
        private Button btn;

        void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            CameraEvents.OnRequestButtonAction?.Invoke(action, true);
        }
    }
}
