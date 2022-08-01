using Cinemachine;
using Combat;
using Core;
using Gui;
using Managers;
using Network;
using Photon.Pun;
using Scriptable;
using UnityEngine;

namespace Control
{
   public class PlayerController : Character
   {
      #region MobileGui
   
      //Visual jostick images for rotation
      [Header("Mobile GUI Input")]
      [SerializeField] private Transform handle;
      [SerializeField] private GameObject leftUp;
      [SerializeField] private GameObject leftDown;
      [SerializeField] private GameObject rightUp;
      [SerializeField] private GameObject rightDown;
      #endregion
      
      
      private CinemachineVirtualCamera _followCam; //The camera must follow this player
      private Weapon _wep; //Character curren weapon
      private Vector3 dir; //Cahced direction vector for rotation
      private PhotonView _photonView;
      private Fighter _fighter;
      private Camera _camera;

      private int raycastMask;
      
      [SerializeField] private SkinnedMeshRenderer _renderer;
      [SerializeField] private GameObject _guiController;
      [SerializeField] private VariableJoystick _joystick;  //Must init at editor prefab
      
      private static readonly int Die = Animator.StringToHash("die");
      private static readonly int Defend = Animator.StringToHash("defend");

      private void Awake()
      {
         _health = GetComponent<Health>();
         _followCam = FindObjectOfType<CinemachineVirtualCamera>();
         _mover = GetComponent<Movement.Mover>();
         _fighter = GetComponent<Fighter>();
         _animator = GetComponent<Animator>();
         _photonView = GetComponent<PhotonView>();
         _camera = Camera.main;
         raycastMask = LayerMask.GetMask("RaycastPlane");

         _character = ActorManager.GetActor();
         _wep = InventoryManager.GetPlayerWeapon();
         _health.OnDie += OnDie;
         GameManager.OnBeforeDay += Moove;
      }
      
      
      private void Start()
      {
         if (_photonView.IsMine)
         {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
               _guiController.SetActive(true);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor ||
                     Application.platform == RuntimePlatform.WebGLPlayer)
            {
               _guiController.SetActive(false);
            }
            _photonView.RPC("Spawn", RpcTarget.AllBuffered, _character.key);
            _followCam.Follow = this.transform;
         }
      }

      private void Update()
      {
         if (!_photonView.IsMine || _health.IsDead()) return;

         AttackInput();  
         Movement();
         CharacterLookRotation();
      }

     

      //Spawns character visual ana weapon
      [PunRPC]
      public void Spawn(string key)
      {
         _character.Spawn(gameObject, _renderer);
         _fighter.EquipWeapon(_wep);
      }

      //When health is below zero, stop all functions of character and unregister
      void OnDie()
      {
         _animator.SetTrigger(Die);
         GameManager.OnBeforeDay -= Moove;
         if(!_photonView.IsMine) return;
         ActorManager.instance.RemoveFromFriendList(_fighter);
         NotificationManager.PopUpOpener(PopUp.PopUpType.Notification, "Next Time!", true);

         NetworkManager.Leave();
      }


      //Attack button OnClickMethod
      public void GuiAttackInput()
      {
         if(!_photonView.IsMine || _health.IsDead()) return;
         
         _fighter.PlayerAttackBehaviour();
      }

      //Defend button OnClickMethod
      public void GuiOnPointerDown()
      {
         if(!_photonView.IsMine || _health.IsDead()) return;
         _fighter.Defend(true);
         _animator.SetBool(Defend, true);
         Moove();
      }

      //Attack button Release button method
      public void GuiOnPointerUp()
      {
         if(!_photonView.IsMine || _health.IsDead()) return;
         _fighter.Defend(false);
         _animator.SetBool(Defend, false);
      }

      //Enables players navigation agent so player can move
      private void Moove()
      {
         _mover.MoveAbleAgain();
      }

#if UNITY_EDITOR || UNITY_WEBGL
   
   private Vector3 mousePos;
   private RaycastHit hit;
   private float h, v;

   private void AttackInput()
   {
      if (Input.GetMouseButtonUp(0))
      {
         _fighter.PlayerAttackBehaviour();
      }

      if (Input.GetMouseButtonDown(1))
      {
         _fighter.Defend(true);
         _animator.SetBool(Defend, true);
         Moove();
      }

      if (Input.GetMouseButtonUp(1))
      {
         _fighter.Defend(false);
         _animator.SetBool(Defend, false);
      }
   }

   private void Movement()
   {
      h = Input.GetAxisRaw("Horizontal");
      v = Input.GetAxisRaw("Vertical");

      dir = new Vector3(h, 0, v);
      _animator.SetFloat(Speed, dir.magnitude);

      if(dir.magnitude == 0 ) return;
      _mover.StartMoveAction(dir, _character.MoveSpeed);
   }

   private void CharacterLookRotation()
   {
      //If character aiming with ranged weapon, do not get the direction input from user and outo lock to enemy
      if (_fighter.IsAimed() && _fighter.Enemy() != null)
      {
         transform.LookAt(_fighter.Enemy().transform.position);
         return;
      }
      
      Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out hit, Mathf.Infinity ,raycastMask))
      {
         mousePos = hit.point;

         Vector3 rot = mousePos - transform.position;
         float angle = Mathf.Atan2(rot.x, rot.z) * Mathf.Rad2Deg;
         transform.rotation = Quaternion.Euler(0, angle, 0);
      }
   }
#elif UNITY_ANDROID || UNITY_IOS
   private void AttackInput()
   { }

   private void Movement()
   {
      dir = Vector3.forward * _joystick.Vertical + Vector3.right * _joystick.Horizontal;
         
      _animator.SetFloat(Speed, dir.magnitude == 0 ? 0 : 1);

      //Starts the movement request
      _mover.StartMoveAction(dir, _character.MoveSpeed);

      //Sets the rotation visual objects
      if (handle.localPosition == Vector3.zero)
      {
         leftDown.SetActive(false);
         leftUp.SetActive(false);
         rightDown.SetActive(false);
         rightUp.SetActive(false);
         return;
      }
         
      if (handle.localPosition.x <= 0)
      {
         if (handle.localPosition.y <= 0)
         {
            leftDown.SetActive(true);
            leftUp.SetActive(false);
            rightDown.SetActive(false);
            rightUp.SetActive(false);
         }
         else
         {
            leftDown.SetActive(false);
            leftUp.SetActive(true);
            rightDown.SetActive(false);
            rightUp.SetActive(false);
         }
      }
      else
      {
         if (handle.localPosition.y <= 0)
         {
            leftDown.SetActive(false);
            leftUp.SetActive(false);
            rightDown.SetActive(true);
            rightUp.SetActive(false);
         }
         else
         {
            leftDown.SetActive(false);
            leftUp.SetActive(false);
            rightDown.SetActive(false);
            rightUp.SetActive(true);
         }
      }
   }
   
   private void CharacterLookRotation()
   {
      //If character aiming with ranged weapon, do not get the direction input from user and outo lock to enemy
      if (_fighter.IsAimed() && _fighter.Enemy() != null)
      {
         transform.LookAt(_fighter.Enemy().transform.position);
         return;
      }

      float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

      if(angle != 0)
         transform.rotation = Quaternion.Euler(0,angle,0);
   }
#endif
   }
}
