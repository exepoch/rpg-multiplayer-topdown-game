using Photon.Pun;
using UnityEngine;

namespace Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 1;
        [SerializeField] private bool isHoming = true;
        [SerializeField] private float maxLifeTime = 10;

        private Vector3 targetPos;
        private float damage;
        private string _tag;

        private void OnEnable()
        {
            transform.LookAt(GetAimLocation());
        }

        void Update()
        {
            if (targetPos == Vector3.zero) return;
            if (isHoming)
            {
                transform.LookAt(GetAimLocation());
            }
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        //Sets the targer rotation
        public void SetProjectileTarget(Vector3 target, float damage, string tag)
        {
            this.targetPos = target;
            this.damage = damage;
            _tag = tag;

            Destroy(gameObject, maxLifeTime);
        }

        private Vector3 GetAimLocation()
        {
            return targetPos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag(_tag) || other.GetComponent<Fighter>() == null) return;
            other.GetComponent<PhotonView>().RPC("GetHit", RpcTarget.AllBuffered, damage);
            transform.parent = other.transform;
            GetComponent<BoxCollider>().enabled = false;
            speed = 0;

            PhotonNetwork.Destroy(gameObject);
        }
    }
}