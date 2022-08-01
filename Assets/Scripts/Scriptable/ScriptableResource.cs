using System;
using PlayFab.EconomyModels;
using UnityEngine;

namespace Scriptable
{
    public abstract class ScriptableResource : ScriptableObject
    {
        public abstract Tuple<string, string, int, string> Data();
        public abstract string GetSpecificInfo(string info);
        
        /// <summary>
        ///  //return true at override if its Weapon.
        /// //TODO Can be return type as string when more type added.
        /// </summary>
        public abstract bool Type();
    }
}
