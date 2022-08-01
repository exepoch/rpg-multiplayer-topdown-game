using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class SettingManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _music;
        
        private AudioListener _audioListener;

        private void Awake()
        {
            _audioListener = Camera.main.GetComponent<AudioListener>();
        }

        public void SetFxLevel(Slider slider)
        {
            
        }

        public void SetMusicLevel(Slider slider)
        {
            
        }
    }
}
