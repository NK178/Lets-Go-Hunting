using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "GameSignal", menuName = "Scriptable Objects/GameSignal")]
//public class GameSignal : ScriptableObject
//{

//    [SerializeField] private GAMEPHASE gamePhase;
//    private event Action<GAMEPHASE> OnSignalRaised; 

//    public void Raise()
//    {
//        //Debug.Log("INVOKED SIGNAL");
//        OnSignalRaised?.Invoke(gamePhase); 
//    }

//    public void RegisterListener(Action<GAMEPHASE> listener)
//    {
//        OnSignalRaised += listener; 
//    }

//    public void UnregisterListener(Action<GAMEPHASE> listener)
//    {
//        OnSignalRaised -= listener; 
//    }
//}

[CreateAssetMenu(fileName = "GameSignal", menuName = "Scriptable Objects/GameSignal")]
public class GameSignal : ScriptableObject
{

    [SerializeField] private GAMESIGNAL gamePhase;
    private event Action<GAMESIGNAL> OnSignalRaised;

    public void Raise()
    {
        //Debug.Log("INVOKED SIGNAL");
        OnSignalRaised?.Invoke(gamePhase);
    }

    public void RegisterListener(Action<GAMESIGNAL> listener)
    {
        OnSignalRaised += listener;
    }

    public void UnregisterListener(Action<GAMESIGNAL> listener)
    {
        OnSignalRaised -= listener;
    }
}


public enum GAMESIGNAL { 
    START_SECTION_1_SWORDFISH,
    END_SECTION_1_SWORDFISH,
    NUM_SIGNALS
}
