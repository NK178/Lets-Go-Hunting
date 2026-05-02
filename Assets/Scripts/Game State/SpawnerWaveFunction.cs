using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "SpawnerWaveFunction", menuName = "Scriptable Objects/SpawnerWaveFunction")]
public class SpawnerWaveFunction : BaseWaveFunction
{
    [Header("Spawner Settings")]    
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private string shipTargetPointName;
    [SerializeField] private string spawnPointName; 
    [SerializeField] private float waveInterval;

    [SerializeField] private Vector3 spawnDeviationAxisMax;
    [SerializeField] private Vector3 axisSpawnOffset;

    //not used anymore 
    [Range(0, 10f)]
    [SerializeField] private float spawnWidthDeviateRange;

    [Header("Follow Ship Movement")]
    [SerializeField] private bool followShipMovement;
    [SerializeField] private Vector3 followDirectionOffset;

    [SerializeField] private bool projectMovementOnSurface;
    [SerializeField] private string targetSurfaceTag;
    [SerializeField] private string targetSurfaceName;

    [Header("Follow Ship Rotation")]
    [SerializeField] private bool followShipRotation;
    [SerializeField] private Vector3 followRotationOffset; 

    private Transform spawnPoint;
    private Transform shipTransform = null;
    private float startingPosY;

    private Transform targetSurface; 

    public override void Excute(SectionManager sectionManager)
    {
        spawnPoint = sectionManager.transform.Find(spawnPointName);

        sectionManager.StartCoroutine(SpawnerCoroutine(spawnPoint));


        //maybe i can go find the target points instead 
        shipTransform = GameObject.FindGameObjectWithTag("Ship").gameObject.transform;

        startingPosY = spawnPoint.position.y;

        if (projectMovementOnSurface)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(targetSurfaceTag);
            foreach (GameObject obj in objects)
            {
                if (obj.name == targetSurfaceName)
                {
                    targetSurface = obj.transform;
                    break;      
                }
            }
        }
    }

    public override void Exit(SectionManager sectionManager)
    {
        Debug.Log("WAVE OVER");

        //delete all the childs 
        foreach(Transform child in spawnPoint.transform)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
    }

    public override void Process(SectionManager sectionManager)
    {

        if (shipTransform == null)
            return;

        ////Follow ship movement based on rotation i think is good 
        //if (followShipMovement)
        //{
        //    Vector3 localOffset = new Vector3(followDirectionOffset.x, 0, followDirectionOffset.z);

        //    //interesting function 
        //    Vector3 rotatedOffset = shipTransform.TransformDirection(localOffset);

        //    Vector3 newSpawnPosition = rotatedOffset + new Vector3(shipTransform.position.x, startingPosY, shipTransform.position.z);
        //    spawnPoint.position = newSpawnPosition;
        //}


        if (followShipMovement)
        {
            Vector3 newSpawnPosition = Vector3.zero;
            Vector3 positionFromShip = CalculatePositionWithShip();

            //works well lol
            if (projectMovementOnSurface && targetSurface != null)
            {
                Collider surfaceColldier = targetSurface.GetComponent<Collider>();
                newSpawnPosition = surfaceColldier.ClosestPointOnBounds(positionFromShip);
                //Debug.Log("PROJECTION: " + newSpawnPosition);
            }
            else
            {
                newSpawnPosition = positionFromShip;
            }

            spawnPoint.position = newSpawnPosition;
        }


        if (followShipRotation)
        {
            //shld add rotational offsets in the future 
            spawnPoint.rotation = shipTransform.rotation; 
        }
    }


    private Vector3 CalculatePositionWithShip()
    {
        Vector3 newPosition = Vector3.zero;
        Vector3 localOffset = new Vector3(followDirectionOffset.x, 0, followDirectionOffset.z);

        //interesting function 
        Vector3 rotatedOffset = shipTransform.TransformDirection(localOffset);

        newPosition = rotatedOffset + new Vector3(shipTransform.position.x, startingPosY, shipTransform.position.z);
        return newPosition;
    }

    private IEnumerator SpawnerCoroutine(Transform spawnPoint)
    {
        while (true)
        {
            yield return new WaitForSeconds(waveInterval);


            Vector3 xDeviation = spawnPoint.right * Random.Range(-spawnDeviationAxisMax.x, spawnDeviationAxisMax.x);
            Vector3 yDeviation = spawnPoint.up * Random.Range(-spawnDeviationAxisMax.y, spawnDeviationAxisMax.y);
            Vector3 zDeviation = spawnPoint.forward * Random.Range(-spawnDeviationAxisMax.z, spawnDeviationAxisMax.z);


            Vector3 xOffset = spawnPoint.right * axisSpawnOffset.x;
            Vector3 yOffset = spawnPoint.up * axisSpawnOffset.y;
            Vector3 zOffset = spawnPoint.forward * axisSpawnOffset.z;

            Vector3 spawnDeviationVector = xDeviation + yDeviation + zDeviation;
            Vector3 spawnOffsetVector = xOffset + yOffset + zOffset;    

            Vector3 objectSpawnPoint = spawnPoint.position + spawnDeviationVector + spawnOffsetVector;

            //Should optimise this later in some object pool 
            Enemy enemyObject = Instantiate(enemyPrefab, objectSpawnPoint, spawnPoint.rotation);
        }

    }
}
