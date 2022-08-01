using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat;
using Network;
using Photon.Pun;
using Scriptable;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class ActorManager : MonoBehaviour
    {
        public static ActorManager instance; //Singleton
        
        private static readonly List<string> npcStaticList = new List<string>(); //Random npc picker for Static function
        [SerializeField] private List<Fighter> FriendList;   // Real players list in game
        [SerializeField] private List<Fighter> FoeList;  // Instantiated enemies in game
        [SerializeField] private List<Fighter> NPCList;  // Instantiated enemies in game

        private static readonly Dictionary<string, Actor> actLib = new Dictionary<string, Actor>(); // All player character actor types
        private static readonly Dictionary<string, Actor> npcLib = new Dictionary<string, Actor>(); // All npc character actor types
        private static readonly Dictionary<string, Actor> enemyLib = new Dictionary<string, Actor>(); // All enemy AI character actor types
        private static readonly Dictionary<string, Actor> alliedLib = new Dictionary<string, Actor>(); // All enemy AI character actor types
        
        public static Action onPlayerJoined;  // Calss when player joins the game BUG USAGE IS TOO EXPENSIVE WHEN MULTIPLE JOIN CALL
        public static Action onFoeJoined; // Calss when AI joins the game  BUG USAGE IS TOO EXPENSIVE WHEN MULTIPLE JOIN CALL
        public static Action onNPCJoined; // Calss when NPC joins the game  BUG USAGE IS TOO EXPENSIVE WHEN MULTIPLE JOIN CALL, SUGGEST POOLING

        private const string characterUrl = "Prefabs/Characters/PLAYER"; // PlayerCharacter prefab url
        private const string enemyUrl = "Prefabs/Characters/ENEMY"; // AICharacter prefab url
        private const string alliedUrl = "Prefabs/Characters/ALLIEDAI"; // AICharacter prefab url
        private const string NPCUrl = "Prefabs/Characters/NPC";   // NPCCharacter prefab url
        private const string defEnemyKey = "Chr_Dungeon_GoblinMale_01"; // Default Enemy type actor UniqKey
        private const string defAlliedKey = "Chr_Vikings_Warrior_01"; // Default Enemy type actor UniqKey
        
        private static string enemySpawnKey; // Enemy key to spawn from enemyActorLibrary
        private static string alliedSpawnKey; // Enemy key to spawn from enemyActorLibrary
        private static int enemyCount; //Current enemy count to check alive enemy count
        
        [SerializeField] private Transform playerSpawnPos; //Position for Spawning player location
        [SerializeField] private Transform enemySpawnPos; //Position for Spawning enemy location
        [SerializeField] private Transform alliedSpawnPos; //Position for Spawning allied location
        
        [Space]
        [SerializeField] private List<Actor> playerActorList; //Serialized type of actor list, to fill dictionary on awake
        [SerializeField] private List<Actor> npcActorList;  //Serialized type of actor list, to fill dictionary on awake
        [SerializeField] private List<Actor> enemyActorList; //Serialized type of actor list, to fill dictionary on awake
        [SerializeField] private List<Actor> alliedActorList; //Serialized type of actor list, to fill dictionary on awake
        [SerializeField] private List<Transform> npcSpawnLocs;  //Random npc spawn location list

        private void Awake()
        {
            instance = this;
            
            FriendList = new List<Fighter>();
            FoeList = new List<Fighter>();
            NPCList = new List<Fighter>();

            //Fills the player actor dictionary
            foreach (Actor actor in playerActorList)
            {
                if(!actLib.ContainsKey(actor.key))
                    actLib.Add(actor.key, actor);
            }

            //Fills the npc actor dictionary
            foreach (Actor actor in npcActorList)
            {
                if(npcLib.ContainsKey(actor.key)) continue;
                
                npcLib.Add(actor.key, actor);
                npcStaticList.Add(actor.key);
            }

            //Fills the enemy actor dictionary
            foreach (Actor actor in enemyActorList) 
            {
                if(!enemyLib.ContainsKey(actor.key))
                    enemyLib.Add(actor.key, actor);
            }
            
            //Fills the allied actor dictionary
            foreach (Actor actor in alliedActorList) 
            {
                if(!alliedLib.ContainsKey(actor.key))
                    alliedLib.Add(actor.key, actor);
            }

            enemySpawnKey = defEnemyKey; //Set initial enemy type
            alliedSpawnKey = defAlliedKey; //Set initial allied type
        }

        //Returns logined user actor
        public static Actor GetActor()
        {
            return actLib[AccountManager.instance.GetAccountCharacter()];
        }

        //Get the actor type within the key
        public static Actor GetEnemy()
        {
            return enemyLib[enemySpawnKey];
        }
        
        //Get the actor type within the key
        public static Actor GetAllied()
        {
            return alliedLib[alliedSpawnKey];
        }

        //Gets a random npc actor from library
        public static Actor GetRandomNPC()
        {
            return npcLib[npcStaticList[Random.Range(0, npcStaticList.Count)]];
        }
    
        //Set the actor type for spwaning enemy
        public void SetEnemyType(string key)
        {
            enemySpawnKey = key;
        }

        //Current player list register function
        public void AddToFriendList(Fighter character)
        {
            FriendList.Add(character);
            onPlayerJoined?.Invoke();
        }

        //Current enemy list register function
        public void AddToFoeList(Fighter character)
        {
            FoeList.Add(character);
            onFoeJoined?.Invoke();
            enemyCount++;
        }
        
        public void AddToNPCList(Fighter character)
        {
            NPCList.Add(character);
            
        }
        
        //Unregister from enemyList
        public void RemoveFromFoeList(Fighter character)
        {
            var li = new List<Fighter>();
            FoeList.Remove(character);
            
            foreach (Fighter fighter in FoeList)
            {
                if(fighter != null)
                    li.Add(fighter);
            }

            FoeList = li;
            
            onFoeJoined?.Invoke();
            
            enemyCount--;
            if (!PhotonNetwork.IsMasterClient) return;
            
            //If all enemies are died, then night has survived, warn the manager
            if (enemyCount == 0)
            {
                GameManager.instance.SetTime();   
            }
        }

        //Unregister player from player list in current game
        public void RemoveFromFriendList(Fighter character)
        {
            print("Removed friend");
            var li = new List<Fighter>();
            FriendList.Remove(character);
            
            foreach (Fighter fighter in FriendList)
            {
                if(fighter != null)
                    li.Add(fighter);
            }

            FriendList = li;
            onPlayerJoined.Invoke();
        }
        
        //Unregister npc from npc list in current game
        public void RemoveFromNPCList(Fighter character)
        {
            var li = new List<Fighter>();
            NPCList.Remove(character);
            
            foreach (Fighter fighter in NPCList)
            {
                if(fighter != null)
                    li.Add(fighter);
            }

            NPCList = li;
        }

        //Get all players currently in the game
        public List<Fighter> GetPlayerList()
        {
            return FriendList;
        }
    
        //Get all enemies list in the game
        public List<Fighter> GetAIEnemyList()
        {
            return FoeList.ToList();
        }
        
        //Get all enemies list in the game
        public byte GetNPCCount()
        {
            return (byte)NPCList.Count;
        }

        //Spawns playerchatacter
        public void SpawnPlayer()
        {
            StartCoroutine(SpawnCharacter(characterUrl, playerSpawnPos.position, true));
        }
    
        //Spawns enemy
        public void SpawnEnemy(int nightMultiplier)
        {
            enemySpawnKey = enemyActorList[nightMultiplier-1].key;

            StartCoroutine(SpawnCharacter(enemyUrl, enemySpawnPos.position, false, 3));
        }
        
        public void SpawnAllied(string alliedKey)
        {
            alliedSpawnKey = alliedKey;

            StartCoroutine(SpawnCharacter(alliedUrl, alliedSpawnPos.position, false, 1));
        }

        //spawns npc
        private void SpawnNPC()
        {
            StartCoroutine(SpawnCharacter(NPCUrl, npcSpawnLocs[Random.Range(0, npcSpawnLocs.Count)].position, false));
        }
        
        //Public method for spawning an npc wave
        public void SpawnNPCWave()
        {
            StartCoroutine(SpawnWave(2));
        }

        //Get all npc spawn positions
        public List<Transform> GetPosList()
        {
            return npcSpawnLocs;
        }

        //Spawn npc wave
        IEnumerator SpawnWave(int waveCount)
        {
            for (int i = 0; i < waveCount; i++)
            {
                SpawnNPC();
                yield return new WaitForSeconds(.5f);
            }
            onNPCJoined?.Invoke();
        }

        //Core character spawner.
        IEnumerator SpawnCharacter(string prefUrl,Vector3 spawnPos, bool isPlayer, float delay = 0)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            if (isPlayer)
            {
                print("PlayerSpawn");
                PhotonNetwork.Instantiate(prefUrl, spawnPos, Quaternion.identity);
            }
            else
            {
                // Only masterPlayer can Instantiate a room object
                print("EnemySpawn");
                PhotonNetwork.InstantiateRoomObject(prefUrl, spawnPos, Quaternion.identity);
            }
        }

       
    }
}
