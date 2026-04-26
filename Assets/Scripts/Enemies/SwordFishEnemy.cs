
using UnityEngine;


//Should make this a kinematic and handle the physics myself 

//power of having their own class the managers can be unique lets go might be new strat
public class SwordFishEnemy : Enemy
{
    //Prob should have dynamic flight time 

    [SerializeField] private string shipTargetPointName;

    //not in use rnow 
    //[SerializeField] private float damage;

    [Header("Targetting and Movement")]



    [Header("Movement")]
    [SerializeField] private float flightTime;
    [SerializeField] private float gravityY;

    [SerializeField] private float maxFlightTime;
    [SerializeField] private float minFlightTime;

    [SerializeField] private int numOfHopsMax;
    [SerializeField] private int numOfHopsMin;

    [Header("Targetting")]
    [SerializeField] private float maxTargetDistPercentage;
    [SerializeField] private float minTargetDistPercentage;

    //[SerializeField] private float followPointOffset;


    [SerializeField] private Vector3 followOffsetLocalDirection;


    private Transform endTarget;
    private Transform shipTransform;


    private Vector3 followPoint;
    private Vector3 currentTarget;

    private int numOfHops;
    private int currentNumHops;

    private float desiredFlightTime;

    private float distToTargetPercentage;
    private Vector3 startPosition;

    //new stuff 
    [SerializeField] private Vector3 followPointOffset;

    //none dynamic rb way 
    [SerializeField] private float jumpDistToTargetPercentage; 
    [SerializeField] private float followPointYOffset; 
    private Vector3 currentVelocity;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numOfHops = Random.Range(numOfHopsMin, numOfHopsMax + 1);
        currentNumHops = 0;

        endTarget = GameObject.FindGameObjectWithTag(shipTargetPointName).transform;
        shipTransform = GameObject.FindGameObjectWithTag("Ship").transform;

        ////Somehow I will calc this offset b4 hand i guess 
        //Vector3 rotatedOffset = shipTransform.TransformDirection(shipTransform.right);
        //followPoint = rotatedOffset * followPointOffset;


        if (endTarget == null)
        {
            Debug.Log("INVALID TARGET POINT");
            return;
        }
        //temp 
        isActive = true;
        isAlive = true;
        desiredFlightTime = minFlightTime;
        distToTargetPercentage = 0f;
        startPosition = transform.position;

