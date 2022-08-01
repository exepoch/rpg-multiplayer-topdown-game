using Photon.Pun;
using UnityEngine;

namespace Combat
{
    public class CloseRangeWeapon : IWeaponType
    {
        [SerializeField] private GameObject trail;
        [SerializeField] private bool isRightHanded = true;
        [SerializeField] private AudioClip _swingSound;
        private AudioSource _source;

        private Fighter hitObject;
        private float damage;
        private Collider _collider;
        private string _tag;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _source = GetComponent<AudioSource>();
            _collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            hitObject = other.GetComponent<Fighter>();
            
            //self or friend players, non damagable object cant pass
            if(hitObject == null || other.CompareTag(_tag)) return; 
        
            Launch(false); //Disable if sword can attack multiplce targets
            hitObject.GetComponent<PhotonView>().RPC("GetHit", RpcTarget.AllBuffered, damage);
        }

        public override void SetProperty(float value, string tagg, bool isRightH, string url)
        {
            this.damage = value;
            _tag = tagg;
            isRightHanded = isRightH;
        }

        public override void Launch(bool set)
        {
            _collider.enabled = set;
            trail.SetActive(set);

            if (set)
            {
                _source.PlayOneShot(_swingSound);
            }
        }
    }
}
