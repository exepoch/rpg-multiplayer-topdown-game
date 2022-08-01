using System.Collections.Generic;
using Gui;
using UnityEngine;

namespace Managers
{
    public class NotificationManager : MonoBehaviour
    {
        [SerializeField] private List<PopUp> popUpList;
        private static Dictionary<PopUp.PopUpType, GameObject> popUps;    
        private static PopUp _currentPopUp;

        private void Awake()
        {
            popUps = new Dictionary<PopUp.PopUpType, GameObject>();
        
            foreach (PopUp o in popUpList)
            {
                popUps.Add(o.GetType(), o.gameObject);
            }
        }

        /// <summary>
        /// Opens the popUp
        /// </summary>
        /// <param name="popType">Can be error or normal PopUpscreen</param>
        /// <param name="message">The message to be shown</param>
        /// <param name="selfDes"></param>
        /// <param name="desTime"></param>
        /// <param name="set"></param>
        public static void PopUpOpener(PopUp.PopUpType popType,string message = "", bool selfDes = false, float desTime=2,bool set = true)
        {
            if(_currentPopUp!=null)
                CloseCurrentPopUp();

            var _rect = FindObjectOfType<SceneManagement>().transform.GetChild(0).transform;
            _currentPopUp = Instantiate(popUps[popType], _rect).GetComponent<PopUp>();
            _currentPopUp.GetComponent<PopUp>();
        
            if(set)
                _currentPopUp.SetPop(message, desTime, selfDes);
        
            _currentPopUp.gameObject.SetActive(true);
        }

        private static void CloseCurrentPopUp()
        {
            if(_currentPopUp !=null)
                _currentPopUp.ClosePopWithButton();
        }
    }
}
