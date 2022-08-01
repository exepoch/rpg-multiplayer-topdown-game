using Scriptable;
using TMPro;
using UnityEngine;

namespace Gui
{
    public class CharacterContentItem : MonoBehaviour
    {
        [SerializeField] private Actor _actor;
        [SerializeField] private GameObject lockImage;
        [SerializeField] private GameObject selectionFrame;
        [SerializeField] private GameObject lockOffParticle;
        [SerializeField] private TextMeshProUGUI priceText;
    
        [Tooltip("Don't manipulate this from editor!")][SerializeField] 
        private bool lockOn = true;

        private void Start()
        {
            priceText.text = _actor.price.ToString();
        }

        public void LockOff()
        {
            lockImage.SetActive(false);
            lockOn = false;
        
            if(lockOffParticle != null)
                lockOffParticle.SetActive(true);
        }

        public bool IsPurchased()
        {
            selectionFrame.SetActive(true);
            return !lockOn;
        }

        public Actor GetActor()
        {
            return _actor;
        }

        public void SelectionFrameOff()
        {
            selectionFrame.SetActive(false);
        }
    }
}
