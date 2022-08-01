using System;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class Health : MonoBehaviour
    {
        [SerializeField] private float _health;
        public Action OnDie;

        bool isDead = false;

        private void Awake()
        {
            _health = GetComponent<Character>().ScriptableCha.Health;
        }

        public bool IsDead()
        {
            return isDead;
        }
    
        public void GetHit(float damage)
        {
            print("Damaged to :" + gameObject.name);
            _health -= damage;

            if (!(_health <= 0)) return;
            
            Die();
            _health = 0;
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            OnDie?.Invoke();
        }


        public float Current()
        {
            return _health;
        }

        public void FullFillHealth()
        {
            _health = GetComponent<Character>().ScriptableCha.Health;
        }
    }
}

