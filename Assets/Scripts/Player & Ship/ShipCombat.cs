using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class PhaseToPosition
{
    public GAMESIGNAL signal;
    public bool isLeft;
}



public class ShipCombat : MonoBehaviour
{

    //CONSIDER SPLITTING TO ANOTHER CLASS 
    [Header("Ship Combat")]
    [SerializeField] private GameObject starboardShootPoint;
    [SerializeField] private GameObject portShootPoint;
    [SerializeField] private List<PhaseToPosition> phaseToShootPositions;
    [SerializeField] private bool enableDebugPrint = false;


    public static Action<Transform> onShipLockTransform;



    private GAMESIGNAL currentWaveSignal; 


    private void OnEnable()
    {
        Ship.onShipChangedMode += HandleShipMode; 
    }

    private void OnDisable()
    {
        Ship.onShipChangedMode -= HandleShipMode;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleShipMode(SHIPMODE mode)
    {

        if (mode == SHIPMODE.COMBAT)
        {
            if (enableDebugPrint)
                Debug.Log("ENTERING COMBAT MODE");
            EnterShipCombatMode(currentWaveSignal);
        }
        else if (mode == SHIPMODE.IDLE || mode == SHIPMODE.MANUAL_DRIVE)
        {
            ExitShipCombatMode();
        }
    }

    //Consider spiltting  to another class this thingy 
    public void EnterShipCombatMode(GAMESIGNAL gamePhase)
    {
        if (enableDebugPrint)
            Debug.Log("SHIP COMBAT MODE " + gamePhase);

        bool isLeft = false;

        foreach (PhaseToPosition phasePos in phaseToShootPositions)
        {
            if (phasePos.signal == gamePhase)
            {
                isLeft = phasePos.isLeft;
                break;
            }
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        GameObject targetPos = starboardShootPoint;
        if (isLeft)
        {
            targetPos = portShootPoint;
        }

        player.transform.position = targetPos.gameObject.transform.position;

        player.transform.rotation = Quaternion.LookRotation(targetPos.gameObject.transform.right, targetPos.gameObject.transform.up);
        onShipLockTransform?.Invoke(targetPos.transform);
    }

    public void ExitShipCombatMode()
    {
        Debug.Log("SHIP EXITING COMBAT MODE TIME TO DRIVE BOI");

        //isInCombatMode = false;

        ////onShipLockTransform?.Invoke(targetPos.transform);
        //onShipIsCombatMode?.Invoke(false);
    }


    public void SetGameSignalReference(GAMESIGNAL gameSignal)
    {
        currentWaveSignal = gameSignal;
    }


}
