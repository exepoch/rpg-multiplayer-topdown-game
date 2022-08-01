using System;
using System.Collections.Generic;
using Combat;
using Core;
using Managers;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Control
{
    public class NpcController : Character
    {
        private List<Transform> points; //Patrol position points
        private PhotonView _pw;
        private NavMeshAgent nA;
        private Fighter _fighter;
        

        [SerializeField] private SkinnedMeshRenderer _renderer;
        [SerializeField] private bool hasJob; //if true, npc wont move anywhere, it stands with his table
        private int destPoint;
        private bool isGoingHome;
        
        private static readonly int Die = Animator.StringToHash("die");
        private static readonly int Job = Animator.StringToHash("job");

        private void Awake()
        {
            _health = GetComponent<Health>();
            _animator = GetComponent<Animator>();
            _mover = GetComponent<Movement.Mover>();
            _pw = GetComponent<PhotonView>();
            _fighter = GetComponent<Fighter>();
            nA = GetComponent<NavMeshAgent>();
            //nA.autoBraking = true;

            _health.OnDie += OnDie;

            GameManager.OnBeforeNight += BeforeNight;
            points = FindObjectOfType<ActorManager>().GetPosList();

            if (hasJob)
            {
                _animator.SetBool(Job,true);
                return;
            }
            
            _character = ActorManager.GetRandomNPC();
            _character.Spawn(gameObject,_renderer);
        }

        //Unregister ondie for prevent NullRefereance excp.
        public void UnRegister()
        {
            _health.OnDie -= OnDie;
            GameManager.OnBeforeNight -= BeforeNight;
        }

        private void Start()
        {
            Movement();
        }

        private void Update()
        {
            if (_health.IsDead() || !_pw.IsMine || isGoingHome) return;

            if (!nA.pathPending && nA.remainingDistance < 0.5f)
                Movement();
        }
        
        void OnDie()
        {
            _animator.SetTrigger(Die);
            
            ActorManager.instance.RemoveFromNPCList(_fighter);
        }

        //Call by got in a house
        private void OnDestroy()
        {
            ActorManager.instance.RemoveFromNPCList(_fighter);
        }

        private void Movement()
        {
            //Job owner npcs doesnt patrol around
            if(points.Count == 0 || hasJob) return;

            destPoint = Random.Range(0, points.Count);
            _mover.SetDestinationAction(points[destPoint].position);
            _animator.SetFloat(Speed, 1);
        }

        void BeforeNight()
        {
            MoveClosestHome();
        }

        private void MoveClosestHome()
        {
            var goal = GetClosestPoint();
            isGoingHome = true;
            nA.speed = nA.speed * 2;
            _animator.SetBool(Job,false);
            _animator.SetFloat(Speed, 1);
            _mover.SetDestinationAction(goal);
        }
        
        Vector3 GetClosestPoint()
        {
            float dis = Mathf.Infinity;
            Vector3 homepos = new Vector3();
            var temp = GameManager.instance.GetHomePositionList();

            foreach (DestroyOnTouch pos in temp)
            {
                /*if (GetComponent<Transform>() == null)
                {
                    print("How can this be NULL?");
                    return Vector3.zero;
                }*/
                if (!pos.Enabled()) continue;
                
                var d = Vector3.Distance(transform.position, pos.transform.position);
                if (d < dis)
                {
                    dis = d;
                    homepos = pos.transform.position;
                }
            }

            return homepos;
        }
    }
}