        //GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        //CalculateNextTargetPoint();
    }



    void Update()
    {

        //Ugly clean up but for now it works, should make it better thanks 21/4 
        if (!isActive)
            return;

        if (!isAlive && gameObject != null)
        {
            isActive = false;
            Destroy(gameObject);
            return;
        }

        //what is this code bruh needa change this 
        Vector3 rotatedOffset = shipTransform.TransformDirection(followOffsetLocalDirection);
        //Vector3 rotatedOffset = shipTransform.TransformDirection(new Vector3(3.5f, 0, 4f));
        //Vector3 rotatedOffset = shipTransform.TransformDirection(followPointOffset);
        Vector3 newFollowPos = rotatedOffset + new Vector3(shipTransform.position.x, endTarget.position.y, shipTransform.position.z) + new Vector3(0, followPointYOffset, 0);
        followPoint = newFollowPos;

        

        Quaternion newRotation = Quaternion.LookRotation(currentVelocity.normalized, Vector3.up);

        //Create a new velocity that rotates the fish movement to the foward facing direction of the follow point
        Vector3 frontalVector = new Vector3(transform.forward.x, 0, transform.forward.z);

        //Vector3 rotatedVector = Vector3.RotateTowards(frontalVector, endTarget.forward, 1.57f, 10f);


        Vector3 rotatedVector = (frontalVector + endTarget.forward) * 5f * Time.deltaTime; 

        Debug.Log("ROT: " + rotatedVector);

        currentVelocity += rotatedVector + new Vector3(0, gravityY, 0) * Time.deltaTime;

        transform.rotation = newRotation;
        transform.position += currentVelocity * Time.deltaTime;

        //Debug.DrawLine(transform.position, currentTarget, Color.red);
        Debug.DrawLine(transform.position, followPoint, Color.red);
    }

    //void Update()
    //{

    //    //Ugly clean up but for now it works, should make it better thanks 21/4 
    //    if (!isActive)
    //        return;

    //    if (!isAlive && gameObject != null)
    //    {
    //        isActive = false;
    //        Destroy(gameObject);
    //        return;
    //    }

    //    Vector3 targetVector = (endTarget.transform.position - transform.position).normalized;
    //    float yRotation = Quaternion.LookRotation(targetVector, Vector3.up).eulerAngles.y;
    //    Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
    //                                                yRotation,
    //                                                transform.rotation.eulerAngles.z);

    //    //what is this code bruh needa change this 
    //    Vector3 rotatedOffset = shipTransform.TransformDirection(followOffsetLocalDirection);
    //    //Vector3 rotatedOffset = shipTransform.TransformDirection(new Vector3(3.5f, 0, 4f));
    //    //Vector3 rotatedOffset = shipTransform.TransformDirection(followPointOffset);


    //    Vector3 newFollowPos = rotatedOffset + new Vector3(shipTransform.position.x, endTarget.position.y, shipTransform.position.z) + new Vector3(0, followPointYOffset, 0);

    //    //Vector3 newFollowPos = new Vector3(shipTransform.position.x, endTarget.position.y, shipTransform.position.z) + new Vector3(0, followPointYOffset, 0);
    //    followPoint = newFollowPos;

    //    //I think would be good to mix the rotations for both 

    //    Vector3 lookDirection = (followPoint - transform.position).normalized;
    //    newRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

    //    //newRotation = shipTransform.rotation;

    //    transform.rotation = newRotation;

    //    currentVelocity += new Vector3(0, gravityY, 0) * Time.deltaTime;
    //    transform.position += currentVelocity * Time.deltaTime;


    //    //Debug.DrawLine(transform.position, currentTarget, Color.red);


    //    Debug.DrawLine(transform.position, followPoint, Color.red);

    //    //Debug.Log("FOLLOW: " + followPoint);
    //}

    public void CalculateNextTargetPoint()
    {
        //Num of hops way with fixed distance increase 
        float additionalDistPercentage = 1.0f / (float)numOfHops;

        distToTargetPercentage += additionalDistPercentage;
        if (distToTargetPercentage > 1.0f || currentNumHops + 1 == numOfHops)
            distToTargetPercentage = 1.0f;

        float distance = (transform.position - followPoint).magnitude;
        currentTarget = Vector3.MoveTowards(transform.position, followPoint, distance * jumpDistToTargetPercentage);
        currentTarget.y = followPoint.y;

        float timeDifference = maxFlightTime - desiredFlightTime;
        float additionalTime = Mathf.Lerp(0, timeDifference, distToTargetPercentage);
        desiredFlightTime += additionalTime;
        currentNumHops++;
        currentVelocity = CalculateForce(currentTarget); 

        //clean up 
        if (currentNumHops == numOfHops)
            isAlive = false;
    }


    //needa make this adapt to make it work for moving targets
    public Vector3 CalculateForce(Vector3 targetPos)
    {
        Vector3 directionToPlayer = (targetPos - transform.position);
        Vector3 normalizeDirection = directionToPlayer.normalized;
        float distanceFromPlayer = directionToPlayer.magnitude;
        float horizontalSpeed = distanceFromPlayer / desiredFlightTime;

        //vy = (deltaY - 0.5*g*t^2) / t     
        float deltaY = targetPos.y - transform.position.y;

        float gravity = Mathf.Abs(gravityY); 
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

        //This might not be the greatest bounce back method but oh well 
        if (other.gameObject.CompareTag("Water"))
        {
            //if (currentNumHops == numOfHops)
            //    return; 

            //TEMP 
            //GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            CalculateNextTargetPoint();
        }


        //coudl prob make this a base class functoin or smth 
        if (other.gameObject.CompareTag("Ship"))
        {
            Debug.Log("HIT SHIP");
            Ship shipRef = other.gameObject.GetComponentInParent<Ship>();
            if (shipRef != null)
            {
                shipRef.DealDamage(enemyData.damage);
                Destroy(gameObject);
            }
        }

    }


}




// old version using the dynmaic rb way lets try kinematic way 
//using UnityEngine;



////Should make this a kinematic and handle the physics myself 

////power of having their own class the managers can be unique lets go might be new strat
//public class SwordFishEnemy : Enemy
//{
//    //Prob should have dynamic flight time 

//    [SerializeField] private string shipTargetPointName;

//    //not in use rnow 
//    [SerializeField] private float damage; 

//    [Header("Targetting and Movement")]



//    [Header("Movement")]
//    [SerializeField] private float flightTime;
//    [SerializeField] private float gravity;

//    [SerializeField] private float maxFlightTime; 
//    [SerializeField] private float minFlightTime; 

//    [SerializeField] private int numOfHopsMax; 
//    [SerializeField] private int numOfHopsMin;

//    [Header("Targetting")]
//    [SerializeField] private float maxTargetDistPercentage;
//    [SerializeField] private float minTargetDistPercentage;

//    [SerializeField] private float followPointOffset; 



//    private Transform endTarget;
//    private Transform shipTransform;


//    private Vector3 followPoint;
//    private Vector3 currentTarget;

//    private int numOfHops;
//    private int currentNumHops;

//    private float desiredFlightTime;

//    private float distToTargetPercentage;
//    private Vector3 startPosition; 



//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        numOfHops = Random.Range(numOfHopsMin, numOfHopsMax + 1);
//        currentNumHops = 0;

//        endTarget = GameObject.FindGameObjectWithTag(shipTargetPointName).transform;
//        shipTransform = GameObject.FindGameObjectWithTag("Ship").transform;

//        //Somehow I will calc this offset b4 hand i guess 
//        Vector3 rotatedOffset = shipTransform.TransformDirection(shipTransform.right);
//        Vector3 newFollowPos = rotatedOffset * followPointOffset;

