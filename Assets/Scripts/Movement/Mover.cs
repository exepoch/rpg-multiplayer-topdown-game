using Core;
using UnityEngine;
using UnityEngine.AI;

namespace Movement
{
    [RequireComponent(typeof(ActionScheduler))]
    public class Mover : MonoBehaviour,IAction
    {
        [SerializeField] private AudioClip footStep;
        
        private AudioSource _source;
        private NavMeshAgent _navMeshAgent;

        private void Awake() 
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _source = GetComponent<AudioSource>();
        }

        public void StartMoveAction(Vector3 dir, float speedFraction)
        {
            if(_navMeshAgent.isStopped) return;
            GetComponent<ActionScheduler>().StartAction(this);
            _navMeshAgent.Move(dir.normalized * speedFraction * Time.deltaTime);
        }

        public void SetDestinationAction(Vector3 pos)
        {
            //_navMeshAgent.enabled = true;
            _navMeshAgent.isStopped = false;
            GetComponent<ActionScheduler>().StartAction(this);
            _navMeshAgent.SetDestination(pos);
        }

        //If character must stop while attacking, comment the line that setoff the navmeshagent
        public void Cancel()
        {
            _navMeshAgent.isStopped = true;
        }

        //Attack Animator Eventda
        public void MoveAbleAgain()
        {
            GetComponent<ActionScheduler>().StartAction(this);
            _navMeshAgent.isStopped = false;
        }

        private void FootS()
        {
            _source.volume = .5f;
            _source.PlayOneShot(footStep);
        }
    }
}
