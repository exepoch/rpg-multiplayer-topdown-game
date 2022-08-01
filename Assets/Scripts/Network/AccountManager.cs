using System;
using System.Collections;
using System.Collections.Generic;
using Gui;
using Managers;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using WebSocketSharp;

namespace Network
{
    [Serializable]
    public class AccIAP
    {
        public string accID;//Fills by REST API
        public string accUserName; //Fills by REST API
        public int accLevel = 1;  //Custom Json Data
        public float accExperiance; //Custom Json Data
        public float accGold;//Fills by REST API
        public float accGems; //Fills by REST API
        public string currentWeapon; //Fills by REST API
        public string currentActor; //Fills by REST API
        [SerializeField] public List<string> accCharacters; //Custom Json Data
        [SerializeField] public List<string> accWeapons; //Custom Json Data
    }

    public class AccountManager : MonoBehaviour, IStoreListener
    {
        public static AccountManager instance;
        private static bool loggedIn;
        [SerializeField]private  AccIAP account;

        [Header("Panels")]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _regPanel;
        [SerializeField] private GameObject _forgotSubmitPanel;
        [SerializeField] private Toggle _remToggle;
    
        [Header("Login Input Field")]
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private TMP_InputField forgotEmailInput;
    
        [Header("Register Input Field")]
        [SerializeField] private TMP_InputField regEmailInput;
        [SerializeField] private TMP_InputField regUserNameInput;
        [SerializeField] private TMP_InputField regPasswordInput;

        private List<CatalogItem> Catalog;
        private List<PlayerLeaderboardEntry> _survivedNightStatistic;
        private List<PlayerLeaderboardEntry> _enemyKilledStatistic;

        private bool rememberAccountCredentials;
        private bool hassent;

        private void Start()
        {
            LocalReset();
        }

        #region InternalCallBacks

