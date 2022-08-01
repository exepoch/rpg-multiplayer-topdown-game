using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Gui
{
    public class GuiControllerCustomizedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent OnButtonUp;
        public UnityEvent OnButtonDown;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            OnButtonDown.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnButtonUp.Invoke();
        }
    }
}
