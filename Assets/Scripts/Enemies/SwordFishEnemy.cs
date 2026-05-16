using UnityEngine;


//power of having their own class the managers can be unique lets go might be new strat
public class SwordFishEnemy : Enemy
{
    [Header("Movement")]
    [SerializeField] private float gravityY;

    [SerializeField] private float maxFlightTime;
    [SerializeField] private float minFlightTime;

    [SerializeField] private int numOfHopsMax;
    [SerializeField] private int numOfHopsMin;

    [Header("Targetting")]
    [SerializeField] private string shipTargetPointName;
    [SerializeField] private string shipTargetSideName;
    [SerializeField] private Vector3 followOffsetLocalDirection;
    [SerializeField] private float jumpDistToTargetPercentage;
    [SerializeField] private float followPointYOffset;
    [SerializeField] private float steeringSensitivity;

    private float desiredFlightTime;

    private Transform endTarget;
    private Transform shipTransform;
    private Ship shipRef;

    private Vector3 followPoint;
    private Vector3 currentTarget;

    private int numOfHops;
    private int currentNumHops;

    private Vector3 currentVelocity;
    private float flightTimeIncrement;


    //[SerializeField] private float flightTime;
    //[SerializeField] private float maxTargetDistPercentage;
    //[SerializeField] private float minTargetDistPercentage;

    //[SerializeField] private float followPointOffset;

    //private float distToTargetPercentage;
    //private Vector3 startPosition;

    //new stuff 
    //[SerializeField] private Vector3 followPointOffset;

    //none dynamic rb way 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numOfHops = Random.Range(numOfHopsMin, numOfHopsMax + 1);
        currentNumHops = 0;

        GameObject[] shipTargetPoints = GameObject.FindGameObjectsWithTag("ShipPoint");
        foreach (GameObject targetPt in shipTargetPoints)
        {
            string pointName = targetPt.name;
            if (pointName.Contains(shipTargetSideName))
            {
                Debug.Log("END TARGET: " + pointName);
                endTarget = targetPt.transform;
                break; 
            }
        }

        //endTarget = GameObject.FindGameObjectWithTag(shipTargetPointName).transform;
        shipTransform = GameObject.FindGameObjectWithTag("Ship").transform;
        shipRef = shipTransform.GetComponent<Ship>();

        if (endTarget == null)
        {
            Debug.Log("INVALID TARGET POINT");
            return;
        }

