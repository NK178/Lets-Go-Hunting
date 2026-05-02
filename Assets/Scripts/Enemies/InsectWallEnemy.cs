using System.Collections.Generic;
using UnityEngine;

//basically my boi will instanly find the wall and cling to it 
public class InsectWallEnemy : Enemy
{
    [Header("Movement")]
    [SerializeField] private float movementFactor;

    [SerializeField] private float fastMoveSpeed; 
    [SerializeField] private float slowMoveSpeed;
    [SerializeField] private float catchUpDistance; 

    [Header("Targetting")]
    [SerializeField] private string shipStarboardName;
    [SerializeField] private string shipPortName;
    [SerializeField] private Vector3 followOffset;

    [Header("Wall Mechanics")]
    [SerializeField] private string clingWallTagName;
    [SerializeField] private LayerMask clingWallLayer;
    [SerializeField] private float raycastDistance;
    [SerializeField] private float surfaceOffset;

    [Header("Attack Mechanics")]
    [SerializeField] private float maxAttackWaitTime;
    [SerializeField] private float minAttackWaitTime;
    [SerializeField] private float attackLeapFlightTime;
    [SerializeField] private float leapGravity;


    [Header("Boid Behaviour")]
    [SerializeField] private float boidDetectionRadius; 
    [SerializeField] private float seperationDistance;
    [SerializeField] private float seperationStrength;

    [SerializeField] private float alignmentDistance;
    [SerializeField] private float alignmentStrengthFactor;



    private float attackWaitTime;
    private bool shouldAttack;
    private bool hasAttacked; 

    private Transform endTarget;
    private Transform shipTransform;
    private Ship shipRef;
    private GameObject wallRef;

    private Vector3 currentVelocity; 

    private Vector3 worldFollowPoint;
    private Vector3 wallFollowPoint;

    private Vector3 wallNormal;
    private Vector3 wallHitPoint;
    private Vector3 wallPerpenNormal;

    private bool shouldCatchUp;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //double raycast to find the wall 
        bool wallFound = false;
        bool isRightWall = false;
        RaycastHit hitInfo; 
        if (Physics.Raycast(transform.position, transform.right, out hitInfo, raycastDistance, clingWallLayer))
            wallFound = isRightWall = true;

        if (!wallFound)
        {
            if (Physics.Raycast(transform.position, -transform.right, out hitInfo, raycastDistance, clingWallLayer))
                wallFound = true;
        }

        if (wallFound && hitInfo.collider != null)
        {
            Debug.Log("WALL FOUND");
            transform.rotation = Quaternion.FromToRotation(transform.up, hitInfo.normal);
            Vector3 offsetVector = transform.up * surfaceOffset; 
            transform.position = hitInfo.point + offsetVector;
            wallNormal = hitInfo.normal;
            wallHitPoint = hitInfo.point;


            //find this vector 
            //wallPerpenNormal = Vector3.Cross(wallNormal, hitInfo.collider.transform.forward);
        }

        if (!wallFound)
        {
            Debug.Log("WALL NOT FOUND");
            return; 
        }

        string endTargetName = string.Empty;

        //Do some left right logic here 
        if (isRightWall)
            endTargetName = shipStarboardName;
        else
            endTargetName = shipPortName;

        GameObject[] shipTargetPoints = GameObject.FindGameObjectsWithTag("ShipPoint");
        foreach (GameObject targetPt in shipTargetPoints)
        {
            string pointName = targetPt.name;
            if (pointName.Contains(endTargetName))
            {
                Debug.Log("END TARGET: " + pointName);
                endTarget = targetPt.transform;
                break;
            }
        }

        attackWaitTime = Random.Range(minAttackWaitTime, maxAttackWaitTime);
        shouldCatchUp = true;
        shipTransform = GameObject.FindGameObjectWithTag("Ship").transform;
        shipRef = shipTransform.GetComponent<Ship>();

        isActive = true;
        isAlive = true;
        hasAttacked = false;
        shouldAttack = false; 

    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
            return;

