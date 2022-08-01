using Control;
using Photon.Pun;
using UnityEngine;

namespace Managers
{
    public class AIPlaceManager : MonoBehaviour
    {
        private static AIController _controlledAI;
        private Camera _camera;
        private static bool placeMod;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Mouse0) && PhotonNetwork.IsMasterClient)
            {
                AIPlaceController();  
            }
        }

        public static void SetMod(bool set)
        {
            placeMod = set;
            
            if(_controlledAI == null) return;
            
            _controlledAI.SelectedVisualOn(set);

            if (!set)
                _controlledAI = null;
        }
        
        public static bool Mod()
        {
            return placeMod;
        }
        
        public static void SetControlled(AIController controller)
        {
            if(_controlledAI != null)
                _controlledAI.SelectedVisualOn(false);
            
            _controlledAI = controller;
            _controlledAI.SelectedVisualOn(true);
        }

        public static AIController GetControlledAI()
        {
            return _controlledAI;
        }

        private static void SendCommand(Vector3 pos)
        {
            if (placeMod && _controlledAI != null)
            {
                _controlledAI.MoveCommandByPlayer(pos);
            }
        }
        
        private void AIPlaceController()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                AIPlaceManager.SendCommand(hit.point);
            }
        }
    }
}
