using UnityEngine;

namespace Core
{
    public class ActionScheduler : MonoBehaviour
    { 
        private IAction action;

        public void StartAction(IAction newAction)
        {
            if(action == newAction) return;

            if (action != null)
            {
                action.Cancel();
            }
            
            this.action = newAction;
        }
    }
}