        if (!isAlive && gameObject != null)
        {
            isActive = false;
            Destroy(gameObject);
            return;
        }

        //run down timer when in range
        if (!shouldCatchUp && !shouldAttack)
        {
            attackWaitTime -= Time.deltaTime;
        }
        if (attackWaitTime < 0)
            shouldAttack = true;

        //Debug.Log("ATTACK TIME: " + attackWaitTime);

        //normal normal movement 
        if (shouldAttack)
        {
            if (!hasAttacked)
                currentVelocity = HandleLeapAttack();
            currentVelocity.y += leapGravity * Time.deltaTime; 

            transform.rotation = Quaternion.LookRotation(currentVelocity.normalized);  

            if (animator != null)
                animator.enabled = false;
        }
        else
        {
            RaycastHit[] hitTargets = Physics.SphereCastAll(transform.position, boidDetectionRadius, transform.up);
            List<GameObject> otherBoids = new List<GameObject>();
            foreach (RaycastHit target in hitTargets)
            {
                GameObject targetObject = target.collider.gameObject;
                //Include all bugs that arent in the middle of attacking 
                if (targetObject.TryGetComponent<InsectWallEnemy>(out InsectWallEnemy enemy))
                {
                    if (!enemy.IsAttacking())
                        otherBoids.Add(targetObject);
                }
            }

            Vector3 seperationVector = BoidSeperation(otherBoids);
            //Vector3 alignmentVector = BoidAlignment(otherBoids); 
            //currentVelocity = HandleWallMovement() + seperationVector + alignmentVector;


            currentVelocity = HandleWallMovement() + seperationVector;
            //Debug.Log("alignment: " + alignmentVector);
        }

        transform.position += currentVelocity * Time.deltaTime;

        Debug.DrawLine(transform.position, wallFollowPoint, Color.red);
        Debug.DrawLine(transform.position, worldFollowPoint, Color.yellow);

