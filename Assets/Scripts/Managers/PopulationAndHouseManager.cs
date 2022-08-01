using System;
using Gui;
using Network;
using Photon.Pun;
using UnityEngine;

namespace Managers
{ 
    public class PopulationAndHouseManager : MonoBehaviour, IShop
    {
        private static byte CurrentPopulation;
        private House _house;

        [SerializeField] private static byte currenthouseCount = 1; //1 house can hold 5 people
        [SerializeField] private static int houseBedCount;
        [SerializeField] private static int availableBeds;


        private void Awake()
        {
            ActorManager.onNPCJoined += UpdatePopulation;
            
            ArrangeBeds();
        }

        public void OpenBuildPanel(House house)
        {
            _house = house;
            GUIManager.instance.OpenHouseBuilderPanel();
        }

        //When became Day, calculating coin that player gets, this is used to multipy
        public static int GetCurrentPopulation()
        {
            return CurrentPopulation;
        }
        //How many npc can hold this town
        public static int GetAvailableBedCount()
        {
            return availableBeds;
        }
        

        void UpdatePopulation()
        {
            CurrentPopulation = ActorManager.instance.GetNPCCount();
        }

        public void TryPurchaseHouse()
        {
            AccountManager.instance.PurhaseItem(Tuple.
                    Create(
                        "house", 
                        "Other",
                        1, 
                        "CO"),
                this);
        }

        public void PurchaseCompleted()
        {
            _house.GetComponent<PhotonView>().RPC("Build", RpcTarget.AllBuffered);
            currenthouseCount++;
            ArrangeBeds();
            GUIManager.instance.CloseHouseBuilderPanel();
            GUIManager.instance.UpdateInventory();
        }

        public void ShopSpecificPurchaseError()
        {
            GUIManager.instance.CloseHouseBuilderPanel();
        }

        void ArrangeBeds()
        {
            houseBedCount = houseBedCount*5;
            availableBeds = houseBedCount - CurrentPopulation;
        }
    }
    

}
