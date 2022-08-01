using System;
using Network;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;

namespace Managers
{
    public class GameEventManager : MonoBehaviour
    {
        [Header("Particle Effects")] 
        private const string _rainEffect = "Prefabs/Other/RainParticle"; //RainParticlePrefab
        private const string _snowEffect = "Prefabs/Other/SnowParticle"; //SnowParticlePrefab

        [Header("Textures")]
        [SerializeField] private Texture villageSummerTexture; //Green MaterialTextıre
        [SerializeField] private Texture villageSnowTexture; //Snow MaterialTextıre
        [SerializeField] private Texture villageAutmnTexture; //Autmn MaterialTextıre

        [Header("Materials")]
        [SerializeField] private Material villageMaterial; //Village objects material

        [SerializeField] private float weatherChangeRate = 10f; //Seconds
        [SerializeField] private float seasonChangeRate = 30f; //Seconds
        [SerializeField] private float subEventRate = 20f; //Seconds
        
        [SerializeField] private Season _currentSeason = Season.Summer; //Current season
        [SerializeField] private Weahter _currentWeahter = Weahter.Dry; //Current weather
        
        private static GameObject _weatherEffect; //Cahche the weather particle for disabling
        private PhotonView _pw;
        
        [Header("Counters / DEBUG")]
        [SerializeField] private float lastSeasonChanged;
        [SerializeField] private float lastWeatherChanged;
        [SerializeField] private float lastSubEventProcessed;

        static readonly Random _random = new Random ();
        private bool canControlEvent = false;
        private Season season = Season.Summer;

        private void Awake()
        {
            _pw = GetComponent<PhotonView>();
            villageMaterial = Resources.Load<Material>("Material/Village_Base");
            
            NetworkManager.OnRoomCreated += () =>
            {
                canControlEvent = true;
                villageMaterial.mainTexture = villageSummerTexture;
                _currentSeason = Season.Summer;
            };
        }

        private void Update()
        {
            if(!canControlEvent || !PhotonNetwork.InRoom) return;
            
            TimeIncrementers();
            EventCheckers();
        }

        //Calculates events happen times
        private void TimeIncrementers()
        {
            lastSeasonChanged += Time.deltaTime;
            lastWeatherChanged += Time.deltaTime;
            //lastSubEventProcessed += Time.deltaTime;  //No sub event exist yet
        }

        private void EventCheckers()
        {
            if (lastWeatherChanged >= weatherChangeRate)
            {
                lastWeatherChanged = 0;
                _pw.RPC("ChangeWeatherEvent", RpcTarget.AllBuffered);
            }

            if (lastSeasonChanged >= seasonChangeRate)
            {
                lastSeasonChanged = 0;
                
                season = RandomEnumValue<Season>();
                int seaseonType = 0;
                if (season == Season.Summer)
                {
                    seaseonType = 0;
                }
                else if (season == Season.Winter)
                {
                    seaseonType = 1;
                }
                else
                {
                    seaseonType = 2;
                }

                _pw.RPC("ChangeSeasonEvent", RpcTarget.AllBuffered, seaseonType);
            }
            
            //No sub event exist currently
            /*if (lastSubEventProcessed >= subEventRate)
            {
                lastSubEventProcessed = 0;
                _pw.RPC("StartSubEvent", RpcTarget.AllBuffered);
            }*/
        }

        #region PunCallBacks

         [PunRPC]
        private void ChangeWeatherEvent()
        {
            Weahter newWeather;
            string effectUrl = "";

            //Every wather condition can happen in winter
            if (_currentSeason == Season.Winter)
            {
                newWeather = RandomEnumValue<Weahter>();
            }
            else //Other seaseon can have just Dry and Raint condition
            {
                do
                {
                    newWeather = RandomEnumValue<Weahter>();
                } while (newWeather == Weahter.Snowy); //Only winter can be snowy
            }
            
            if(newWeather == _currentWeahter) return;
            
            if(_weatherEffect !=null)
                PhotonNetwork.Destroy(_weatherEffect);

            switch (newWeather)
            {
                case Weahter.Rainy:
                    effectUrl = _rainEffect;
                    GameManager.instance.ChangeHappinesBalance(-10);
                    break;
                case Weahter.Snowy:
                    effectUrl = _snowEffect;
                    GameManager.instance.ChangeHappinesBalance(-20);
                    break;
                case Weahter.Dry:
                    effectUrl = "";
                    GameManager.instance.ChangeHappinesBalance(+10);
                    break;
            }
            
            if(effectUrl !="") 
                _weatherEffect = PhotonNetwork.InstantiateRoomObject(effectUrl, Vector3.zero, Quaternion.identity);
            
            _currentWeahter = newWeather;
        }

        [PunRPC]
        private void ChangeSeasonEvent(int setter)
        {
            switch (setter)
            {
                case 0://Summer
                    villageMaterial.mainTexture = villageSummerTexture;
                    break;
                case 1://Winter
                    villageMaterial.mainTexture = villageSnowTexture;
                    break;
                case 2://Autmn
                    villageMaterial.mainTexture = villageAutmnTexture;
                    break;
            }
            _currentSeason = season;
        }

        [PunRPC]
        private void StartSubEvent()
        {
            
        }

        #endregion

       
        
        //Returns random valu accord. type parameter
        static T RandomEnumValue<T> ()
        {
            var v = Enum.GetValues (typeof (T));
            return (T) v.GetValue (_random.Next(v.Length));
        }

        private enum Season
        {
            Summer,
            Winter,
            Autmn
        }

        private enum Weahter
        {
            Dry,
            Rainy,
            Snowy
        }
    }
}
