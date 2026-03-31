using UnityEngine;



public enum WAVETYPE { 
    SWARM, 
    BOSS
}


//[CreateAssetMenu(fileName = "BaseWaveData", menuName = "Scriptable Objects/BaseWaveData")]
abstract public class BaseWaveFunction : ScriptableObject
{

    public WAVETYPE waveType; 

    abstract public void Excute(SectionManager sectionManager); 
}
