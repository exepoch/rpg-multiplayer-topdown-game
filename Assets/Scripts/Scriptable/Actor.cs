using System;
using Managers;
using UnityEngine;

namespace Scriptable
{
    [CreateAssetMenu(menuName = "ScriptableObject/Character", fileName = "Character")]
    public class Actor : ScriptableResource
    {
        [Space,Header("General")]
        public float Health;
        public float MoveSpeed;
        public float experiance;
        public string displayName;
    
        [Space, Header("Player Specific")]
        public string key;
        public string catalog;
        public int price = 0;
        public string currency = "CO";
    
    
        [Tooltip("Just for the enemies, player gets weapon from account purchase table"),Space,Header("Enemy Specific")]
        public string aiWeaponType = "basic_sword";

        const string consumedName = "ConsumedCharacter";
    
        public void Spawn(GameObject parent,SkinnedMeshRenderer renderer, string oldKey = "")
        {
            //DestroyOtherVisual(parent.transform.Find(oldKey).gameObject);
            DestroyOtherVisual(renderer);

            renderer.sharedMesh = Instantiate(Resources.Load<Mesh>("Meshes/" + key));

            //parent.transform.Find(key).gameObject.SetActive(true);
            //parent.SetActive(true);
        }

        private void DestroyOtherVisual(SkinnedMeshRenderer vis)
        {
            //vis.SetActive(false);
            vis.sharedMesh = null;
        }

        //Get all scriptable purhase variables
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
                case "price":
                    return price.ToString();
                case "damage":
                    return InventoryManager.GetWeaponWithKey(aiWeaponType).damage.ToString();
                case "durability":
                    return Health.ToString();
                default:
                    return null;
            }
        }

        //False means Actor, true means Weapon
        public override bool Type()
        {
            return false;
        }
    }
}
