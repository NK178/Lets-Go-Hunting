using System;
using UnityEngine;

public class ShipWheel : MonoBehaviour
{
    [SerializeField] private Transform playerFixedLocation;


    [SerializeField] private float propellerAcceleration;
    [SerializeField] private float maxAheadPropellerSpeed;
    [SerializeField] private float maxAsternPropellerSpeed;


    [SerializeField] private float rudderTurnSpeed;
    [SerializeField] private float maxRudderAngle;

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
        //if (isPlayerDriving && !isShipAlive)
        //{
        //    isPlayerDriving = !isPlayerDriving;
        //    onPlayerAtWheel?.Invoke(isPlayerDriving);
        //}
    }



    private void SetShipMode(SHIPMODE shipMode)
    {
        referenceShipMode = shipMode;
    }

    private void onPlayerInteractWheel()
    {
        if (!isPlayerInRange || !isShipAlive)
            return;

        //isPlayerDriving = !isPlayerDriving;

        //SHIPMODE newMode = SHIPMODE.MANUAL_DRIVE;
        //if (!isPlayerDriving)
        //    newMode = SHIPMODE.IDLE;

        if (referenceShipMode == SHIPMODE.IDLE)
            onChangeShipMode?.Invoke(SHIPMODE.MANUAL_DRIVE);
        else if (referenceShipMode == SHIPMODE.MANUAL_DRIVE)
            onChangeShipMode?.Invoke(SHIPMODE.IDLE);


        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {

            player.transform.position = playerFixedLocation.position;
            player.transform.rotation = playerFixedLocation.rotation;

            //player.transform.position = gameObject.transform.position;
            //player.transform.rotation = gameObject.transform.rotation;
        }
    }


    //private void HandleShipInCombat(bool condition)
    //{
    //    if (condition && isPlayerDriving)
    //    {
    //        onPlayerInteractWheel();
    //    }
    //}

    private void HandleShipControls(Vector2 direction)
    {
        //if (!isPlayerDriving)
        //    return; 

        if (referenceShipMode != SHIPMODE.MANUAL_DRIVE)
            return;     

        //for moving forward and back 

        if (direction.y != 0)
            HandleShipPropeller(direction.y);

        if (direction.x != 0)
            HandleShipRudder(direction.x);
    }

    private void HandleShipPropeller(float input)
    {
        //going ahead 
        if (input > 0)
        {
            currentPropellerSpeed += propellerAcceleration * Time.deltaTime;
        }
        else if (input < 0)
        {
            currentPropellerSpeed -= propellerAcceleration * Time.deltaTime;
        }

        //forward postive backwards negative 
        if (currentPropellerSpeed > maxAheadPropellerSpeed)
            currentPropellerSpeed = maxAheadPropellerSpeed;
        else if (currentPropellerSpeed < maxAsternPropellerSpeed)
            currentPropellerSpeed = maxAsternPropellerSpeed;

        onPropellerActive?.Invoke(currentPropellerSpeed);
    }

    private void HandleShipRudder(float input)
    {
        //going starboard
        if (input > 0)
        {
            currentRudderAngle += rudderTurnSpeed * Time.deltaTime;
        }
        //going port side 
        else if (input < 0)
        {
            currentRudderAngle -= rudderTurnSpeed * Time.deltaTime;
        }

        if (Mathf.Abs(currentRudderAngle) >= maxRudderAngle)
        {
            if (currentRudderAngle < 0)
                currentRudderAngle = -maxRudderAngle;
            else
                currentRudderAngle = maxRudderAngle;
        }

        onRudderActive?.Invoke(currentRudderAngle);

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
