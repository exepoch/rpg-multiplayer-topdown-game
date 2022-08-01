using Control;
using Photon.Pun;
using UnityEngine;

public class DestroyOnTouch : MonoBehaviour
{
    //Destroyin function is active?
    [SerializeField] private bool isEnable;
    
    private void OnTriggerEnter(Collider other)
    {
        if(!isEnable || !other.CompareTag("npc")) return;
        
        other.GetComponent<NpcController>().UnRegister();
        PhotonNetwork.Destroy(other.gameObject);
    }

    public void SetActivity(bool set)
    {
        isEnable = set;
    }

    public bool Enabled()
    {
        return isEnable;
    }
}
