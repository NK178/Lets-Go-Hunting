using UnityEngine;


//Should make this a kinematic and handle the physics myself 
public class SwordFishEnemy : Enemy
{
    //Prob should have dynamic flight time 
    [SerializeField] private float flightTime;


    [SerializeField] private float gravity;



    [SerializeField] private float maxFlightTime; 
    [SerializeField] private float minFlightTime; 

    [SerializeField] private int numOfHopsMax; 
    [SerializeField] private int numOfHopsMin;

    [SerializeField] private float maxTargetDistPercentage;
    [SerializeField] private float minTargetDistPercentage;


    public GameObject endTarget;
    private Vector3 currentTarget;

    private int numOfHops;
    private int currentNumHops;

    private float desiredFlightTime;

    private float distToTargetPercentage;
    private Vector3 startPosition; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numOfHops = Random.Range(numOfHopsMin, numOfHopsMax + 1);
        currentNumHops = 0;

        //temp 
        isActive = true;
        isAlive = true;
        desiredFlightTime = minFlightTime;
        distToTargetPercentage = 0f;
        startPosition = transform.position; 
        CalculateNextTargetPoint();
    }

    void Update()
    {
        if (!isActive)
            return;

        if (!isAlive)
            Destroy(gameObject);
    }

    //Idea is to choose a random point that is at least 40% the distance from 
    public void CalculateNextTargetPoint()
    {

        //randomized distance increase 
        //float additionalDistPercentage = Random.Range(minTargetDistPercentage, maxTargetDistPercentage);
        //distToTargetPercentage += additionalDistPercentage;

        //Num of hops way with fixed distance increase 
        float additionalDistPercentage = 1.0f / (float)numOfHops;

        distToTargetPercentage += additionalDistPercentage;
        if (distToTargetPercentage > 1.0f || currentNumHops + 1 == numOfHops)
            distToTargetPercentage = 1.0f;    

        currentTarget = Vector3.Lerp(startPosition, endTarget.transform.position, distToTargetPercentage);
        float timeDifference = maxFlightTime - desiredFlightTime;
        float additionalTime = Mathf.Lerp(0, timeDifference, distToTargetPercentage);
        desiredFlightTime += additionalTime;

        currentNumHops++;
        Vector3 flyForce = CalculateForce(currentTarget);
        //Debug.Log("CALCULATED FORCE: " + flyForce + "FLIGHT TIME: " + desiredFlightTime);

        //TEMP 
        GetComponent<Rigidbody>().AddForce(flyForce, ForceMode.Impulse);
    }

    public Vector3 CalculateForce(Vector3 targetPos)
    {
        Vector3 directionToPlayer = (targetPos - transform.position);
        Vector3 normalizeDirection = directionToPlayer.normalized;
        float distanceFromPlayer = directionToPlayer.magnitude;

        //float horizontalSpeed = distanceFromPlayer / flightTime;
        float horizontalSpeed = distanceFromPlayer / desiredFlightTime;

        //vy = (deltaY - 0.5*g*t^2) / t
        float deltaY = targetPos.y - transform.position.y;

        float gravity = Mathf.Abs(Physics.gravity.y);
        //float verticalSpeed = (deltaY + 0.5f * gravity * flightTime * flightTime) / flightTime;
        float verticalSpeed = (deltaY + 0.5f * gravity * desiredFlightTime * desiredFlightTime) / desiredFlightTime;


        Vector3 firingForce = new Vector3(
            normalizeDirection.x * horizontalSpeed,
            verticalSpeed,
            normalizeDirection.z * horizontalSpeed
        );

        return firingForce;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            //if (currentNumHops == numOfHops)
            //    return; 

            //TEMP 
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero; 
            CalculateNextTargetPoint();
        }
    }

}
