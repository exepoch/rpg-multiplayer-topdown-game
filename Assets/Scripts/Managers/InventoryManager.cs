using System.Collections.Generic;
using Gui;
using Network;
using Photon.Pun;
using Scriptable;
using UnityEngine;

namespace Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public static PhotonView Player;
            
        [SerializeField] private List<Weapon> wpnList;

        private static readonly Dictionary<string, Weapon> wpnLib = new Dictionary<string, Weapon>();

        private void Awake()
        {
            foreach (Weapon weapon in wpnList)
            {
                if(!wpnLib.ContainsKey(weapon.key))
                    wpnLib.Add(weapon.key, weapon);
            }
        }

        //Enables the button which opens the shop UI
        private void OnTriggerEnter(Collider other)
        {
            if(!other.CompareTag("Player")) return;
            
            Player = other.GetComponent<PhotonView>();
            if(Player.IsMine)
                GUIManager.instance.TriggerShopOpenerButton(true);
        }

        //Disables the button which opens the shop UI
        private void OnTriggerExit(Collider other)
        {
            if(!other.CompareTag("Player")) return;
            
            GUIManager.instance.TriggerShopOpenerButton(false);
            Player = null;
        }


        //Returns the player current weapon
        public static Weapon GetPlayerWeapon()
        {
            return wpnLib[AccountManager.instance.GetAccountWeapon()];
        }
        
        //Returns wepon which requested
        public static Weapon GetWeaponWithKey(string key)
        {
            return wpnLib[key];
        }

        //Opens the Shop UI
        public void ShopActivate(bool set)
        {
            GetComponent<BoxCollider>().enabled = set;
            transform.GetChild(0).gameObject.SetActive(set);
        }
    }
}
