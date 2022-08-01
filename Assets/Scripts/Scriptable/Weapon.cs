using System;
using Combat;
using UnityEngine;

namespace Scriptable
{
    [CreateAssetMenu(menuName = "ScriptableObject/Weapon", fileName = "Weapon")]
    public class Weapon : ScriptableResource
    {
        public string displayName;
        public string key;
        public string catalog;
        public int price;
        public string currency = "CO";
        public float durability;

        public AnimatorOverrideController animatorOverride;
        public GameObject equippedPrefab;
        public Projectile projectile;

        public float damage = 5f;
        public float weaponRange = 2f;
        public bool isRightHanded = true;
        public bool isRanged;
        public IWeaponType _handledWeapon;
        public string projectileUrl = "Prefabs/Other/ArrowProjectile";
        
        const string weaponName = "Weapon";

        public IWeaponType Spawn(Transform rightHand, Transform leftHand, Animator animator, string _tag)
        {
            DestroyOldWeapon(rightHand, leftHand);

            if (equippedPrefab != null)
            {
                Transform handTransform = GetTransform(rightHand, leftHand);
                GameObject weapon = Instantiate(equippedPrefab, handTransform);
                weapon.name = weaponName;
                
                _handledWeapon = weapon.GetComponent<IWeaponType>(); 
                _handledWeapon.SetProperty(damage, _tag, isRightHanded,projectileUrl);
            }

            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride; 
            }
            else if (overrideController != null)
            {
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }
            
            return _handledWeapon;
        }

        private void DestroyOldWeapon(Transform rightHand, Transform leftHand)
        {
            Transform oldWeapon = rightHand.Find(weaponName);
            if (oldWeapon == null)
            {
                oldWeapon = leftHand.Find(weaponName);
            }
            if (oldWeapon == null) return;

            oldWeapon.name = "DESTROYING";
            Destroy(oldWeapon.gameObject);
        }
    
        
        
        private Transform GetTransform(Transform rightHand, Transform leftHand)
        {
            Transform handTransform = isRightHanded ? rightHand : leftHand;
            return handTransform;
        }

        public bool HasProjectile()
        {
            return projectile != null;
        }

        public float GetRange()
        {
            return weaponRange;
        }

        public override Tuple<string, string, int, string> Data()
        {
            return Tuple.Create(key, catalog, price, currency);
        }
        
        public override string GetSpecificInfo(string info)
        {
            switch (info)
            {
                case "displayname":
                    return displayName;
                case "key":
                    return key;
                case "weapondamage":
                    return damage.ToString();
                case "durability":
                    return durability.ToString();
                case "price":
                    return price.ToString();
                default:
                    return null;
            }
        }

        //False means Actor, true means Weapon
        public override bool Type()
        {
            return true;
        }
    }
}
