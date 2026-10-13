using System;
using UnityEngine;

public class ShipWheel : MonoBehaviour
{
    [SerializeField] private Transform playerFixedLocation;


    [Header("Propeller")]
    [SerializeField] private float propellerAcceleration;
    [SerializeField] private float maxAheadPropellerSpeed;
    [SerializeField] private float maxAsternPropellerSpeed;
    [SerializeField] private float propellerSpeedDecay;

    [Header("Rudder")]
    [SerializeField] private float rudderTurnAcceleration;
    [SerializeField] private float maxRudderAngle;
    [SerializeField] private float rudderAngleDecay;

    [Header("Auto Drive")]
    [SerializeField] private float autoPropellerCruiseSpeed;
    [SerializeField] private float autoRudderCruiseSpeed;



    private float currentPropellerSpeed;
    private float currentRudderAngle; 

    bool isPlayerInRange = false;
    //bool isPlayerDriving = false;
    bool isShipAlive = true;

    private Transform playerRef; 

    //public static Action<bool> onPlayerAtWheel;


    public Action<SHIPMODE> onChangeShipMode;
    public Action<float> onPropellerActive; 
    public Action<float> onRudderActive;

    private SHIPMODE referenceShipMode;


    //trying out new method 
    private float currentPropellerAcceleration = 0; 
    private float currentRudderTurnAcceleration = 0; 
    private bool isPropellerActive = false;


    private int currentRudderDirection;
    private int currentPropellerDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPlayerInRange = false;
        //isPlayerDriving = false;


    }

    private void OnEnable()
    {
        PlayerInputManager.onInteract += onPlayerInteractWheel;
        PlayerInputManager.onMove += HandleShipControls;
        Ship.onShipDeath += OnShipDeath;
        Ship.onShipChangedMode += SetShipMode;

        //Ship.onShipIsCombatMode += HandleShipInCombat;

    }

    private void OnDisable()
    {
        PlayerInputManager.onInteract -= onPlayerInteractWheel;
        PlayerInputManager.onMove -= HandleShipControls; 
        Ship.onShipDeath -= OnShipDeath;
        Ship.onShipChangedMode -= SetShipMode; 
        //Ship.onShipIsCombatMode -= HandleShipInCombat;
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void SetShipMode(SHIPMODE shipMode)
    {
        referenceShipMode = shipMode;

        if (shipMode == SHIPMODE.MANUAL_DRIVE)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = playerFixedLocation.position;
                player.transform.rotation = playerFixedLocation.rotation;
            }
        }
    }

    private void onPlayerInteractWheel()
    {
        if (!isPlayerInRange || !isShipAlive)
            return;

        if (referenceShipMode == SHIPMODE.IDLE)
            onChangeShipMode?.Invoke(SHIPMODE.MANUAL_DRIVE);
        else if (referenceShipMode == SHIPMODE.MANUAL_DRIVE)
            onChangeShipMode?.Invoke(SHIPMODE.IDLE);


    }

    private void HandleShipControls(Vector2 direction)
    {
        if (referenceShipMode != SHIPMODE.MANUAL_DRIVE)
            return;

        HandleShipPropeller(direction.y);
        HandleShipRudder(direction.x);
    }

    //trying out new way 
    private void HandleShipPropeller(float input)
    {
        //going ahead 
        if (input > 0)
            currentPropellerDirection = 1;
        else if (input < 0)
            currentPropellerDirection = -1;
        else
            currentPropellerDirection = 0;
    }

    //trying out new way 
    private void HandleShipRudder(float input)
    {
        //going starboard
        if (input > 0)
            currentRudderDirection = 1;
        //going port side 
        else if (input < 0)
            currentRudderDirection = -1;
        else
            currentRudderDirection = 0;
    }

    public float GetMaxAheadSpeed()
    {
        return maxAheadPropellerSpeed;
    }

    public float GetMaxAsternSpeed()
    {
        return maxAsternPropellerSpeed;
    }

    public float GetPropellerSpeedDecay()
    {
        return propellerSpeedDecay;
    }

    public float GetRudderAngleDecay()
    {
        return rudderAngleDecay; 
    }

    public float GetMaxRudderAngle()
    {
        return maxRudderAngle;
    }

    public float GetPropellerCruiseSpeed()
    {
        return autoPropellerCruiseSpeed;
    }

    public float GetRudderCruiseSpeed()
    {
        return autoRudderCruiseSpeed;
    }

    public float GetRudderAcceleration()
    {
        return rudderTurnAcceleration;
    }

    public float GetPropellerAcceleration()
    {
        return propellerAcceleration; 
    }

    public int GetRudderDirection()
    {
        return currentRudderDirection; 
    }

    public int GetPropellerDirection()
    {
        return currentPropellerDirection;
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.gameObject.tag == "Player")
        {
            isPlayerInRange = true;
            playerRef = other.gameObject.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerInRange = false;
            playerRef = null;

        }
    }

    private void OnShipDeath()
    {
        isShipAlive = false;
    }

}
