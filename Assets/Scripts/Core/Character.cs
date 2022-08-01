using Combat;
using Scriptable;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(Animator)),
     RequireComponent(typeof(Movement.Mover)),
     RequireComponent(typeof(Health))]
    public class Character : MonoBehaviour
    {
        [SerializeField] protected Actor _character; //Character properties

        protected Animator _animator;  //Character animator
        protected Health _health; //Heal component
        protected Movement.Mover _mover;
        protected static readonly int Speed = Animator.StringToHash("speed");

        public Actor ScriptableCha
        {
            get
            {
                return this._character;
            }
        } //Returns this charater properties

        void OnDie()
        {
            
        }
    }
}
