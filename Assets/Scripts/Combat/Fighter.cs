using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Gui;
using Managers;
using Network;
using Photon.Pun;
using Scriptable;
using UnityEngine;
using UnityEngine.UI;

namespace Combat
{
    [RequireComponent(typeof(ActionScheduler))]
    public class Fighter : MonoBehaviour,IAction
    {
        public enum FighterType
        {
            Player,
            EnemyAI,
            Npc,
            Allied
        }

        [SerializeField] private GameObject hitEffect; //Particle has own deactivate
        [SerializeField] private AudioClip _getDamageSound; //The sound when attacked and damaged
        [SerializeField] private AudioClip _getHitSound; //The sound when attacked but defended
        [SerializeField] private AudioClip _sheatheSound; //The sound when weapon enabling
        [SerializeField] private AudioClip _unsheatheSound; //The sound when weapon disabling
        [SerializeField] private AudioClip _swordSwingSound; //The sound when sword started attack
        
        [SerializeField] private FighterType fighterType;
        [SerializeField] private GameObject aimLockPrefab;
        [SerializeField] private Transform rightHandTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Image healthBar;

        [SerializeField] private List<Fighter> _enemyList;  //Temporary attack able characters list
        
        [SerializeField] private Fighter _currentEnemy;
        [SerializeField] private PhotonView _lastAttacker; //Last attacked fighter to this fighter.

        private Weapon _weaponType;
        private Health _health;
        private Transform _transform;
        private GameObject _aimVisual;
        private Animator _animator;
        private IWeaponType _handledWeapon;
        private AudioSource _source;

        [SerializeField] private float timeBetweenAttacks;
        private float _timeSinceLastAttack = Mathf.Infinity;
        private float _distanceToOpponent = Mathf.Infinity;
        private float _dist;
        private bool _enemyChanged;
        private float _maxHealth;
        private bool _isDefending;
        private bool _isAimed;
        private bool _canFight;
        private string _fighterID;


        private static readonly int Attack = Animator.StringToHash("attack");
        private static readonly int Gethit = Animator.StringToHash("gethit");
        private static readonly int Unsheathe = Animator.StringToHash("unsheathe");
        private static readonly int Sheathe = Animator.StringToHash("sheathe");

        private void Awake()
        {
            _health = GetComponent<Health>();
            _animator = GetComponent<Animator>();
            _source = GetComponent<AudioSource>();
            _maxHealth = GetComponent<Character>().ScriptableCha.Health;
            _enemyList = new List<Fighter>();
            _transform = transform;
            _fighterID = Guid.NewGuid().ToString();

            //If we want the animation attack speed is sequenced with the attack animation lengt
            if (fighterType == FighterType.Player)
            {
                SequenceAttackSpeedWithAttackAnimation();   
            }
        }

        private void Start()
        {
            RegisterActorManager();
        }

        private void SequenceAttackSpeedWithAttackAnimation()
        {
            var time = _animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip animationClip in time)
            {
                if (animationClip.name == "Attack")
                {
                    timeBetweenAttacks = animationClip.length;
                }
            }
        }

        private void Update()
        {
            _timeSinceLastAttack += Time.deltaTime;
            
            if (fighterType == FighterType.Player && _weaponType.isRanged)
            {
                _isAimed = true;
                RangedAimVisualisation();   
            }
            else
            {
                _isAimed = false;
            }
        }

        public FighterType Get()
        {
            return fighterType;
        }
        
        public bool IsAimed()
        {
            return _isAimed;
        }

