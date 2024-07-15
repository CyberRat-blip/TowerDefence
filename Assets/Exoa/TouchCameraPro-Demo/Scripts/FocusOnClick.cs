using Exoa.Events;
using Exoa.Common;

namespace Exoa.Cameras.Demos
{
    public class FocusOnClick : TouchSelectableBehaviour
    {
        public bool follow;
        public bool focusOnFollow;
        public bool allowYoffset;

        public bool Focus
        {
            get => follow == false || (follow == true && focusOnFollow);
        }
        public bool Follow
        {
            get => follow || focusOnFollow;
        }
        protected override void OnSelected(TouchSelect select)
        {
            print("OnSelected " + gameObject.name);
            if (follow)
                CameraEvents.OnRequestObjectFollow?.Invoke(gameObject, focusOnFollow, allowYoffset);
            else
                CameraEvents.OnRequestObjectFocus?.Invoke(gameObject, allowYoffset);
        }

        protected override void OnDeselected(TouchSelect select)
        {
            print("OnDeselected " + gameObject.name);
            if (follow)
                CameraEvents.OnRequestObjectFollow?.Invoke(null, focusOnFollow, false);
        }
    }
}
