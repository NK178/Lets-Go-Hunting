using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//idk if this will work 

public class SectionManager : MonoBehaviour
{

    [SerializeField] private List<BaseWaveFunction> waveDataList;


    private BaseWaveFunction currentWaveFunction;

    private int currentWaveIndex; 

    private bool isSectionOver = false;
    private bool isSectionActive = false; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentWaveIndex = 0;
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
}
