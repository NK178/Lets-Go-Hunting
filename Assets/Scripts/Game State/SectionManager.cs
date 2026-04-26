using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


//idk if this will work 

public class SectionManager : MonoBehaviour
{

    [SerializeField] private int sectionIndex; 
    [SerializeField] private List<BaseWaveFunction> waveDataList;


    [SerializeField] private bool enableDebugText = false;


    private BaseWaveFunction currentWaveFunction;

    private int currentWaveIndex; 

    private bool isSectionOver = false;
    private bool isSectionActive = false;



    //public GAMESIGNAL lastRecievedGameSignal; 

    void Awake()
    {
        currentWaveIndex = 0;

        //register itself with the Game Manager 
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSection(this);
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (!isSectionActive)
            return;

        if (currentWaveFunction != null)
        {
            currentWaveFunction.Process(this);
        }

        //if (!currentWaveFunction)
        //{
        //    currentWaveFunction.Excute(this);
        //}

    }

    public void InitSection()
    {
        isSectionActive = true;

        SwitchToNextWave();
        //currentWaveFunction = waveDataList[currentWaveIndex];

        //currentWaveFunction.Excute(this);

        //currentWaveIndex++;
    }

    
    public void SwitchToNextWave()
    {
        StopAllCoroutines();
        if (currentWaveIndex >= waveDataList.Count)
            return; 

        currentWaveFunction = waveDataList[currentWaveIndex];

        currentWaveFunction.Excute(this);

        currentWaveIndex++;

        isSectionActive = true;
    }


    public void StartNextWave()
    {
        if (enableDebugText)
            Debug.Log("START NEW WAVE");

        SwitchToNextWave();
        //currentWaveFunction = waveDataList[currentWaveIndex];
        //currentWaveFunction.Excute(this);
    }

    public void EndWave()
    {

        if (enableDebugText)
            Debug.Log("END WAVE");

        StopAllCoroutines();
        currentWaveFunction.Exit(this);
        currentWaveIndex++;
        //to end this part 
        if (currentWaveIndex >= waveDataList.Count)
            isSectionOver = true; 
    }


    //Hmmmm potentially need to change this function 
    public void ReadGameSignal(GAMESIGNAL gameSignal)
    {
        if (enableDebugText)
            Debug.Log("SectionManager reading signal: " + gameSignal);

        //lastRecievedGameSignal = gameSignal;
        string signal = gameSignal.ToString();

        if (signal.Contains("START"))
        {
            StartNextWave();
        }
        else if (signal.Contains("END"))
        {
            EndWave();
        }
    }

    public bool IsSectionOver()
    {
        return isSectionOver;
    }

    public int GetSectionIndex()
    {
        return sectionIndex;
    }



}