        //Register this fighter
        private void RegisterActorManager()
        {
            switch (this.fighterType)
            {
                case FighterType.Player:
                    ActorManager.onFoeJoined += UpdateFoeList;
                    GameManager.OnBeforeDay += DisableFighting;
                    GameManager.OnBeforeNight += EnableFighting;
                    ActorManager.instance.AddToFriendList(this);
                    _enemyList = ActorManager.instance.GetAIEnemyList();
                    break;
                case FighterType.Allied:
                    ActorManager.onFoeJoined += UpdateFoeList;
                    GameManager.OnBeforeDay += DisableFighting;
                    GameManager.OnBeforeNight += EnableFighting;
                    ActorManager.instance.AddToFriendList(this);
                    _enemyList = ActorManager.instance.GetAIEnemyList();
                    break;
                case FighterType.EnemyAI:
                    ActorManager.instance.AddToFoeList(this);
                    _enemyList = ActorManager.instance.GetPlayerList();
                    ActorManager.onPlayerJoined += UpdatePlayerList;
                    _canFight = true;
                    break;
                case FighterType.Npc:
                    ActorManager.instance.AddToNPCList(this);
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (this.fighterType)
            {
                case FighterType.Player:
                    ActorManager.onFoeJoined -= UpdateFoeList;
                    GameManager.OnBeforeDay -= DisableFighting;
                    GameManager.OnBeforeNight -= EnableFighting;
                    break;
                case FighterType.Allied:
                    ActorManager.onFoeJoined -= UpdateFoeList;
                    GameManager.OnBeforeDay -= DisableFighting;
                    GameManager.OnBeforeNight -= EnableFighting;
                    break;
                case FighterType.EnemyAI:
                    break;
                case FighterType.Npc:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EnableFighting()
        {
            StartCoroutine(Delayer(() => { _canFight = true; },2 ));
            _animator.SetTrigger(Unsheathe);
            _source.PlayOneShot(_unsheatheSound);
            _handledWeapon.gameObject.SetActive(true);
        }
        
        private void DisableFighting()
        {
            _canFight = false;
            _animator.SetTrigger(Sheathe);
            _source.PlayOneShot(_sheatheSound);
            _handledWeapon.gameObject.SetActive(false);
        }

        public Fighter Enemy()
        {
            return _currentEnemy;
        }
        public void Defend(bool input)
        {
            _isDefending = input;
            _handledWeapon.Launch(false);
        }

        private void RangedAimVisualisation()
        {
            if(!CanAttack() || !_enemyChanged) return;

            _enemyChanged = false;
            if(_aimVisual != null)
                Destroy(_aimVisual);

            _aimVisual = Instantiate(aimLockPrefab, _currentEnemy.transform);
            print("AimVisualCreated");
        }

        public void EquipWeapon(Weapon weapon)
        {
            _weaponType = weapon;
            _handledWeapon = _weaponType.Spawn(rightHandTransform, leftHandTransform, _animator, tag);
            
            print("Time : " + GameManager.Time());
            if(GameManager.Time())
                _handledWeapon.gameObject.SetActive(false);
        }

        #region PunCallBacks

        [PunRPC]
        public void RPCEquip(string key)
        {
            Weapon wep = InventoryManager.GetWeaponWithKey(key);
            EquipWeapon(wep);
        }

        [PunRPC]
        public void GetHit(float damage, PhotonMessageInfo messageInfo)
        {
            if (_isDefending)
            {
                _animator.SetTrigger(Gethit);
                _source.PlayOneShot(_getHitSound);
                return;
            }
            _health.GetHit(damage);
            _source.PlayOneShot(_getDamageSound);
            hitEffect.SetActive(true);
            _lastAttacker = messageInfo.photonView;//Just uses by AIController
            
            if (fighterType == FighterType.Player && GetComponent<PhotonView>().IsMine)
            {
                GUIManager.instance.PlayerInfoGUI(GetComponent<Health>().Current());
            }
            
            //not null only AI
            if(healthBar != null)
                healthBar.DOFillAmount(Mathf.Clamp01(_health.Current() / _maxHealth), .1f);
        }
        
        [PunRPC]
        public void LaunchRanged()
        {
            var targetCapsule = _currentEnemy.GetComponent<CapsuleCollider>();
            var target = _currentEnemy.transform.position + Vector3.up * targetCapsule.height / 2;
            _handledWeapon.Launch(target, leftHandTransform.position, rightHandTransform.position);
        }

        [PunRPC]
        public void LaunchCloseRange(bool set)
        {
            _handledWeapon.Launch(set);
        }

        [PunRPC]
        void KillConfirmed(float val)
        {
            AccountManager.instance.GrantExperianceToAccount(val);
            AccountManager.instance.UpdatePlayerStatistics("EnemyKilled",1);
        }
            
        #endregion


        public void PlayerAttackBehaviour()
        {
            if(!_canFight) return;
            if (!(_timeSinceLastAttack > timeBetweenAttacks)) return;

            GetComponent<ActionScheduler>().StartAction(this);
            if (_weaponType.isRanged && _currentEnemy != null)
            {
                transform.DOLookAt(_currentEnemy.transform.position, .3f);
            }
            
            _animator.SetTrigger(Attack);

            _timeSinceLastAttack = 0;
        }

        #region AnimatorEvents

        //Enables weapon collider
        public void WeaponEnable()
        {
            GetComponent<PhotonView>().RPC("LaunchCloseRange", RpcTarget.AllBuffered, true);
            _source.PlayOneShot(_swordSwingSound);
        }

        //Spawns projectiles from weapon
        public void Shoot()
        {  
            if(_currentEnemy == null) { return; }
            
            if (_weaponType.HasProjectile())
            {
                GetComponent<PhotonView>().RPC("LaunchRanged", RpcTarget.AllBuffered);
            }
        }

        #endregion
        
        public bool CanAttack()
        {
            _distanceToOpponent = Mathf.Infinity;
            if (_enemyList.Count == 0)
            {
                _currentEnemy = null;
            }

            foreach (var enemy in _enemyList)
            {
                _dist = Vector3.Distance(enemy.transform.position, _transform.position);

                if (_dist < _distanceToOpponent)
                {
                    _distanceToOpponent = _dist;
                    if (_currentEnemy != null)
                    {
                        if (_currentEnemy.FighterID() != enemy.FighterID())
                        {
                            print(gameObject.name + "EnemyChanged");
                            _enemyChanged = true;
                        }   
                    }
                    _currentEnemy = enemy;
                }
            }

            return _distanceToOpponent < _weaponType.GetRange();
        }

        private void UpdatePlayerList()
        {
            _enemyList = ActorManager.instance.GetPlayerList();
        }
    
        private void UpdateFoeList()
        {
            _enemyList = ActorManager.instance.GetAIEnemyList();
        }

        public void Cancel()
        {
            _handledWeapon.Launch(false);
        }

        private string FighterID()
        {
            return _fighterID;
        }

        public PhotonView GetKiller()
        {
            return _lastAttacker;
        }

        IEnumerator Delayer( Action act, float delay)
        {
            yield return new WaitForSeconds(delay);
            act.Invoke();
        }
    }
}
