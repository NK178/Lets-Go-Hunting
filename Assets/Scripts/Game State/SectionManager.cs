using System.Collections.Generic;
using UnityEngine;


//idk if this will work 

public class SectionManager : MonoBehaviour
{

    [SerializeField] private int sectionIndex; 
    [SerializeField] private List<BaseWaveFunction> waveDataList;


    private BaseWaveFunction currentWaveFunction;

    private int currentWaveIndex; 

    private bool isSectionOver = false;
    private bool isSectionActive = false; 


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


    private void SwitchToNextWave()
    {
        StopAllCoroutines();
        currentWaveFunction = waveDataList[currentWaveIndex];

        currentWaveFunction.Excute(this);

        currentWaveIndex++;
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
