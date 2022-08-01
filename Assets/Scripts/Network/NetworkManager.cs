using System;
using System.Collections.Generic;
using Combat;
using Gui;
using Managers;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static Action OnRoomCreated;
        private ActorManager _actorManager;

        [SerializeField] private byte maxPlayer = 1;
        private static string roomID;
        private bool host = true;
        private string joinRequest;
        private float lastConnectionChecked = 2;

        private static List<PhotonView> _playerList;
        private Dictionary<string, RoomInfo> cachedRoomList;

        public List<string> roomKeyList;
        public List<string> roomNameList;

        #region Unity

        private void Awake()
        {
            cachedRoomList = new Dictionary<string, RoomInfo>();
            roomKeyList = new List<string>();
            roomNameList = new List<string>();
            _playerList = new List<PhotonView>();
            ActorManager.onPlayerJoined += UpdatePlayerList;
        }

        private void Update()
        {
            lastConnectionChecked -= 1 * Time.deltaTime;
        
            if(lastConnectionChecked <=0 && Application.internetReachability == NetworkReachability.NotReachable)
            {
                lastConnectionChecked = 6;
                NotificationManager.PopUpOpener(PopUp.PopUpType.ConnectionWarning, selfDes:true, desTime:3);
            }
        }

        public static void ConnectToLobby()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public void SetupRoom()
        {
            if(_actorManager == null)
                _actorManager = FindObjectOfType<ActorManager>();
        
            if (host)
            {
                do
                {
                    roomID = Guid.NewGuid().ToString();
                    roomID = roomID.Substring(0, 8).ToUpper();    
                } while (cachedRoomList.ContainsKey(roomID));
            
                cachedRoomList.Clear();
                PhotonNetwork.CreateRoom(roomID, new RoomOptions {MaxPlayers = maxPlayer});
                print("RoomID :" + roomID);
            }
            else
            {
                PhotonNetwork.JoinOrCreateRoom(joinRequest, new RoomOptions(), TypedLobby.Default);
            }
        }
    
        public static string RoomId()
        {
            return roomID;
        }

        private static void UpdatePlayerList()
        {
            var plList = ActorManager.instance.GetPlayerList();

            _playerList.Clear();
            foreach (Fighter fighter in plList)
            {
                _playerList.Add(fighter.GetComponent<PhotonView>());
            }
        }

        //If master client left, all players must left
        private static void LeaveAllPlayer()
        {
            foreach (PhotonView view in _playerList)
            {
                if(view.IsMine) continue;
                view.RPC("Leave", RpcTarget.All);
            }
        }

        public static void Leave()
        {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            FindObjectOfType<SceneManagement>().LoadScene("Home", 1);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Leave();
        }

        public void SetMaxPlayer(byte count)
        {
            maxPlayer = count;
        }

        public void SetHostorJoin(bool set)
        {
            host = set;
        }
        public bool SetHostorJoin(string set)
        {
            if (!cachedRoomList.ContainsKey(set)) return false;
        
            host = false;
            joinRequest = set;
            return true;

        }
        #endregion

        #region PhotonCallBacks

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            print("JoinedLobby");
        }

        public override void OnCreatedRoom()
        {
            OnRoomCreated?.Invoke();
        }

        public override void OnJoinedRoom()
        {
            cachedRoomList.Clear();
            _actorManager.SpawnPlayer();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            NotificationManager.PopUpOpener(PopUp.PopUpType.Error,  message);
            joinRequest = "";
        }
    
        public override void OnLeftLobby()
        {
            cachedRoomList.Clear();
        }

        #endregion

        #region RoomProcces

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            UpdateCachedRoomList(roomList);
        
            roomKeyList.Clear();
            roomNameList.Clear();
            foreach (KeyValuePair<string,RoomInfo> info in cachedRoomList)
            {
                roomKeyList.Add(info.Key);
                roomNameList.Add(info.Value.Name);
            }
        }
    
        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }

        #endregion
    }
}
