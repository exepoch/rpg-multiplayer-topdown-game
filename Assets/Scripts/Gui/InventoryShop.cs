using System.Collections;
using System.Collections.Generic;
using Managers;
using Network;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gui
{
    public class InventoryShop : MonoBehaviour,IShop
    {
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _itemName;
        [SerializeField] private TextMeshProUGUI _itemHealth;
        [SerializeField] private TextMeshProUGUI _value;
        [SerializeField] private TextMeshProUGUI _damage;


        [SerializeField] private List<ContentItem> contentItems = new List<ContentItem>();
        
        [SerializeField] private List<GameObject> titleEnabledImages;
        [SerializeField] private List<GameObject> titleFocusImages;
        [SerializeField] private List<GameObject> unRangedWeaponItems;
        [SerializeField] private List<GameObject> rangedWeaponItems;
        [SerializeField] private List<GameObject> heroItems;
        
        private readonly List<GameObject> itemFocusImages = new List<GameObject>();

        private ContentItem currentSelectedItem;

        private void Awake()
        {
            foreach (GameObject o in unRangedWeaponItems)
            {
                contentItems.Add(o.GetComponent<ContentItem>());
            }
            foreach (GameObject o in rangedWeaponItems)
            {
                contentItems.Add(o.GetComponent<ContentItem>());
            }

            foreach (ContentItem contentItem in contentItems)
            {
                itemFocusImages.Add(contentItem.FocusImage());
            }
            
            foreach (GameObject o in heroItems)
            {
                contentItems.Add(o.GetComponent<ContentItem>());
            }

            var dic = AccountManager.instance.GetWeapons();

            foreach (ContentItem contentItem in contentItems)
            {
                if (dic.Contains(contentItem.Content().GetSpecificInfo("key")))
                {
                    contentItem.LockImage().SetActive(true);
                }
            }

            OpenDetailPanel(contentItems[0]);
        }

        //Enables just self from OnClick event in editor.
        public void DisableAll()
        {
            foreach (GameObject o in titleEnabledImages)
            {
                o.SetActive(false);
            }

            foreach (GameObject o in titleFocusImages)
            {
                o.SetActive(false);
            }

            foreach (GameObject o in unRangedWeaponItems)
            {
                o.SetActive(false);
            }

            foreach (GameObject o in rangedWeaponItems)
            {
                o.SetActive(false);
            }
            
            foreach (GameObject o in heroItems)
            {
                o.SetActive(false);
            }
        }

        public void OpenDetailPanel(ContentItem item)
        {
            currentSelectedItem = item;

            //Check and lock only for weapons. Heroes can be purchased many times.
            if (currentSelectedItem.Content().Type())
            {
                if (AccountManager.instance.GetWeapons().Contains(currentSelectedItem.Content().GetSpecificInfo("key")))
                {
                    _buyButton.interactable = false;
                    _equipButton.interactable = true;
                }
                else
                {
                    _buyButton.interactable = true;
                    _equipButton.interactable = false;
                }   
            }
            else
            {
                if (!GameManager.HasEmptyAllieSlot())
                {
                    _buyButton.interactable = false;
                    _equipButton.interactable = false;
                }
                else
                {
                    _buyButton.interactable = true;
                    _equipButton.interactable = false;
                }
            }

            foreach (GameObject itemFocusImage in itemFocusImages)
            {
                itemFocusImage.SetActive(false);
            }

            _detailPanel.SetActive(false);

            _itemImage.sprite = currentSelectedItem.Image().sprite;
            currentSelectedItem.FocusImage().SetActive(true);
            _itemName.text = currentSelectedItem.Content().GetSpecificInfo("displayname");
            _value.text = currentSelectedItem.Content().GetSpecificInfo("price");
            _damage.text = currentSelectedItem.Content().GetSpecificInfo("damage");
            _itemHealth.text = currentSelectedItem.Content().GetSpecificInfo("durability");
            
            _detailPanel.SetActive(true);

        }

        public void ListRanged()
        {
            StartCoroutine(ListItems(rangedWeaponItems));
        }

        public void ListUnRanged()
        {
            StartCoroutine(ListItems(unRangedWeaponItems));
        }
        
        public void ListHeroes()
        {
            StartCoroutine(ListItems(heroItems));
        }

        public void Buy()
        {
            _buyButton.interactable = false;
            _equipButton.interactable = false;
            AccountManager.instance.PurhaseItem(currentSelectedItem.Content().Data(),
                this);
        }

        public void Equip()
        {
            InventoryManager.Player.RPC("RPCEquip", RpcTarget.AllBuffered, currentSelectedItem.Content().GetSpecificInfo("key"));
        }


        IEnumerator ListItems(List<GameObject> list)
        {
            foreach (GameObject o in list)
            {
                o.SetActive(true);
                yield return new WaitForSecondsRealtime(.05f);
            }
        }

        public void PurchaseCompleted()
        {
            //If weapon
            if (currentSelectedItem.Content().Type())
            {
                currentSelectedItem.LockImage().SetActive(true);
                _buyButton.interactable = false;
                _equipButton.interactable = true;   
            }
            else //If purhcased a hero
            {
                _buyButton.interactable = true;
                ActorManager.instance.SpawnAllied(currentSelectedItem.Content().GetSpecificInfo("key"));
            }
        }

        public void ShopSpecificPurchaseError()
        {
            _buyButton.interactable = true;
            _equipButton.interactable = false;
        }
    }
}
