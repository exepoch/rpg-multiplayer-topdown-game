using System;
using Gui;
using Managers;
using Photon.Pun;
using UnityEngine;

public class House : MonoBehaviour
{
    [SerializeField] private GameObject _house;
    [SerializeField] private DestroyOnTouch _destroyer;
    [SerializeField] private GameObject _purchParticle;

    private PopulationAndHouseManager _houseManager;
    private bool isBuilded; //if house builded

    private void Awake()
    {
        _houseManager = FindObjectOfType<PopulationAndHouseManager>();
    }

    private void OnMouseUp()
    {
        //Just the host can build.
        if (!isBuilded && GameManager.Time() && PhotonNetwork.IsMasterClient)
        {
            _houseManager.OpenBuildPanel(this);
        }
    }

    //Calls by population manager if player purchased house
    [PunRPC]
    public void Build()
    {
        //Some build animation
        _house.SetActive(true);
        _purchParticle.SetActive(false);
        isBuilded = true;
        _destroyer.SetActivity(true);
    }

    public void Leave()
    {
        
    }
}
