using Network;
using UnityEngine;

namespace Managers
{
    public class TutorialSceneManager : MonoBehaviour
    {
        private NetworkManager nM;
    
        private void Awake()
        {
            PlayerPrefManager.Tutorial(1);
            nM = FindObjectOfType<NetworkManager>();
        }

        private void Start()
        {
            if(nM != null)
                nM.SetupRoom();
        }
    }
}
