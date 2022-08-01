using System.Collections.Generic;
using System.Linq;
using Network;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gui
{
    public class RankPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _username; //Self uname
        [SerializeField] private TextMeshProUGUI _score; //self score
        [SerializeField] private TextMeshProUGUI _rank; //Self uname
        [SerializeField] private Button _updateButton;
        [SerializeField] private Image _flag;  //self flag
        
        [SerializeField] private List<RankListItem> _rankListItems;
        [SerializeField] private List<Sprite> _flagImageList;
        [SerializeField] private List<string> _flagKeyList;
        [SerializeField] private List<PlayerLeaderboardEntry> _rankList;
        private Dictionary<string, Sprite> _flags = new Dictionary<string, Sprite>();

        /// <summary>
        /// False if "EnemyKilled" leaderboard active
        /// True if "SurvivedNight" leaderboard active
        /// </summary>
        private bool activePanel;

        private void Awake()
        {
            for (int i = 0; i < _flagKeyList.Count; i++)
            {
                _flags.Add(_flagKeyList[i], _flagImageList[i]);
            }

            _rankList = new List<PlayerLeaderboardEntry>();
            _username.text = AccountManager.instance.GetAccountUserName();
            
            GetEnemyKilledList();
        }

        #region ButtonEvent

        public void GetNightSurvivedList()
        {
            _rankList = AccountManager.instance.GetSurvivedDayRanking();
            ListRanking();
            activePanel = false;
        }

        public void GetEnemyKilledList()
        {
            _rankList = AccountManager.instance.GetEnemyKilledRanking();
            ListRanking();
            activePanel = true;
        }

        public void RequestUpdate()
        {
            AccountManager.instance.UpdateLeaderboards();
            _updateButton.interactable = false;
        }

        #endregion


        void ListRanking()
        {
            int max = _rankList.Count <= 10 ? _rankList.Count : 10;
            
            for (int i = 0; i < max; i++)
            {
                _rankListItems[i].SetItemCred(_flagImageList[0], _rankList[i].DisplayName, _rankList[i].StatValue.ToString());
            }

            foreach (var entry in _rankList.Where(entry => entry.DisplayName == AccountManager.instance.GetAccountUserName()))
            {
                _score.text = entry.StatValue.ToString();
                _rank.text = (entry.Position + 1).ToString();
            }
        }
        
        public void UpdateLeaderBoard()
        {
            if (activePanel)
            {
                GetEnemyKilledList();
            }
            else
            {
                GetNightSurvivedList();
            }

            _updateButton.interactable = true;
        }

    }
}
