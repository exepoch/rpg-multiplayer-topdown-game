using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gui
{
    public class UINavigator : MonoBehaviour
    {
        private EventSystem _system;
    
        private void Awake()
        {
            _system = FindObjectOfType<EventSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                var next = Input.GetKeyDown(KeyCode.LeftShift)
                    ? _system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp()
                    : _system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                if (next != null)
                {
                    InputField inputfield = next.GetComponent<InputField>();
                    if (inputfield != null)
                        inputfield.OnPointerClick(
                            new PointerEventData(_system)); //if it's an input field, also set the text caret

                    _system.SetSelectedGameObject(next.gameObject, new BaseEventData(_system));
                }
            }
        }
    }
}
