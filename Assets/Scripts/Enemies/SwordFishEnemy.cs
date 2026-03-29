using System.Collections;
using UnityEngine;


//Should make this a kinematic and handle the physics myself 
public class SwordFishEnemy : Enemy
{
    //Prob should have dynamic flight time 
    [SerializeField] private float flightTime;
    [SerializeField] private float gravity; 

    public GameObject endTarget;
    private Vector3 currentTarget; 

    [SerializeField] private int numOfHopsMax; 
    [SerializeField] private int numOfHopsMin; 
    private int numOfHops;
    private int currentNumHops;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numOfHops = Random.Range(numOfHopsMin, numOfHopsMax + 1);
        currentNumHops = 0;

        //temp 
        isActive = true;
        isAlive = true;

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
        float distancePercentage = 0.4f;

        //only if not last hop
        if (currentNumHops + 1 == numOfHops)
            currentTarget = endTarget.transform.position;
        else
            currentTarget = Vector3.Lerp(transform.position, endTarget.transform.position, distancePercentage); 

        currentNumHops++;

        Vector3 flyForce = CalculateForce(currentTarget);
        Debug.Log("CALCULATED FORCE: " + flyForce);

        //TEMP 
        GetComponent<Rigidbody>().AddForce(flyForce,ForceMode.Impulse);
    }

    public Vector3 CalculateForce(Vector3 targetPos)
    {
        Vector3 directionToPlayer = (targetPos - transform.position);
        Vector3 normalizeDirection = directionToPlayer.normalized;
        float distanceFromPlayer = directionToPlayer.magnitude;

        float horizontalSpeed = distanceFromPlayer / flightTime;

        //vy = (deltaY - 0.5*g*t^2) / t
        float deltaY = targetPos.y - transform.position.y;

        float gravity = Mathf.Abs(Physics.gravity.y);
        float verticalSpeed = (deltaY + 0.5f * gravity * flightTime * flightTime) / flightTime;

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
            //TEMP 
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero; 
            CalculateNextTargetPoint();
        }
    }

}
