using System;
using System.Collections.Generic;
using Cinemachine;
using Combat;
using Core;
using DG.Tweening;
using Gui;
using Network;
using Photon.Pun;
using UnityEngine;

namespace Managers
{
    [RequireComponent(typeof(PhotonView))]
    public class GameManager : MonoBehaviour,IPunObservable
    {
        #region Observable
        
        private const float balanceProcessRate = 5;
        private const byte maxAlliedCount = 4;
        private int nightCount = 1; //Survived nightCount
        private float happiness = 100;
        private int balance;
        private float lastBalanceUpdated;
        private static byte currentAlliedCount;

        #endregion

        private enum DayTime
        {
            Day,
            Night
        }

        public static GameManager instance;
        private InventoryManager _inventoryManager;

        public static Action OnBeforeNight; // What must happen before the night event
        public static Action OnBeforeDay; // What must happen before the day event
        
        [SerializeField] private Light _timeLight; //The light gets dark or light
        [SerializeField] private GameObject _dayTheme; //The light gets dark or light
        [SerializeField] private GameObject _nightTheme; //The light gets dark or light
        [SerializeField] private List<DestroyOnTouch> homeDoorCorners; //Home door locations for npcs to run at night
        [SerializeField] private List<GameObject> jobOwners;  //Static npc list for day

        private NetworkManager _networkManager;
        private ActorManager _actorManager;
        private DayTime _currentDayTime = DayTime.Day;

        private static bool _isDayTime = true; //True means Daytime

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
            
            _networkManager = FindObjectOfType<NetworkManager>();
            _actorManager = ActorManager.instance;
            nightCount = 1;
            _isDayTime = true;
            _inventoryManager = FindObjectOfType<InventoryManager>();

            NetworkManager.OnRoomCreated += OnRoomCreated;
        }

        private void Start()
        {
            // Connects to master lobby and creates a room for multiplayer
            if(_networkManager != null)
                _networkManager.SetupRoom();
        }

        private void Update()
        {
            lastBalanceUpdated += UnityEngine.Time.deltaTime;

            if (lastBalanceUpdated >= balanceProcessRate)
            {
                lastBalanceUpdated = 0;
                UpdateHappiness();
            }
        }

        //Call when entered photon room
        private void OnRoomCreated()
        {
            //Set active the button that can start night life, only available for the master client.
            GUIManager.instance.EnableDayMasterUIElements(true);
        }
        
        //Public method for button to change daytime 
        public void SetTime()
        {
            GetComponent<PhotonView>().RPC("RPCChangeTime", RpcTarget.AllBuffered);
        }
        
        //NpcSpawner method
        public void SpawnNpc()
        {
            FindObjectOfType<ActorManager>().SpawnNPCWave();
        }

        //Calls by SetTime metod
        [PunRPC]
        void RPCChangeTime()
        {
            _timeLight.DOComplete();
            switch (_currentDayTime)
            {
                case DayTime.Day://Do Night
                    _timeLight.DOIntensity(8, 3f); //Dark light gets power an become night
                    _currentDayTime = DayTime.Night;

                    NightLifeInitializer();
                    OnBeforeNight?.Invoke();
                    break;
                case DayTime.Night://Do Day
                    _timeLight.DOIntensity(0, 5f); //Dark light get lost and became day
                    _currentDayTime = DayTime.Day;
                    OnBeforeDay?.Invoke();
                    
                    DayLifeInitializer();
                    break;
            }
            
            //Other player cannot join while time is night and player is fighting, only day life
            PhotonNetwork.CurrentRoom.IsOpen = _currentDayTime == DayTime.Day;
        }


        //Sets the dayLife
        private void DayLifeInitializer()
        {
            SetTime(true);
            _dayTheme.SetActive(true);
            _nightTheme.SetActive(false);
            _inventoryManager.ShopActivate(true); //Shop is only available at day
            nightCount++;

            if (PhotonNetwork.IsMasterClient)
            {
                AccountManager.instance.GrantGoldToAccount(nightCount*2); //give player his reward
                GUIManager.instance.EnableDayMasterUIElements(true);
            }
            else
            {
                AccountManager.instance.GrantGoldToAccount(nightCount *.2f); //give helper player his reward
            }
            AccountManager.instance.UpdatePlayerStatistics("SurvivedNights",nightCount);

            var plH = FindObjectsOfType<PlayerController>();

            //player healts get filled at day
            foreach (PlayerController controller in plH)
            {
                var h = controller.GetComponent<Health>();
                h.FullFillHealth();
                
                if(controller.GetComponent<Fighter>().Get() == Fighter.FighterType.Player)
                    GUIManager.instance.PlayerInfoGUI(h.Current());
            }
            
            //For the test case, at night 8 the game is over.
            if (nightCount == 8)
            {
                PhotonNetwork.LeaveRoom();
                NotificationManager.PopUpOpener(PopUp.PopUpType.Notification, "Congrulations!", true);
                FindObjectOfType<SceneManagement>().LoadScene("Home", 2);
            }

            //Instantiate static npcs
            foreach (GameObject o in jobOwners)
            {
                PhotonNetwork.InstantiateRoomObject("Prefabs/Characters/"+o.name, o.transform.position, o.transform.rotation);
            }
        }

        //sets the night life
        private void NightLifeInitializer()
        {
            SetTime(false);
            _dayTheme.SetActive(false);
            _nightTheme.SetActive(true);
            _inventoryManager.ShopActivate(false);
            GUIManager.instance.EnableDayMasterUIElements(false);
            NotificationManager.PopUpOpener(PopUp.PopUpType.Notification, $"Day {nightCount}", true);
            _actorManager.SpawnEnemy(nightCount);
        }

        public void ChangeHappinesBalance(int value)
        {
            balance += value;
        }

        private static void  SetTime(bool set)
        {
            _isDayTime = set;
        }

        public static bool Time()
        {
            return _isDayTime;
        }

        private void UpdateHappiness()
        {
            happiness += balance;
            happiness = happiness >= 100 ? 100 : happiness;
            happiness = happiness <= 0 ? 0 : happiness;
            
            GUIManager.instance.SetHappiness(happiness/100f);
        }

        public List<DestroyOnTouch> GetHomePositionList()
        {
            return homeDoorCorners;
        }

        public static bool HasEmptyAllieSlot()
        {
            return currentAlliedCount < maxAlliedCount;
        }

        public static void RegisterAllie()
        {
            currentAlliedCount++;
            GUIManager.instance.UpdateAlliedCount(currentAlliedCount);
        }
        
        public static void UnRegisterAllie()
        {
            currentAlliedCount--;
            GUIManager.instance.UpdateAlliedCount(currentAlliedCount);
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(nightCount);
                stream.SendNext(happiness);
            }
            else
            {
                nightCount = (int)stream.ReceiveNext();
                happiness = (float)stream.ReceiveNext();
            }
        }
    }
}