        isActive = true;
        isAlive = true;
        desiredFlightTime = minFlightTime;
        flightTimeIncrement = 1.0f / (float)numOfHops;
    }


    //Not bad at alll 
    void FixedUpdate()
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
        followPoint = shipTransform.position + rotatedOffset;
        followPoint.y += followPointYOffset;

        if (currentNumHops + 1 < numOfHops)
        {
            Vector3 directionToTarget = (followPoint - transform.position).normalized;
            currentVelocity = Vector3.Lerp(currentVelocity.normalized, directionToTarget, steeringSensitivity * Time.deltaTime) * currentVelocity.magnitude;
        }

        currentVelocity.y += gravityY * Time.deltaTime;
        transform.position += currentVelocity * Time.deltaTime;

        if (currentNumHops + 1 < numOfHops)
        {
            if (currentVelocity.sqrMagnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentVelocity.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(currentVelocity.normalized);

        }


        Debug.DrawLine(transform.position, transform.position + currentVelocity.normalized * 5f, Color.yellow);
        Debug.DrawLine(transform.position, currentTarget, Color.red);
    }


    ////Not bad at alll 
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

    //    //what is this code bruh needa change this 
    //    Vector3 rotatedOffset = shipTransform.TransformDirection(followOffsetLocalDirection);
    //    followPoint = shipTransform.position + rotatedOffset;
    //    followPoint.y += followPointYOffset;

    //    if (currentNumHops + 1 < numOfHops)
    //    {
    //        Vector3 directionToTarget = (followPoint - transform.position).normalized;
    //        currentVelocity = Vector3.Lerp(currentVelocity.normalized, directionToTarget, steeringSensitivity * Time.deltaTime) * currentVelocity.magnitude;
    //    }

    //    currentVelocity.y += gravityY * Time.deltaTime;
    //    transform.position += currentVelocity * Time.deltaTime;

    //    if (currentNumHops + 1 < numOfHops)
    //    {
    //        if (currentVelocity.sqrMagnitude > 0.1f)
    //        {
    //            Quaternion targetRotation = Quaternion.LookRotation(currentVelocity.normalized, Vector3.up);
    //            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    //        }
    //    }
    //    else
    //    {
    //        transform.rotation = Quaternion.LookRotation(currentVelocity.normalized);

    //    }


    //    Debug.DrawLine(transform.position, transform.position + currentVelocity.normalized * 5f, Color.yellow);
    //    Debug.DrawLine(transform.position, currentTarget, Color.red);
    //}

    public void CalculateNextTargetPoint()
    {
        desiredFlightTime += flightTimeIncrement;
        if (desiredFlightTime > maxFlightTime)
            desiredFlightTime = maxFlightTime;

        currentNumHops++;


        //jump to ship 
        if (currentNumHops + 1 == numOfHops)
        {
            Debug.Log("JUMPING TOWARDS SHIP");
            currentTarget = endTarget.position;
        }
        else
        {
            float distance = (transform.position - followPoint).magnitude;
            currentTarget = Vector3.MoveTowards(transform.position, followPoint, distance * jumpDistToTargetPercentage);
            currentTarget.y = followPoint.y;
        }


        currentVelocity = CalculateForce(currentTarget, shipRef.GetCurrentVelocity());

        //clean up 
        if (currentNumHops == numOfHops)
            isAlive = false;
    }

    //Good enough for now
    public Vector3 CalculateForce(Vector3 targetPos, Vector3 targetVelocity)
    {
        Vector3 predictedPos = targetPos + (targetVelocity * desiredFlightTime);

        Vector3 directionToTarget = (predictedPos - transform.position);
        float horizontalDistance = new Vector3(directionToTarget.x, 0, directionToTarget.z).magnitude;

        float horizontalSpeed = horizontalDistance / desiredFlightTime;

        float deltaY = predictedPos.y - transform.position.y;
        float gravity = Mathf.Abs(gravityY);

        // Standard projectile motion formula for initial vertical velocity
        float verticalSpeed = (deltaY + 0.5f * gravity * desiredFlightTime * desiredFlightTime) / desiredFlightTime;

        Vector3 normalizeHorizontal = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;

        return new Vector3(
            normalizeHorizontal.x * horizontalSpeed,
            verticalSpeed,
            normalizeHorizontal.z * horizontalSpeed
        );
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

    //    //what is this code bruh needa change this 
    //    Vector3 rotatedOffset = shipTransform.TransformDirection(followOffsetLocalDirection);
    //    //Vector3 rotatedOffset = shipTransform.TransformDirection(new Vector3(3.5f, 0, 4f));
    //    //Vector3 rotatedOffset = shipTransform.TransformDirection(followPointOffset);
    //    Vector3 newFollowPos = rotatedOffset + new Vector3(shipTransform.position.x, endTarget.position.y, shipTransform.position.z) + new Vector3(0, followPointYOffset, 0);
    //    followPoint = newFollowPos;

    //    Quaternion newRotation = Quaternion.LookRotation(currentVelocity.normalized, Vector3.up);

    //    //Create a new velocity that rotates the fish movement to the foward facing direction of the follow point
    //    Vector3 frontalVector = new Vector3(transform.forward.x, 0, transform.forward.z);

    //    Vector3 targetDirection = (followPoint - transform.position).normalized;
    //    targetDirection.y = 0;


    //    ///28/4 THIS THINGY KINDA WORKS NEEDA IMPROVE IT, IF ONLY I COULD GET THE ROTATIONAL VALUES ANDNOT ADD ANY EXTRA STUFF TO IT 
    //    // 1. Calculate the direction toward the target point (Horizontal only)
    //    //Vector3 targetDirection = (followPoint - transform.position);
    //    targetDirection.y = 0;
    //    targetDirection = targetDirection.normalized;

    //    // 2. Get your current horizontal heading
    //    Vector3 currentHeading = currentVelocity;
    //    float originalSpeed = currentHeading.magnitude;
    //    currentHeading.y = 0;
    //    currentHeading = currentHeading.normalized;

    //    Vector3 steeringVelocity = Vector3.zero;

    //    // 3. Slerp the direction (The 3rd parameter is the "Turn Speed")
    //    // If targetDirection is zero (you're on top of it), Slerp might flicker, so check magnitude
    //    if (targetDirection.sqrMagnitude > 0.001f)
    //    {
    //        float turnSpeed = 2f; // Adjust this: Higher = sharper turns
    //        Vector3 steeredDirection = Vector3.Slerp(currentHeading, targetDirection, turnSpeed * Time.deltaTime);

    //        // 4. Reconstruct velocity: (New Direction * Original Speed) + Gravity
    //        steeringVelocity = new Vector3(steeredDirection.x * originalSpeed, 0, steeredDirection.z * originalSpeed);
    //    }


    //    Vector3 finalVelocity = steeringVelocity + currentVelocity;
    //    // 5. Apply Gravity and Move
    //    currentVelocity.y += gravityY * Time.deltaTime;
    //    transform.position += finalVelocity * Time.deltaTime;

    //    // 6. Update Rotation to look where it's moving
    //    if (currentVelocity.sqrMagnitude > 0.1f)
    //    {
    //        transform.rotation = Quaternion.LookRotation(currentVelocity.normalized, Vector3.up);
    //    }

    //    Debug.DrawLine(transform.position, transform.position + frontalVector * 5f, Color.yellow);

    //    ////Vector3 rotatedVector = Vector3.RotateTowards(frontalVector, endTarget.forward, 1.57f, 10f);

    //    //Vector3 rotatedVector = (targetDirection - frontalVector) * 5f * Time.deltaTime; 


    //    ////Vector3 rotatedVector = (frontalVector + endTarget.forward) * 5f * Time.deltaTime; 

    //    //Debug.Log("ROT: " + rotatedVector);

    //    //currentVelocity += new Vector3(0, gravityY, 0) * Time.deltaTime;
    //    ////currentVelocity += new Vector3(0, gravityY, 0) * Time.deltaTime;

    //    //transform.rotation = newRotation;
    //    //transform.position += currentVelocity * Time.deltaTime;

    //    //Debug.DrawLine(transform.position, currentTarget, Color.red);
    //    Debug.DrawLine(transform.position, followPoint, Color.red);
    //}

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



    ////needa make this adapt to make it work for moving targets
    //public Vector3 CalculateForce(Vector3 targetPos)
    //{
    //    Vector3 directionToPlayer = (targetPos - transform.position);
    //    Vector3 normalizeDirection = directionToPlayer.normalized;
    //    float distanceFromPlayer = directionToPlayer.magnitude;
    //    float horizontalSpeed = distanceFromPlayer / desiredFlightTime;

    //    //vy = (deltaY - 0.5*g*t^2) / t     
    //    float deltaY = targetPos.y - transform.position.y;

    //    float gravity = Mathf.Abs(gravityY); 
    //    float verticalSpeed = (deltaY + 0.5f * gravity * desiredFlightTime * desiredFlightTime) / desiredFlightTime;

    //    Vector3 firingForce = new Vector3(
    //        normalizeDirection.x * horizontalSpeed,
    //        verticalSpeed,
    //        normalizeDirection.z * horizontalSpeed
    //    );

    //    return firingForce;
    //}

}




