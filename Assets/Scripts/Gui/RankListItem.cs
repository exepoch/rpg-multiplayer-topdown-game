using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gui
{
    public class RankListItem : MonoBehaviour
    {
        [SerializeField] private Image _flag;
        [SerializeField] private TextMeshProUGUI _username;
        [SerializeField] private TextMeshProUGUI _score;

        public void SetItemCred(Sprite flag, string username, string score)
        {
            _flag.sprite = flag;
            _username.text = username;
            _score.text = score;
        }
    }
}
