using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DG.Tweening;
using Managers;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gui
{
    [SuppressMessage("ReSharper", "SpecifyACultureInStringConversionExplicitly")]
    public class GUIManager : MonoBehaviour
    {
        public static GUIManager instance;

        [SerializeField] private bool isHomeScreen;
        private AccIAP _accIAP;
        
        [Header("GlobalPlayerInfo")]
        [SerializeField] private TextMeshProUGUI username;
        [SerializeField] private TextMeshProUGUI level;
        [SerializeField] private TextMeshProUGUI exeperianceText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI homeGems;
        [SerializeField] private Slider exeperianceValue;
        [SerializeField] private Slider healthValue;
        
        #region HomeVariables

        [Header("HomeSceneVariables")]
        [SerializeField] private TextMeshProUGUI shopGold;
        [SerializeField] private TextMeshProUGUI shopGems;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private Button joinButton;
        #endregion

        #region GameVariables

        [Header("Game")]
        [SerializeField] private TextMeshProUGUI _roomNameText;
        [SerializeField] private TextMeshProUGUI _housePriceText;
        [SerializeField] private TextMeshProUGUI _alliedCountText;
        [SerializeField] private GameObject _shopOpenerButton;
        [SerializeField] private Button _nightReadyButton;
        [SerializeField] private GameObject _buildHousePanel;
        [SerializeField] private Button _purhaseHouseButton;
        [SerializeField] private Slider _happinessBar;
        [SerializeField] private List<GameObject> _hostDayUIEelements; //UI objects that only appears for Host of the game.

        #endregion

        private void Awake()
        {
            instance = this;
            UpdateInventory();
        }

        public void UpdateInventory()
        {
            if (AccountManager.instance == null)
            {
                Debug.LogWarning("Scene must have AccountManager!");
                return;
            }

            var tuple = AccountManager.instance.GetAccGuiData();
            _accIAP = new AccIAP
            {
                accUserName =  tuple.Item1,
                accLevel= tuple.Item2,
                accExperiance= tuple.Item3,
                accGold= tuple.Item4,
                accGems= tuple.Item5
            };
            
            username.text = _accIAP.accUserName;
            level.text = _accIAP.accLevel.ToString();
            exeperianceText.text = _accIAP.accExperiance.ToString();
            exeperianceValue.value = Mathf.Clamp01(_accIAP.accExperiance/100);
            goldText.text = _accIAP.accGold.ToString();
            homeGems.text = _accIAP.accGems.ToString();
            
            if (isHomeScreen)
            {
                shopGold.text = _accIAP.accGold.ToString();
                shopGems.text = _accIAP.accGems.ToString();
                tutorialPanel.SetActive(true);
            }
        }

        public void TriggerShopOpenerButton(bool set)
        {
            _shopOpenerButton.SetActive(set);   
        }
        
        public void SetHappiness(float set)
        {
            _happinessBar.DOValue(set, 1);
        }

        public void UpdateAlliedCount(byte count)
        {
            _alliedCountText.text = count.ToString();
        }
        
        public void UpdateExperiance(float expVal)
        {
            exeperianceValue.DOValue(expVal / 100, .5f);
        }

        /// <summary>
        /// Enables the buttons whichs can be seen by host client at the day time
        /// </summary>
        public void EnableDayMasterUIElements(bool set)
        {
            foreach (GameObject o in _hostDayUIEelements)
            {
                o.SetActive(set);
            }
        }

        public void PlayerInfoGUI(float health)
        {
            healthValue.value = 1f / 100f * health;
        }

        public void OpenHouseBuilderPanel()
        {
            _purhaseHouseButton.interactable = true;
            _housePriceText.text = 1 + " CO";
            _buildHousePanel.SetActive(true);
        }

        public void CloseHouseBuilderPanel()
        {
            _buildHousePanel.SetActive(false);
        }

        #region ButtonEvents

        public void SetPlacerMod(bool set)
        {
            _nightReadyButton.interactable = !set;
            AIPlaceManager.SetMod(set);
        }
        
        public void PurchaseHouseRequest()
        {
            if(!GameManager.Time()) return;
            _purhaseHouseButton.interactable = false;
            FindObjectOfType<PopulationAndHouseManager>().TryPurchaseHouse();
        }
        
        public void HomeScenePlayButton()
        { 
            tutorialPanel.SetActive(!PlayerPrefManager.Tutorial());
        }

        public void LoadSceneWithName(string sceneName)
        {
            FindObjectOfType<SceneManagement>().LoadScene(sceneName);
        }

        public void SetRoomMaxPlayer(TMP_Dropdown set)
        {
            var p = set.value + 1;
            FindObjectOfType<NetworkManager>().SetMaxPlayer((byte)p);
        }

        public void SetHostorJoin(bool host)
        {
            FindObjectOfType<NetworkManager>().SetHostorJoin(host);
        }
        public void SetHostorJoin(TMP_InputField host)
        {
            bool canJoin = FindObjectOfType<NetworkManager>().SetHostorJoin(host.text);
            joinButton.interactable = canJoin;
        
            if(!canJoin)
                NotificationManager.PopUpOpener(PopUp.PopUpType.Error, "Invalid Room Name", true, 1);
        }

        public void GetRoomId()
        {
            _roomNameText.text = NetworkManager.RoomId();
        }

        #endregion
    }
}