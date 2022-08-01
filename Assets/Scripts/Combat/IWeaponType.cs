using UnityEngine;

namespace Combat
{
    public abstract class IWeaponType : MonoBehaviour
    {
        public virtual void Launch(bool set){} //For close range weapon launch
        public virtual void Launch(Vector3 target,Vector3 leftH,Vector3 rightH){} //For ranged weapon launch

        public virtual void SetProperty(float damage, string tagg, bool hand, string url = ""){}
    }
}
