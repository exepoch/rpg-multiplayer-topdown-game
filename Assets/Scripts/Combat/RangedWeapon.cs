using Photon.Pun;
using UnityEngine;

namespace Combat
{
    public class RangedWeapon : IWeaponType
    {
        [SerializeField] private AudioClip _shotSound;
        private AudioSource _source;
        private string projectileUrl = "Prefabs/Other/ArrowProjectile";
        private float _damage;
        private string _tag;
        private bool isRightHanded;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        public override void SetProperty(float damage, string tagg, bool hand, string url)
        {
            _damage = damage;
            _tag = tagg;
            isRightHanded = hand;
            projectileUrl = url;
        }

        public override void Launch(Vector3 target, Vector3 leftH, Vector3 rightH)
        {
            _source.PlayOneShot(_shotSound);
            Projectile projectileInstance = PhotonNetwork.Instantiate(projectileUrl, GetTransform(leftH,rightH),
                Quaternion.identity).GetComponent<Projectile>();
            
            projectileInstance.SetProjectileTarget(target,_damage,_tag);
        }
        
        private Vector3 GetTransform(Vector3 rightHand, Vector3 leftHand)
        {
            Vector3 handTransform = isRightHanded ? rightHand : leftHand;
            return handTransform;
        }
    }
}
