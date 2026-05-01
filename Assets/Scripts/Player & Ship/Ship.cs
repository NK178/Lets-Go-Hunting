using System;
using System.Transactions;
using UnityEngine;



public enum SHIPMODE
{
    IDLE,
    COMBAT, 
    MANUAL_DRIVE, //change back to normal drive 
    NUM_MODES
}


public class Ship : MonoBehaviour
{

    [SerializeField] private Vector3 DEBUG_travelDirection;
    [SerializeField] private float DEBUG_waveRockStrength;



    [SerializeField] private ShipWheel shipWheel;
    [SerializeField] private GameObject shipRudder; 
    [SerializeField] private GameObject shipPropeller;

    [SerializeField] private ShipCombat shipCombat;

    [SerializeField] private float shipMaxHealth;


    //CONSIDER SPLITTING TO ANOTHER CLASS
    [Header("Ship AUTO DRIVE CONTROLS")]

    [Header("Ship Controls")]
    [SerializeField] private float moveSpeed;

    [SerializeField] private float angularDamping; 
    [SerializeField] private float maxAngularPower;


    [SerializeField] private float propellerRotationDamping;
    [SerializeField] private float maxPropellerRotateSpeed;


    [Header("DEBUG")]
    [SerializeField] private bool DEBUG_invincible; 

    public static Action onShipDeath;
    public static Action<float, float> onShipHealthChanged;
    public static Action<bool> onShipIsCombatMode;
    public static Action<SHIPMODE> onShipChangedMode; 

    private float propellerPower;
    private float rudderAngle;
    private float angularPower;
    private float rudderForce;
    private float shipHealth;

    private Vector3 currentVelocity;
    private Vector3 movementVector; 
    private Vector3 rotationVector; 

    private bool isPlayerOnShip;

    private bool isShipAlive;

    private bool onAutoDrive = false;

    private DriveCheckpoint targetCheckpoint = null; 

    private SHIPMODE currentMode; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPlayerOnShip = false;
        isShipAlive = true;
        shipHealth = shipMaxHealth;

        //set spawn point 

