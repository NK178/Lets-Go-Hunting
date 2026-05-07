using System;
using UnityEngine;

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

    public GAMESIGNAL GetSignalName()
    {
        return gamePhase; 
    }
}


//USING THE START AND END FOR SUBSTRINGS KIDNA IMPORTANT
//can consider changing the substring to STARTPHASE and ENDPHASE in the future i guess
public enum GAMESIGNAL { 
    START_SECTION_1_SWORDFISH,
    END_SECTION_1_SWORDFISH,
    START_SECTION2_INSECT_LEFTSIDE,
    MID_SECTION2_INSECT_RIGHTSIDE,
    END_SECTION2_INSECT,
    START_SECTION3_SHARK, 
    //PROB GONNA HAVE LOTS OF MID SECTIONS OR SMTH 
    //END_SECTION3_SHARK,
    END_AREA, //aka switch scene
    NUM_SIGNALS
}