        private void ImportAccountDataFromAPI()
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
            {
                account = JsonUtility.FromJson<AccIAP>(result.Data["account"].Value);
                PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), getAccountInfoResult =>
                {
                    account.accUserName = getAccountInfoResult.AccountInfo.Username;
                    account.accID = getAccountInfoResult.AccountInfo.PlayFabId;
                    PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), getUserInventoryResult =>
                    {
                        account.accGold = getUserInventoryResult.VirtualCurrency["CO"];
                        account.accGems = getUserInventoryResult.VirtualCurrency["GM"];
                        var re = getUserInventoryResult.Inventory;

                        account.accCharacters.Clear();
                        account.accWeapons.Clear();
                    
                        //Initial gift for every account. (This section must migrate to cloud script via Playfab)
                        account.accCharacters.Add("base_character");
                        account.accWeapons.Add("basic_sword");
                        //
                    
                        foreach (ItemInstance item in re)
                        {
                            if (item.CatalogVersion == "Characters")
                            {
                                account.accCharacters.Add(item.ItemId);
                            }
                            else
                            {
                                account.accWeapons.Add(item.ItemId);
                            }
                        }
                        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), getCatalogItemsResult =>
                        {
                            Catalog = getCatalogItemsResult.Catalog;
                            UpdateLeaderboards();
                            loggedIn = true;
                        }, OnError);
                    }, OnError);
                }, OnError);
            }, OnError);
        }
    
        /// <summary>
        /// Updates user custom account data on Playfab
        /// </summary>
        private void UpdateAccountData()
        {
            if (GUIManager.instance != null)
            {
                GUIManager.instance.UpdateInventory();   
            }
            
            string json = JsonUtility.ToJson(account);
            var updateUserDataRequest = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {"account", json}
                }
            };
            PlayFabClientAPI.UpdateUserData(updateUserDataRequest, OnDataSend,OnError);
        }

        void LocalReset()
        {
            AccountManager.instance = this;
        
            _loginPanel.SetActive(false);
            _regPanel.SetActive(false);
            _forgotSubmitPanel.SetActive(false);
            loggedIn = false;

            rememberAccountCredentials = PlayerPrefManager.RememberCredentials();

            if (rememberAccountCredentials)
            {
                loginEmailInput.text = PlayerPrefManager.Email();
                loginPasswordInput.text = PlayerPrefManager.Pass();
            }
        }
    
        private void LeaderBoardRequest(string statistic)
        {
            List<PlayerLeaderboardEntry> entry;
            var req = new GetLeaderboardRequest
            {
                MaxResultsCount = 10,
                StartPosition = 0,
                StatisticName = statistic,

            };
            
            PlayFabClientAPI.GetLeaderboard(req, result =>
            {
                switch (statistic)
                {
                    case "SurvivedNights":
                        _survivedNightStatistic = result.Leaderboard;
                        break;
                    case "EnemyKilled":
                        _enemyKilledStatistic = result.Leaderboard;
                        break;
                }

                var panel = FindObjectOfType<RankPanel>();
                if (panel != null)
                {
                    panel.UpdateLeaderBoard();
                }
            }, er=> {});
        }
        #endregion
    
        #region ExternallCallBacks

        //key,catalog,price,currency
        public void PurhaseItem(Tuple<string,string,int,string> holder, IShop shop)
        {
            var req = new PurchaseItemRequest
            {
                ItemId = holder.Item1,
                Price = holder.Item3,
                VirtualCurrency = holder.Item4,
                CatalogVersion = holder.Item2
            };

            PlayFabClientAPI.PurchaseItem(req, result =>
            {
                switch (holder.Item4)
                {
                    case "Characters":
                        account.accCharacters.Add(holder.Item1);
                        break;
                    case "Weapons":
                        account.accWeapons.Add(holder.Item1);
                        break;
                    case "Other":
                        shop.PurchaseCompleted();
                        return;
                }

                account.accGold -= holder.Item3;
                shop.PurchaseCompleted();
                UpdateAccountData();
            }, error =>
            {
                shop.ShopSpecificPurchaseError();
                OnError(error);
            });
        }

        public void UpdateLeaderboards()
        {
            LeaderBoardRequest("SurvivedNights");
            LeaderBoardRequest("EnemyKilled");
        }

        public void UpdatePlayerStatistics(string statistic, int val)
        {
            PlayFabClientAPI.UpdatePlayerStatistics( new UpdatePlayerStatisticsRequest {
                    // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
                    Statistics = new List<StatisticUpdate> {
                        new StatisticUpdate { StatisticName = statistic, Value = val },
                    }
                }, result => { print("PlayerStatisticUpdadet");}, OnError);
        }

        public void SetActor(string key)
        {
            if(key == account.currentActor) return;

            account.currentActor = key;
            UpdateAccountData();
        }

        public void SetWeapon(string key)
        {
            if(key == account.currentWeapon) return;

            account.currentWeapon = key;
            UpdateAccountData();
        }

        public string GetAccountCharacter()
        {
            return account.currentActor;
        }
    
        public float GetAccountExperiance()
        {
            return account.accExperiance;
        }

        public string GetAccountUserName()
        {
            return account.accUserName;
        }
        
        public int GetAccountLevel()
        {
            return account.accLevel;
        }
    
        public string GetAccountWeapon()
        {
            return account.currentWeapon;
        }

        public List<string> GetCharacters()
        {
            return account.accCharacters;
        }
        
        public List<string> GetWeapons()
        {
            return account.accWeapons;
        }

        public Tuple<string, int, float, float, float> GetAccGuiData()
        {
            return Tuple.Create(
                account.accUserName,
                account.accLevel,
                account.accExperiance,
                account.accGold,
                account.accGems);
        }

        public void GrantGoldToAccount(float val)
        {
            account.accGold += val;
            UpdateAccountData();
        }

        private void GrantLevelToAccount()
        {
            account.accLevel++;
            UpdateAccountData();
        }
        
        public void GrantExperianceToAccount(float val)
        {
            account.accExperiance += val;
            if (account.accExperiance >= 100)
            {
                account.accExperiance -= 100;
                GrantLevelToAccount();
                return;
            }
            
            UpdateAccountData();
            GUIManager.instance.UpdateExperiance(account.accExperiance);
        }

        public List<PlayerLeaderboardEntry> GetSurvivedDayRanking()
        {
            return _survivedNightStatistic;
        }
        
        public List<PlayerLeaderboardEntry> GetEnemyKilledRanking()
        {
            return _enemyKilledStatistic;
        }
    
        #endregion

        #region LoginCredentials
    
        public void LoginWithEmailButton()
        {
            var request = new LoginWithEmailAddressRequest
            {
                Email = loginEmailInput.text,
                Password = loginPasswordInput.text
            };
            NotificationManager.PopUpOpener(PopUp.PopUpType.NetworkLoad , set:false);
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
        }

        public void RegisterButton()
        {
            var request = new RegisterPlayFabUserRequest
            {
                Email = regEmailInput.text,
                Username = regUserNameInput.text,
                Password = regPasswordInput.text,
            };
            NotificationManager.PopUpOpener(PopUp.PopUpType.NetworkLoad , set:false);        
            PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
        }

        public void ResetPasswordButton()
        {
            if(hassent) return;
            var request = new SendAccountRecoveryEmailRequest
            {
                Email = forgotEmailInput.text,
                TitleId = "55527"
            };
            hassent = true;
            PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
        }

        void OnRegisterSuccess(RegisterPlayFabUserResult request) {
            print("Registered and Logged in!");
            _regPanel.SetActive(false);
            _loginPanel.SetActive(true);
            
            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = regUserNameInput.text
            }, result => {}, OnError);
            
            NotificationManager.PopUpOpener(PopUp.PopUpType.Notification, "Registiration Succesfull!");

            account = new AccIAP
            {
                accLevel = 1,
                accCharacters = new List<string>{"base_character"},
                accWeapons = new List<string>{"basic_sword"},
                currentActor = "base_character",
                currentWeapon = "basic_sword"
            };

            UpdateAccountData();
        }
    
        void OnLoginSuccess(LoginResult result) {
            print("Login Success!");

            if (_remToggle.isOn)
            {
                PlayerPrefManager.Email(loginEmailInput.text);
                PlayerPrefManager.Pass(loginPasswordInput.text);
                PlayerPrefManager.Remember(1);
            }
            else
            {
                PlayerPrefManager.Email("");
                PlayerPrefManager.Pass("");
                PlayerPrefManager.Remember(0);
            }
            StartCoroutine(AccountDataCall());
        }

        IEnumerator AccountDataCall()
        {
            ImportAccountDataFromAPI();

            while (!loggedIn)
            {
                yield return null;
            }
            _loginPanel.SetActive(false);
            NotificationManager.PopUpOpener(PopUp.PopUpType.Notification, "Login Succesfull!", true);
            NetworkManager.ConnectToLobby();
            FindObjectOfType<SceneManagement>().LoadScene("Home",2);
        }

        void OnPasswordReset(SendAccountRecoveryEmailResult result) {
            print("Recovery Mail Has Been Sent");
            NotificationManager.PopUpOpener(PopUp.PopUpType.Notification, "Recovery Email Has Been Sent");
        }
    
        #endregion

    
        #region InAppPurchase
    
        // The Unity Purchasing system
        private static IStoreController m_StoreController;
    
        private void RefreshIAPItems()
        {
            PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), result =>
            {
                Catalog = result.Catalog;

                // Make UnityIAP initialize
                InitializePurchasing();
            }, OnError);
        }

        // This is invoked manually on Start to initialize UnityIAP
        private void InitializePurchasing() {
            // If IAP is already initialized, return gently
            if (IsInitialized) return;

            // Create a builder for IAP service
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.GooglePlay));

            // Register each item from the catalog
            foreach (var item in Catalog) {
                builder.AddProduct(item.ItemId, ProductType.Consumable);
            }

            // Trigger IAP service initialization
            UnityPurchasing.Initialize(this, builder);
        }

        // We are initialized when StoreController and Extensions are set and we are logged in
        private bool IsInitialized {
            get {
                return m_StoreController != null && Catalog != null;
            }
        }

        // This is automatically invoked automatically when IAP service is initialized
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            m_StoreController = controller;
        }

        // This is automatically invoked automatically when IAP service failed to initialized
        public void OnInitializeFailed(InitializationFailureReason error) {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        // This is automatically invoked automatically when purchase failed
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
            Debug.Log(
                $"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
        }

        // This is invoked automatically when successful purchase is ready to be processed
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
            // NOTE: this code does not account for purchases that were pending and are
            // delivered on application start.
            // Production code should account for such case:
            // More: https://docs.unity3d.com/ScriptReference/Purchasing.PurchaseProcessingResult.Pending.html

            if (!IsInitialized) {
                return PurchaseProcessingResult.Complete;
            }

            // Test edge case where product is unknown
            if (e.purchasedProduct == null) {
                Debug.LogWarning("Attempted to process purchase with unknown product. Ignoring");
                return PurchaseProcessingResult.Complete;
            }

            // Test edge case where purchase has no receipt
            if (string.IsNullOrEmpty(e.purchasedProduct.receipt)) {
                Debug.LogWarning("Attempted to process purchase with no receipt: ignoring");
                return PurchaseProcessingResult.Complete;
            }

            Debug.Log("Processing transaction: " + e.purchasedProduct.transactionID);

            // Deserialize receipt
            var googleReceipt = GooglePurchase.FromJson(e.purchasedProduct.receipt);

            // Invoke receipt validation
            // This will not only validate a receipt, but will also grant player corresponding items
            // only if receipt is valid.
            PlayFabClientAPI.ValidateGooglePlayPurchase(new ValidateGooglePlayPurchaseRequest() {
                    // Pass in currency code in ISO format
                    CurrencyCode = e.purchasedProduct.metadata.isoCurrencyCode,
                    // Convert and set Purchase price
                    PurchasePrice = (uint)(e.purchasedProduct.metadata.localizedPrice * 100),
                    // Pass in the receipt
                    ReceiptJson = googleReceipt.PayloadData.json,
                    // Pass in the signature
                    Signature = googleReceipt.PayloadData.signature
                }, result => Debug.Log("Validation successful!"),
                error => Debug.Log("Validation failed: " + error.GenerateErrorReport())
            );

            return PurchaseProcessingResult.Complete;
        }

        // This is invoked manually to initiate purchase
        void BuyProductID(string productId) {
            // If IAP service has not been initialized, fail hard
            if (!IsInitialized) throw new Exception("IAP Service is not initialized!");

            // Pass in the product id to initiate purchase
            m_StoreController.InitiatePurchase(productId);
        }

        #endregion

        #region TitleDataSection

        /// <summary>
        /// Useful for Game Tips or Hello Messages to Players.
        /// Can get an integerString value, parse to int and can use it like score multiplier for today YAY!
        /// </summary>
    
        private string messageText;
        void GetTitleData() {
            PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnTileDataReceived, OnError);
        }

        void OnTileDataReceived(GetTitleDataResult result)
        {
            if (result.Data == null || result.Data.ContainsKey("Message") == false) 
            {
                print("No Message");
                return;
            }

            messageText = result.Data["Message"];
        }

        #endregion
    
        #region DataTransferExamples

        /// <summary>
        /// Key-value pair data transfer, More useful with Json file. There is no need to create houndred key-value pairs.
        /// </summary>

        private string playerName;
        private string someData;
    
        public void SendData()
        {
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {"PlayerName", playerName},
                    {"SomeKey", someData}
                }
            };
            PlayFabClientAPI.UpdateUserData(request, OnDataSend,OnError);
        }

        void OnDataSend(UpdateUserDataResult result) {
            print("Succesful Data Send");
        }
        void OnDataReceived(GetUserDataResult result) {
            print("Received User Data");

            if (result.Data != null && result.Data.ContainsKey("PlayerName") && result.Data.ContainsKey("SomeKey"))
            {
                playerName = result.Data["PlayerName"].Value;
                someData = result.Data["SomeKey"].Value;
            }
            else
            {
                print("Player Data is NOT Complete");
            }
        }
        #endregion
    
        void OnError(PlayFabError error)
        {
            var er = error.GenerateErrorReport();

            NotificationManager.PopUpOpener(PopUp.PopUpType.Error,
                !er.IsNullOrEmpty() ? error.GenerateErrorReport() : error.ErrorMessage);
        }
    }


    #region IAP

// The following classes are used to deserialize JSON results provided by IAP Service
// Please, note that JSON fields are case-sensitive and should remain fields to support Unity Deserialization via JsonUtilities
    public class JsonData {
        // JSON Fields, ! Case-sensitive

        public string orderId;
        public string packageName;
        public string productId;
        public long purchaseTime;
        public int purchaseState;
        public string purchaseToken;
    }

    public class PayloadData {
        public JsonData JsonData;

        // JSON Fields, ! Case-sensitive
        public string signature;
        public string json;

        public static PayloadData FromJson(string json) {
            var payload = JsonUtility.FromJson<PayloadData>(json);
            payload.JsonData = JsonUtility.FromJson<JsonData>(payload.json);
            return payload;
        }
    }

    public class GooglePurchase {
        public PayloadData PayloadData;

        // JSON Fields, ! Case-sensitive
        public string Store;
        public string TransactionID;
        public string Payload;

        public static GooglePurchase FromJson(string json) {
            var purchase = JsonUtility.FromJson<GooglePurchase>(json);
            purchase.PayloadData = PayloadData.FromJson(purchase.Payload);
            return purchase;
        }
    }

    #endregion
}