        GameObject spawnPoint = GameObject.FindGameObjectWithTag("StartPoint");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position; 
            transform.rotation = spawnPoint.transform.rotation; 
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = shipWheel.gameObject.transform.position;
            player.transform.rotation = shipWheel.gameObject.transform.rotation;

        }

        SwitchShipMode(SHIPMODE.IDLE);


        movementVector = transform.forward;
    }

    private void OnEnable()
    {
        shipWheel.onPropellerActive += ReadPropeller;
        shipWheel.onRudderActive += ReadRudder;
        shipWheel.onChangeShipMode += SwitchShipMode;

        DriveTrack.onCheckpointSet += HandleCheckPoint;
    }

    private void OnDisable()
    {
        shipWheel.onPropellerActive -= ReadPropeller;
        shipWheel.onRudderActive -= ReadRudder;
        shipWheel.onChangeShipMode -= SwitchShipMode;

        DriveTrack.onCheckpointSet -= HandleCheckPoint;

    }

    private void FixedUpdate()
    {
        if (!isShipAlive)
            return;

        if (currentMode == SHIPMODE.MANUAL_DRIVE)
            HandlePlayerDriveShip();
        else if (currentMode == SHIPMODE.COMBAT)
            HandleAutoDrive();

 
        if (currentMode == SHIPMODE.IDLE || currentMode == SHIPMODE.MANUAL_DRIVE) {
            propellerPower += -Math.Sign(propellerPower) * shipWheel.GetPropellerSpeedDecay() * Time.deltaTime;
            rudderAngle += -Math.Sign(rudderAngle) * shipWheel.GetRudderAngleDecay() * Time.deltaTime;
        }


        //hmm I just use this for now
        movementVector = Vector3.Lerp(movementVector, transform.forward, Time.deltaTime * 1.2f);
        //Debug
        Debug.DrawLine(transform.position, transform.position + transform.forward * 40f, Color.blue);
        Debug.DrawLine(transform.position, transform.position + movementVector * 40f, Color.red);

        currentVelocity = movementVector * propellerPower;
        Quaternion currentRotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                                                      transform.rotation.eulerAngles.y + angularPower * Time.deltaTime,
                                                      transform.rotation.eulerAngles.z);
        transform.position += currentVelocity * Time.deltaTime;
        transform.rotation = currentRotation;
    }

    private void HandlePlayerDriveShip()
    {

        //foward backwards 
        float propellerAcceleration = shipWheel.GetPropellerAcceleration();
        float propellerDirection = shipWheel.GetPropellerDirection();
        propellerPower += propellerAcceleration * propellerDirection * Time.deltaTime;
        propellerPower = Math.Clamp(propellerPower, shipWheel.GetMaxAsternSpeed(), shipWheel.GetMaxAheadSpeed());

        //prob dont need this when auto drive 
        //propellerPower += -Math.Sign(propellerPower) * shipWheel.GetPropellerSpeedDecay() * Time.deltaTime;


        //Left right 
        float rudderTurnAcceleration = shipWheel.GetRudderAcceleration();
        float rudderDirection = shipWheel.GetRudderDirection();

        rudderAngle += rudderTurnAcceleration * rudderDirection * Time.deltaTime;
        rudderAngle = Math.Clamp(rudderAngle, -shipWheel.GetMaxRudderAngle(), shipWheel.GetMaxRudderAngle());
        //lets try decay in this way first idk if its good enough 
        //rudderAngle += -Math.Sign(rudderAngle) * shipWheel.GetRudderAngleDecay() * Time.deltaTime;

        float kFactor = 0.05f;

        //simplified rudder formula 
        angularPower = propellerPower * rudderAngle * kFactor;

    }

    private void HandleAutoDrive()
    {   

        if (targetCheckpoint == null)
        {
            Debug.Log("NO CHECKPOINT");
            return; 
        }

        Vector3 checkpointPos = targetCheckpoint.transform.position;
        //now how should i do this ??
        Vector3 directionVector = (checkpointPos - transform.position).normalized;

        propellerPower = shipWheel.GetPropellerCruiseSpeed();

        //havnet used this function b4 
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionVector, Vector3.up);

        float rudderTurnAcceleration = shipWheel.GetRudderAcceleration();
        float rudderCruiseSpeed = shipWheel.GetRudderCruiseSpeed();
        angularPower = angleToTarget * rudderCruiseSpeed * Time.deltaTime;
    }

    void ReadPropeller(float power)
    {
        propellerPower = power;


        //shipPropeller.transform.rotation
    }

    void ReadRudder(float angle)
    {
        rudderAngle = angle;

        float kFactor = 0.05f;

        //simplified rudder formula 
        rudderForce = propellerPower * angle * kFactor;


        shipRudder.transform.rotation = Quaternion.Euler(shipRudder.transform.eulerAngles.x,
                                                         angle,
                                                         shipRudder.transform.eulerAngles.z);
    }


    private void SwitchShipMode(SHIPMODE newMode)
    {
        if (newMode == currentMode)
            return;

        currentMode = newMode;

        onShipChangedMode?.Invoke(newMode);
    }



    public void DealDamage(float damage)
    {
        if (DEBUG_invincible)
            return; 

        shipHealth -= damage; 
        if (shipHealth <= 0)
        {
            shipHealth = 0;
            isShipAlive = false;
            onShipDeath?.Invoke();
        }
        float healthPercentage = shipHealth / shipMaxHealth;
        onShipHealthChanged?.Invoke(shipHealth, healthPercentage);

        Debug.Log("SHIP HP: " + shipHealth);
    }


    private void HandleCheckPoint(DriveCheckpoint newCheckpoint)
    {
        targetCheckpoint = newCheckpoint; 
    }

    public void GameSignalResponse(GAMESIGNAL gameSignal)
    {
        Debug.Log("SHIP COMBAT " + gameSignal);

        string signal = gameSignal.ToString();
       
        if (signal.Contains("START"))
        {
            shipCombat.SetGameSignalReference(gameSignal);
            SwitchShipMode(SHIPMODE.COMBAT);
            onAutoDrive = true;
        }
        else if (signal.Contains("END"))
        {
            SHIPMODE newMode = SHIPMODE.MANUAL_DRIVE; 

            if (newMode == SHIPMODE.IDLE)
            {
                SwitchShipMode(SHIPMODE.IDLE);


                ////Hmm slight issue but eh will fix 
                //GameObject player = GameObject.FindGameObjectWithTag("Player");
                //if (player != null)
                //{
                //    player.transform.position = shipWheel.gameObject.transform.position;
                //    player.transform.rotation = shipWheel.gameObject.transform.rotation;
                //}


            }
            else if (newMode == SHIPMODE.MANUAL_DRIVE)
            {
                SwitchShipMode(SHIPMODE.MANUAL_DRIVE);

            }
            onAutoDrive = false;

            ////default to drive for now 
            //SwitchShipMode(SHIPMODE.DRIVE);
        }

    }
        

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerOnShip = true;
            other.gameObject.transform.parent = this.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerOnShip = false;
            other.gameObject.transform.parent = null;
        }
    }

    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }


    public float GetPropellerSpeed()
    {
        return propellerPower;
    }


    public float GetRudderSpeed()
    {
        return angularPower;
    }


    public float DEBUG_GetDistanceToCurrentCheckpoint()
    {
        if (targetCheckpoint == null)
            return 0f;
        else 
            return Vector3.Distance(transform.position, targetCheckpoint.transform.position);
    }

}
