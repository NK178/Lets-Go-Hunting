using System.Collections.Generic;
using UnityEngine;


public class SectionManager : MonoBehaviour
{
    [SerializeField] private int sectionIndex; 
    [SerializeField] private List<BaseWaveFunction> waveDataList;

    [SerializeField] private bool enableDebugText = false;

    private BaseWaveFunction currentWaveFunction;

    private int currentWaveIndex; 

    private bool isSectionOver = false;

    //public GAMESIGNAL lastRecievedGameSignal; 

    //void Awake()
    //{
    //    currentWaveIndex = 0;

    //    //register itself with the Game Manager 
    //    if (GameManager.Instance != null)
    //    {
    //        GameManager.Instance.RegisterSection(this);
    //    }
    //}

    private void Start()
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

        if (isSectionOver)
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
        isSectionOver = false;

        StartNextWave();
        //currentWaveFunction = waveDataList[currentWaveIndex];

        //currentWaveFunction.Excute(this);

        //currentWaveIndex++;
    }

    
    //public void SwitchToNextWave()
    //{
    //    StopAllCoroutines();
    //    if (currentWaveIndex >= waveDataList.Count)
    //        return; 

    //    currentWaveFunction = waveDataList[currentWaveIndex];

    //    currentWaveFunction.Excute(this);

    //    currentWaveIndex++;

    //    isSectionActive = true;
    //}


    //public void StartNextWave()
    //{
    //    if (enableDebugText)
    //        Debug.Log("START NEW WAVE");

    //    SwitchToNextWave();
    //    //currentWaveFunction = waveDataList[currentWaveIndex];
    //    //currentWaveFunction.Excute(this);
    //}


    public void StartNextWave()
    {
        StopAllCoroutines();
        if (currentWaveIndex >= waveDataList.Count)
            return;

        currentWaveFunction = waveDataList[currentWaveIndex];

        currentWaveFunction.Excute(this);

        currentWaveIndex++;

        isSectionOver = false; 
    }

    public void EndWave()
    {

        if (enableDebugText)
            Debug.Log("END WAVE CURR INDEX: " + currentWaveIndex + " TOTAL: " + waveDataList.Count);

        StopAllCoroutines();
        currentWaveFunction.Exit(this);
        //currentWaveIndex++;
        //to end this part 
        if (currentWaveIndex + 1 >= waveDataList.Count)
        {
            Debug.Log("SECTION IS OVER");
            isSectionOver = true;
        }
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
        else if (signal.Contains("MID"))
        {
            Debug.Log("MID SECTION CHANGE");
            EndWave();
            StartNextWave();
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