        //Debug.DrawLine(transform.position, transform.position + transform.right * raycastDistance, Color.red);
        //Debug.DrawLine(transform.position, transform.position + -transform.right * raycastDistance, Color.red);
    }



    private Vector3 BoidSeperation(List<GameObject> boids)
    {
        if (boids.Count == 0)
            return Vector3.zero;

        Vector3 resultingVector = Vector3.zero;

        float sqrSeperationDistance = seperationDistance * seperationDistance;
        foreach (GameObject boid in boids)
        {
            Vector3 direction = (boid.transform.position - transform.position).normalized;
            float sqrDistance = (boid.transform.position - transform.position).sqrMagnitude;

            if (sqrDistance < sqrSeperationDistance)
            {
                resultingVector += -direction * seperationStrength;
                float proximityMultiplier = 1.0f - (Mathf.Sqrt(sqrDistance) / seperationDistance);
                resultingVector += direction * (seperationStrength * proximityMultiplier);
            }
        }

        resultingVector = Vector3.ProjectOnPlane(resultingVector, wallNormal);
        return resultingVector;
    }




    //private Vector3 BoidSeperation(List<GameObject> boids)
    //{
    //    if (boids.Count == 0)    
    //        return Vector3.zero;    

    //    Vector3 resultingVector = Vector3.zero;

    //    foreach (GameObject boid in boids) {
    //        Vector3 direction = (boid.transform.position - transform.position).normalized;
    //        float distance = (boid.transform.position - transform.position).sqrMagnitude;

    //        if (distance < seperationDistance * seperationDistance)
    //        {
    //            //resultingVector += -direction * seperationStrength * Time.deltaTime;

    //            resultingVector += -direction * seperationStrength;
    //        }
    //    }

    //    resultingVector = Vector3.ProjectOnPlane(resultingVector, wallNormal);
    //    return resultingVector;
    //}

    private Vector3 BoidAlignment(List<GameObject> boids)
    {
        if (boids.Count == 0)
            return Vector3.zero;
        Vector3 resultingVector = Vector3.zero;

        float sqrAlignmentDistance = seperationDistance * seperationDistance;

        int boidInRange = 0;

        Vector3 averageVelocity = Vector3.zero;
        foreach (GameObject boid in boids)
        {
            float sqrDistance = (boid.transform.position - transform.position).sqrMagnitude;

            if (sqrDistance < sqrAlignmentDistance)
            {
                averageVelocity += boid.GetComponent<InsectWallEnemy>().GetCurrentVelocity();
                boidInRange++;
            }
        }

        if (boidInRange == 0)
            return resultingVector;

        averageVelocity /= boidInRange; 

        resultingVector = Vector3.ProjectOnPlane(averageVelocity, wallNormal);
        return resultingVector;
    }

    private Vector3 HandleWallMovement()
    {
        Vector3 resultingVelocity = Vector3.zero;

        worldFollowPoint = shipTransform.position + followOffset;

        Vector3 vectorToFollowPoint = worldFollowPoint - wallHitPoint;
        float distToFollowPoint = Vector3.Dot(vectorToFollowPoint, wallNormal);

        wallFollowPoint = worldFollowPoint - (wallNormal * distToFollowPoint);

        Vector3 followPointVector = wallFollowPoint - transform.position;

        float currentMoveSpeed = slowMoveSpeed;

        float distance = followPointVector.magnitude;
        if (distance > catchUpDistance)
            shouldCatchUp = true;

        float factor = 10f;
        if (distance < factor)
            shouldCatchUp = false;

        if (shouldCatchUp)
            currentMoveSpeed =  fastMoveSpeed;

        resultingVelocity = followPointVector.normalized * currentMoveSpeed;

        return resultingVelocity; 
    }

    private Vector3 HandleLeapAttack()
    {

        Debug.Log("LEAPING");
        Vector3 resultingVector = Vector3.zero;
        Vector3 shipVelocity = shipRef.GetCurrentVelocity();
        resultingVector = CalculateForce(endTarget.transform.position, shipVelocity);
        hasAttacked = true; 
        return resultingVector;
    }

    private float CalculateBaseMovementSpeed(Vector3 followVector)
    {
        float distance = followVector.magnitude;

        print("DIST: " + distance);
        if (distance > catchUpDistance)
            shouldCatchUp = true;

        float factor = 10f;
        if (distance < factor)
            shouldCatchUp = false;

        if (shouldCatchUp)
            return fastMoveSpeed;
        else
            return slowMoveSpeed; 
    }


    public Vector3 CalculateForce(Vector3 targetPos, Vector3 targetVelocity)
    {
        Vector3 predictedPos = targetPos + (targetVelocity * attackLeapFlightTime);

        Vector3 directionToTarget = (predictedPos - transform.position);
        float horizontalDistance = new Vector3(directionToTarget.x, 0, directionToTarget.z).magnitude;

        float horizontalSpeed = horizontalDistance / attackLeapFlightTime;

        float deltaY = predictedPos.y - transform.position.y;
        float gravity = Mathf.Abs(leapGravity);

        // Standard projectile motion formula for initial vertical velocity
        float verticalSpeed = (deltaY + 0.5f * gravity * attackLeapFlightTime * attackLeapFlightTime) / attackLeapFlightTime;

        Vector3 normalizeHorizontal = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;

        return new Vector3(
            normalizeHorizontal.x * horizontalSpeed,
            verticalSpeed,
            normalizeHorizontal.z * horizontalSpeed
        );
    }


 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == clingWallTagName)
        {
            Debug.Log("FOUND WALL");

            wallRef = other.gameObject; 
        }

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


    public bool IsAttacking()
    {
        return hasAttacked;
    }

    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity; 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        // Draw the starting sphere
        Gizmos.DrawWireSphere(transform.position, boidDetectionRadius);

        // Draw the cast path (optional: change '10f' to your desired visual distance)
        Vector3 endPoint = transform.position + (transform.up * 1f);
        Gizmos.DrawLine(transform.position, endPoint);

        //// Draw the end of the visual range
        //Gizmos.DrawWireSphere(endPoint, boidDetectionRadius);
    }
}
