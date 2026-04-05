using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PhaseToPosition
{
    public GAMESIGNAL signal;
    public bool isLeft; 
}

public class Ship : MonoBehaviour
{

    [SerializeField] private Vector3 DEBUG_travelDirection;
    [SerializeField] private float DEBUG_waveRockStrength;



    [SerializeField] private ShipWheel shipWheel;
    [SerializeField] private GameObject shipRudder; 
    [SerializeField] private GameObject shipPropeller;


    //CONSIDER SPLITTING TO ANOTHER CLASS 
    [Header("Ship Combat")]
    [SerializeField] private GameObject starboardShootPoint; 
    [SerializeField] private GameObject portShootPoint; 
    [SerializeField] private float shipMaxHealth;

    [SerializeField] private List<PhaseToPosition> phaseToShootPositions;

    [Header("Ship Controls")]
    [SerializeField] private float moveSpeed;

    [SerializeField] private float angularDamping; 
    [SerializeField] private float maxAngularPower;


    [SerializeField] private float propellerRotationDamping;
    [SerializeField] private float maxPropellerRotateSpeed;

    public static Action onShipDeath;
    public static Action<float, float> onShipHealthChanged;
    public static Action<bool> onShipIsCombatMode;
    public static Action<Transform> onShipLockTransform; 


    private float propellerPower;
    private float rudderAngle;
    private float rudderForce;
    private float shipHealth;

    private Vector3 currentVelocity;
    private Vector3 movementVector; 
    private Vector3 rotationVector; 

    private float angularPower; 
    private bool isPlayerOnShip;

    private bool isShipAlive;


    //CONSIDER MOVING TO ANOTHER CLASS

    private bool isInCombatMode = false;


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
    }

    private void OnEnable()
    {
        shipWheel.onPropellerActive += ReadPropeller;
        shipWheel.onRudderActive += ReadRudder;
    }

    private void OnDisable()
    {
        shipWheel.onPropellerActive -= ReadPropeller;
        shipWheel.onRudderActive -= ReadRudder;

    }


    [SerializeField] float waterResistance = 10f; // How hard the water fights the turn


    private void FixedUpdate()
    {
        if (!isShipAlive)
            return; 

        currentVelocity = transform.forward * propellerPower;
        //movementVector = transform.forward * propellerPower;


        angularPower = Mathf.Lerp(angularPower, rudderForce, angularDamping * Time.deltaTime);
        angularPower = Mathf.Clamp(angularPower, -maxAngularPower, maxAngularPower);

        //Debug.Log("RUDDER POWER: " + rudderForce + "ANGULAR: " + angularPower);
         


        transform.position += currentVelocity * Time.deltaTime;
        transform.Rotate(0, rudderForce * Time.deltaTime, 0);
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

    public void DealDamage(float damage)
    {
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


    //Consider spiltting  to another class this thingy 
    public void EnterShipCombatMode(GAMESIGNAL gamePhase)
    {
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
        isInCombatMode = true;

        onShipLockTransform?.Invoke(targetPos.transform);
        onShipIsCombatMode?.Invoke(true);


    }
}
