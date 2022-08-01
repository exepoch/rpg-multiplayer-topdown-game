using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gui
{
    public class PopUp : MonoBehaviour
    {
        public enum PopUpType
        {
            Error,
            Notification,
            NetworkLoad,
            ConnectionWarning
        }
    
        [SerializeField] private TextMeshProUGUI _popText;
        [SerializeField] private Image _dimmPanel;
        [SerializeField] private GameObject _closeButton;

        [SerializeField] private PopUpType type;

        public new PopUpType GetType() { return type; }
    
        private float closeDelay;
        private bool selfDest = false;

        public void SetPop(string message,float desTime, bool selfDes = false, bool set = false)
        {
            _popText.text = message;
            closeDelay = desTime;
            this.selfDest = selfDes;
        }

        private void OnEnable()
        {
            _dimmPanel.DOFade(1, 1f);

            if (selfDest)
            {
                if(_closeButton)
                    _closeButton.SetActive(false);
                Invoke(nameof(ClosePopWithButton), closeDelay);
            }
            else
            {
                if(_closeButton)
                    _closeButton.SetActive(true);
            }
            
        }

        public void ClosePopWithButton()
        {
            _dimmPanel.DOFade(0, 1).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}
