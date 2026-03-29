using UnityEngine;

public class Ship : MonoBehaviour
{


    [SerializeField] private Vector3 DEBUG_travelDirection;



    [SerializeField] private float DEBUG_waveRockStrength;



    [SerializeField] private ShipWheel shipWheel;
    [SerializeField] private GameObject shipRudder; 
    [SerializeField] private GameObject shipPropeller; 
    [SerializeField] private float moveSpeed;

    [SerializeField] private float angularDamping; 
    [SerializeField] private float maxAngularPower;


    [SerializeField] private float propellerRotationDamping;
    [SerializeField] private float maxPropellerRotateSpeed; 


    private float propellerPower;
    private float rudderAngle;
    private float rudderForce; 

    private Vector3 currentVelocity;
    private Vector3 movementVector; 
    private Vector3 rotationVector; 

    private float angularPower; 
    private bool isPlayerOnShip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPlayerOnShip = false; 
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
        currentVelocity = transform.forward * propellerPower;
        //movementVector = transform.forward * propellerPower;


        angularPower = Mathf.Lerp(angularPower, rudderForce, angularDamping * Time.deltaTime);
        angularPower = Mathf.Clamp(angularPower, -maxAngularPower, maxAngularPower);

        Debug.Log("RUDDER POWER: " + rudderForce + "ANGULAR: " + angularPower);
         


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
}
