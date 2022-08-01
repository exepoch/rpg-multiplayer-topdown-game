using Combat;
using Core;
using Managers;
using Photon.Pun;
using Scriptable;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

namespace Control
{
    public class AIController : Character
    {
        [SerializeField] private SkinnedMeshRenderer _renderer;
        [SerializeField] private LookAtConstraint _constraint; //Player healthbar sprite looakat object referance
        [SerializeField] private GameObject _selectedVisual;
        private Transform _transform;
        private PhotonView _pw;
        private Weapon _weapon;
        private Fighter _fighter;
        private NavMeshAgent nav;

        private Vector3 holdPos; //Allied AI last pos just before night to turn basck this position after fight if still alive
        private bool enemyInRange; //Is enemy in attack range

        private static readonly int Die = Animator.StringToHash("die");


        private void Awake()
        {
            _health = GetComponent<Health>();
            _animator = GetComponent<Animator>();
            _fighter = GetComponent<Fighter>();
            _mover = GetComponent<Movement.Mover>();
            _transform = GetComponent<Transform>();
            nav = GetComponent<NavMeshAgent>();
            _pw = GetComponent<PhotonView>();
            
            //Prepare healthbar lookat constraint
            if (Camera.main != null)
            {
                ConstraintSource camS = new ConstraintSource
                {
                    sourceTransform = Camera.main.transform,
                    weight = 1
                };
                _constraint.SetSource(0, camS);
            }

            nav.speed = _character.MoveSpeed;

            _health.OnDie += OnDie;
        }

        private void Start()
        {
            var type = _fighter.Get(); 
            if (type == Fighter.FighterType.EnemyAI)
            {
                _character = ActorManager.GetEnemy();
            }
            else if (type == Fighter.FighterType.Allied)
            {
                _character = ActorManager.GetAllied();
                GameManager.RegisterAllie();
                GameManager.OnBeforeDay += BeforeDay;
                GameManager.OnBeforeNight += BeforeNight;
            }

            _character.Spawn(gameObject,_renderer);
            _weapon = InventoryManager.GetWeaponWithKey(_character.aiWeaponType);
            _fighter.EquipWeapon(_weapon);
        }


        private void Update()
        {
            if (_health.IsDead() || !_pw.IsMine) return;

            AILookRotation();
            //Check this fighter can attack his enemy
            enemyInRange = _fighter.CanAttack();
            if (enemyInRange)
            {
                _mover.Cancel(); //Stop going to destination
                _animator.SetFloat(Speed,0);
                AIAttackBehaviour();
                
                return;
            }
            
            PlaceModArranger();
            
            if(enemyInRange) return;
            Movement();
        }

        private void PlaceModArranger()
        {
            if (AIPlaceManager.Mod() && nav.remainingDistance >= 0.01)
            {
                print("PlacerModActived");
                _animator.SetFloat(Speed, 1);
            }
            else if (!AIPlaceManager.Mod() && nav.remainingDistance >= 0.01)
            {
                _animator.SetFloat(Speed, 1);
            }
            else
            {
                _animator.SetFloat(Speed, 0);
            }
        }

        private void OnDestroy()
        {
            _health.OnDie -= OnDie;
            GameManager.OnBeforeDay -= BeforeDay;
            GameManager.OnBeforeNight -= BeforeNight;
        }
        
        
        private void BeforeNight()
        {
            holdPos = transform.position;
        }

        private void BeforeDay()
        {
            if(_health.IsDead()) return;
            print("Onbeforeday");
            nav.isStopped = false;
            _mover.SetDestinationAction(holdPos);
        }


        private void OnDie()
        {
            _animator.SetTrigger(Die);
            _fighter.enabled = false;
            
            var type = _fighter.Get(); 
            if (type == Fighter.FighterType.EnemyAI)
            {
                ActorManager.instance.RemoveFromFoeList(_fighter);
                _fighter.GetKiller().RPC("KillConfirmed", RpcTarget.All, _character.experiance);
            }
            else if (type == Fighter.FighterType.Allied)
            {
                ActorManager.instance.RemoveFromFriendList(_fighter);
                GameManager.UnRegisterAllie();
            }
            
            Invoke(nameof(Destroy),4);
        }

        void Destroy()
        {
            if(_pw.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }

        private void OnMouseUp()
        {
            if(!PhotonNetwork.IsMasterClient || !AIPlaceManager.Mod()) return;
            
            AIPlaceManager.SetControlled(this);
        }

        public void SelectedVisualOn(bool set)
        {
            _selectedVisual.SetActive(set);
        }

        public void MoveCommandByPlayer(Vector3 pos)
        {
            _mover.SetDestinationAction(pos);
        }

        //Start attack
        private void AIAttackBehaviour()
        {
            _fighter.PlayerAttackBehaviour();
        }

        //Move towards enemy if its not in weapon range
        private void Movement()
        {
            if(_fighter.Enemy() == null) return;
            nav.isStopped = false;
            
            
            //_mover.StartMoveAction(dir, _character.MoveSpeed);
            _mover.SetDestinationAction(_fighter.Enemy().transform.position);
            _animator.SetFloat(Speed, 1);
        }

        private void AILookRotation()
        {
            if(_fighter.Enemy() == null) return;
            
            var dir = _fighter.Enemy().transform.position - _transform.position;
            var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
}
