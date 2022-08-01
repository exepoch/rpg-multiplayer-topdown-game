using Scriptable;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gui
{
    public class ContentItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private ScriptableResource _content;
        [SerializeField] private Image _itemImage;
        [SerializeField] private GameObject lockImage;
        [SerializeField] private GameObject selectionFrame;

        private void Awake()
        {
            //Get data from accountM is weapon has purchased
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            FindObjectOfType<InventoryShop>().OpenDetailPanel(this);
        }

        public GameObject FocusImage()
        {
            return selectionFrame;
        }
        
        public GameObject LockImage()
        {
            return lockImage;
        }

        public Image Image()
        {
            return _itemImage;
        }

        public ScriptableResource Content()
        {
            return _content;
        }
    }
}
