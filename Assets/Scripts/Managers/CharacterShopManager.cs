using System.Collections.Generic;
using Gui;
using Network;
using UnityEngine;

namespace Managers
{
    public class CharacterShopManager : MonoBehaviour, IShop
    {
        [SerializeField] private SkinnedMeshRenderer _renderer;
        [SerializeField] private Transform UI3D; // 3D visual for inpecting
        [SerializeField] private GameObject _purchaseActorButton;
        [SerializeField] private GameObject _setActorButton;
        [SerializeField] private GameObject _buyCheckPanel;  //Inpect character panel before buying
        [SerializeField] private List<CharacterContentItem> contentList;  //Character list in shop

        private CharacterContentItem _currentItem;

        private void Awake()
        {
            var dic = AccountManager.instance.GetCharacters(); //Loads all available characters
        
            _purchaseActorButton.SetActive(false);
            _setActorButton.SetActive(true);
        
            //Sets the buy and select buttons according to user purchased chatacter list
            foreach (CharacterContentItem item in contentList)
            {
                if (dic.Contains(item.GetActor().key))
                {
                    _currentItem = item;
                    PurchaseCompleted();
                }
            }

            _currentItem = contentList[0];
        }

        private void FixedUpdate()
        {
            UI3D.transform.Rotate(Vector3.up);
        }

        
        //When user selects an character item, sets the selection visual on shop.
        public void ContentButton(CharacterContentItem item)
        {
            var oldItem = _currentItem;
            _currentItem = item;

            foreach (CharacterContentItem o in contentList)
            {
                o.SelectionFrameOff();           
            }

            _currentItem.GetActor().Spawn(UI3D.gameObject,_renderer, oldItem.GetActor().key);

            var deger = item.IsPurchased(); 
            _purchaseActorButton.SetActive(!deger);
            _setActorButton.SetActive(deger);
        }

        //When player clicks the buy button, request a purchase the item
        public void BuyActorRequest()
        {
            AccountManager.instance.PurhaseItem(_currentItem.GetActor().Data(),this);
        }

        //Sets the active player visual for account
        public void SetCurrentActor()
        {
            AccountManager.instance.SetActor(_currentItem.GetActor().key);
        }

        
        //Set buttons for buy or select
        public void PurchaseCompleted()
        {
            _currentItem.LockOff(); 
            _buyCheckPanel.SetActive(false);
        
            _purchaseActorButton.SetActive(false);
            _setActorButton.SetActive(true);
        }

        public void ShopSpecificPurchaseError()
        {
            //
        }
    }
}
