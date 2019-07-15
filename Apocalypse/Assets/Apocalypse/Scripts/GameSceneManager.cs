using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInfo
{
    public Collider collider = null;
    public CharacterManager characterManager = null;
    public Camera camera = null;
    public CapsuleCollider meleeTrigger = null;
}


public class GameSceneManager : MonoBehaviour
{

    [SerializeField] private ParticleSystem _bloodParticle= null;
    [SerializeField] private ParticleSystem _gunParticle = null;
    //statics
    private static GameSceneManager _instance = null;
    public static GameSceneManager instance
    {
        get
        {
            if (_instance == null)
                //find the very first GameSceneManager type object in the scene. In this project GameSceneManager object
                _instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));
            return _instance;
        }
    }

    //private
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();
    private Dictionary<int, PlayerInfo> _playerInfos = new Dictionary<int, PlayerInfo>();
    //get getter
    public ParticleSystem bloodParticle { get { return _bloodParticle; } }
    public ParticleSystem gunParticle { get { return _gunParticle; } }

    //public
    /*********************************************************/
    //Stores the passed state machine in the dictionary with the suppiled key
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if (!_stateMachines.ContainsKey(key))
        {
            _stateMachines[key] = stateMachine;
        }
    }

    /*********************************************************/
    //returns an AIStateMachine reference searched on by the ID of an object
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;
        if(_stateMachines.TryGetValue(key,out machine))
        {
            return machine;
        }
        return null;
    }

    /*********************************************************/
    //Stores the passed playerInfo in the dictionary with the suppiled key
    public void RegisterPlayerInfo(int key, PlayerInfo playerInfo)
    {
        if (!_playerInfos.ContainsKey(key))
        {
            _playerInfos[key] = playerInfo;
        }
    }

    /*********************************************************/
    //returns an PlayerInfo reference searched on by the ID of a player
    public PlayerInfo GetPlayerInfo(int key)
    {
        PlayerInfo info = null;
        if (_playerInfos.TryGetValue(key, out info))
        {
            return info;
        }
        return null;
    }

}
