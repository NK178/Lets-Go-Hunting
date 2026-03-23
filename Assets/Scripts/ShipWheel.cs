using System;
using UnityEngine;

public class ShipWheel : MonoBehaviour
{
    [SerializeField] private Transform fixedPosition;

    [SerializeField] private float propellerAcceleration;
    [SerializeField] private float maxAheadPropellerSpeed;
    [SerializeField] private float maxAsternPropellerSpeed;


    [SerializeField] private float rudderTurnSpeed;
    [SerializeField] private float maxRudderAngle;

    private float currentPropellerSpeed;
    private float currentRudderAngle; 

    bool isPlayerInRange = false;
    bool isPlayerDriving = false;

    private Transform playerRef; 

    public static Action<bool> onPlayerAtWheel;

    public Action<float> onPropellerActive; 
    public Action<float> onRudderActive; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPlayerInRange = false;
        isPlayerDriving = false;

    }

    private void OnEnable()
    {
        PlayerInputManager.onInteract += onPlayerInteractWheel;
        PlayerInputManager.onMove += HandleShipControls; 
    }

    private void OnDisable()
    {
        PlayerInputManager.onInteract -= onPlayerInteractWheel;
        PlayerInputManager.onMove -= HandleShipControls;
    }
    // Update is called once per frame
    void Update()
    {


    }

    private void onPlayerInteractWheel()
    {
        if (!isPlayerInRange)
            return;

        Debug.Log("PLAYER DRIVING: " + isPlayerDriving);
        isPlayerDriving = !isPlayerDriving;

        onPlayerAtWheel?.Invoke(isPlayerDriving);    

        if (!playerRef)
            playerRef = fixedPosition;    
    }

    private void HandleShipControls(Vector2 direction)
    {

        if (!isPlayerDriving)
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


}