//        followPoint = newFollowPos; 


//        if (endTarget == null)
//        {
//            Debug.Log("INVALID TARGET POINT");
//            return;
//        }
//        //temp 
//        isActive = true;
//        isAlive = true;
//        desiredFlightTime = minFlightTime;
//        distToTargetPercentage = 0f;
//        startPosition = transform.position;



//        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
//        CalculateNextTargetPoint();
//    }

//    void Update()
//    {

//        //Ugly clean up but for now it works, should make it better thanks 21/4 
//        if (!isActive)
//            return;

//        if (!isAlive && gameObject != null)
//        {
//            isActive = false;
//            Destroy(gameObject);
//            return; 
//        }

//        Vector3 targetVector = (endTarget.transform.position - transform.position).normalized;
//        float yRotation = Quaternion.LookRotation(targetVector, Vector3.up).eulerAngles.y;
//        Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
//                                                    yRotation,
//                                                    transform.rotation.eulerAngles.z);


//        Vector3 rotatedOffset = shipTransform.TransformDirection(new Vector3(3.5f, 0, 1.2f));
//        Vector3 newFollowPos = rotatedOffset * followPointOffset + new Vector3(shipTransform.position.x, endTarget.position.y, shipTransform.position.z);
//        followPoint = newFollowPos;

//        //I think would be good to mix the rotations for both 

//        Vector3 lookDirection = (followPoint - transform.position).normalized;
//        newRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

//        //newRotation = shipTransform.rotation;

//        transform.rotation = newRotation;


//        Debug.DrawLine(transform.position, followPoint, Color.red);

//        //Debug.Log("FOLLOW: " + followPoint);
//    }

//    //Idea is to choose a random point that is at least 40% the distance from 
//    public void CalculateNextTargetPoint()
//    {

//        //randomized distance increase 
//        //float additionalDistPercentage = Random.Range(minTargetDistPercentage, maxTargetDistPercentage);
//        //distToTargetPercentage += additionalDistPercentage;

//        //Num of hops way with fixed distance increase 
//        float additionalDistPercentage = 1.0f / (float)numOfHops;

//        distToTargetPercentage += additionalDistPercentage;
//        if (distToTargetPercentage > 1.0f || currentNumHops + 1 == numOfHops)
//            distToTargetPercentage = 1.0f;


//        //currentTarget = Vector3.Lerp(startPosition, endTarget.transform.position, distToTargetPercentage);

//        currentTarget = Vector3.Lerp(startPosition, followPoint, distToTargetPercentage);

//        float timeDifference = maxFlightTime - desiredFlightTime;
//        float additionalTime = Mathf.Lerp(0, timeDifference, distToTargetPercentage);
//        desiredFlightTime += additionalTime;

//        currentNumHops++;
//        Vector3 flyForce = CalculateForce(currentTarget);

//        //TEMP 
//        GetComponent<Rigidbody>().AddForce(flyForce, ForceMode.Impulse);

//        //clean up 
//        if (currentNumHops == numOfHops)
//        {
//            isAlive = false; 
//        }
//    }

//    public Vector3 CalculateForce(Vector3 targetPos)
//    {
//        Vector3 directionToPlayer = (targetPos - transform.position);
//        Vector3 normalizeDirection = directionToPlayer.normalized;
//        float distanceFromPlayer = directionToPlayer.magnitude;

//        //float horizontalSpeed = distanceFromPlayer / flightTime;
//        float horizontalSpeed = distanceFromPlayer / desiredFlightTime;

//        //vy = (deltaY - 0.5*g*t^2) / t
//        float deltaY = targetPos.y - transform.position.y;

//        float gravity = Mathf.Abs(Physics.gravity.y);
//        //float verticalSpeed = (deltaY + 0.5f * gravity * flightTime * flightTime) / flightTime;
//        float verticalSpeed = (deltaY + 0.5f * gravity * desiredFlightTime * desiredFlightTime) / desiredFlightTime;

//        Vector3 firingForce = new Vector3(
//            normalizeDirection.x * horizontalSpeed,
//            verticalSpeed,
//            normalizeDirection.z * horizontalSpeed
//        );

//        return firingForce;
//    }

//    private void OnTriggerEnter(Collider other)
//    {

//        //This might not be the greatest bounce back method but oh well 
//        if (other.gameObject.CompareTag("Water"))
//        {
//            //if (currentNumHops == numOfHops)
//            //    return; 

//            //TEMP 
//            GetComponent<Rigidbody>().linearVelocity = Vector3.zero; 
//            CalculateNextTargetPoint();
//        }


//        //coudl prob make this a base class functoin or smth 
//        if (other.gameObject.CompareTag("Ship"))
//        {
//            Debug.Log("HIT SHIP");
//            Ship shipRef = other.gameObject.GetComponentInParent<Ship>();
//            if (shipRef != null)
//            {
//                shipRef.DealDamage(damage);
//                Destroy(gameObject);
//            }
//        }

//    }


//}
