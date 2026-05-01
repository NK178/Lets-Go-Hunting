using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Hierarchy;
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
            //Debug.Log("WALL FOUND");
            transform.rotation = Quaternion.FromToRotation(transform.up, hitInfo.normal);
            Vector3 offsetVector = transform.up * surfaceOffset; 
            transform.position = hitInfo.point + offsetVector;
            wallNormal = hitInfo.normal;
            wallHitPoint = hitInfo.point;   
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

        Debug.Log("ATTACK TIME: " + attackWaitTime);

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
            //Physics.SphereCast()
            currentVelocity = HandleWallMovement();
        }

        transform.position += currentVelocity * Time.deltaTime;

        Debug.DrawLine(transform.position, wallFollowPoint, Color.red);
        Debug.DrawLine(transform.position, worldFollowPoint, Color.yellow);

        //Debug.DrawLine(transform.position, transform.position + transform.right * raycastDistance, Color.red);
        //Debug.DrawLine(transform.position, transform.position + -transform.right * raycastDistance, Color.red);
    }


    private Vector3 BoidSeperation(GameObject[] boids)
    {
        Vector3 resultingVector = Vector3.zero;

        float factor = 2f;

        foreach (GameObject boid in boids) {
            Vector3 direction = (boid.transform.position - transform.position).normalized;
            float distance = (boid.transform.position - transform.position).sqrMagnitude;

            if (distance < seperationDistance * seperationDistance)
            {
                resultingVector += -direction * factor;
            }
        }
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
    }
}
