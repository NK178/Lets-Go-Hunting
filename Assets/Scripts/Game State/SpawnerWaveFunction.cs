using System.Collections;
using UnityEngine;

// 5/7 I can make this multi dimensional, spawn both boss and wave stuff 
[CreateAssetMenu(fileName = "SpawnerWaveFunction", menuName = "Scriptable Objects/SpawnerWaveFunction")]
public class SpawnerWaveFunction : BaseWaveFunction
{
    [Header("Spawner Settings")]    
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private string shipTargetPointName;
    [SerializeField] private string spawnPointName;
    //[SerializeField] private float waveInterval;
    [SerializeField] private float waveIntervalMax;
    [SerializeField] private float waveIntervalMin;
    [SerializeField] private float startingDelay = 0;
    [SerializeField] private int spawnDensityMax;
    [SerializeField] private int spawnDensityMin;


    [Header("Spawned Transform")]
    [SerializeField] private Vector3 spawnDeviationAxisMax;
    [SerializeField] private Vector3 axisSpawnOffset;

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
    private bool isCoroutineRunning = false;

    private float waveInterval; 

    //Fish = 1
    //Insect = 0.8

    public override void Excute(SectionManager sectionManager)
    {
        spawnPoint = sectionManager.transform.Find(spawnPointName);

        sectionManager.StartCoroutine(BeginSpawnCoroutine(sectionManager));



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

                    //match the rotation as well hm 
                    spawnPoint.transform.rotation = targetSurface.rotation;
                    break;      
                }
            }
        }

        waveInterval = Random.Range(waveIntervalMin, waveIntervalMax);
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

        if (followShipMovement)
        {
            Vector3 newSpawnPosition = Vector3.zero;
            Vector3 positionFromShip = CalculatePositionWithShip();

            //works well lol
            if (projectMovementOnSurface && targetSurface != null)
            {
                Collider surfaceColldier = targetSurface.GetComponent<Collider>();
                newSpawnPosition = surfaceColldier.ClosestPoint(positionFromShip);
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


    private IEnumerator BeginSpawnCoroutine(SectionManager sectionManager)
    {
        yield return new WaitForSeconds(startingDelay);
        isCoroutineRunning = true; 
        sectionManager.StartCoroutine(SpawnerCoroutine(spawnPoint));
    }

    private IEnumerator SpawnerCoroutine(Transform spawnPoint)
    {
        while (isCoroutineRunning)
        {
            waveInterval = Random.Range(waveIntervalMin, waveIntervalMax);
            yield return new WaitForSeconds(waveInterval);

            Vector3 xOffset = spawnPoint.right * axisSpawnOffset.x;
            Vector3 yOffset = spawnPoint.up * axisSpawnOffset.y;
            Vector3 zOffset = spawnPoint.forward * axisSpawnOffset.z;
            Vector3 spawnOffsetVector = xOffset + yOffset + zOffset;

            int spawnDensity = Random.Range(spawnDensityMin, spawnDensityMax + 1);

            Vector3 xDeviation = spawnPoint.right * Random.Range(-spawnDeviationAxisMax.x, spawnDeviationAxisMax.x);
            Vector3 yDeviation = spawnPoint.up * Random.Range(-spawnDeviationAxisMax.y, spawnDeviationAxisMax.y);
            Vector3 zDeviation = spawnPoint.forward * Random.Range(-spawnDeviationAxisMax.z, spawnDeviationAxisMax.z);

            for (int i = 0; i < spawnDensity; i++)
            {
                float randomSpawnDelay = Random.Range(0f, 0.2f);

                //This will work for now 
                //Deviation between enemies spawned together in the same batch 
                float localDeviationFactor = 1.2f;  
                Vector3 spawnDeviationVector = (xDeviation + yDeviation + zDeviation) * localDeviationFactor;
                Vector3 objectSpawnPoint = spawnPoint.position + spawnDeviationVector + spawnOffsetVector;

                //Should optimise this later in some object pool 
                Enemy enemyObject = Instantiate(enemyPrefab, objectSpawnPoint, spawnPoint.rotation);

                yield return new WaitForSeconds(randomSpawnDelay); 
            }

            //For one time spawning, stop coroutine immedietly if wave interval = 0 
            if (waveInterval == 0)
                isCoroutineRunning = false;
        }

    }



    //private IEnumerator SpawnerCoroutine(Transform spawnPoint)
    //{
    //    while (isCoroutineRunning)
    //    {
    //        yield return new WaitForSeconds(waveInterval);

    //        Vector3 xOffset = spawnPoint.right * axisSpawnOffset.x;
    //        Vector3 yOffset = spawnPoint.up * axisSpawnOffset.y;
    //        Vector3 zOffset = spawnPoint.forward * axisSpawnOffset.z;
    //        Vector3 spawnOffsetVector = xOffset + yOffset + zOffset;

    //        int spawnDensity = Random.Range(spawnDensityMin, spawnDensityMax + 1);





    //        Vector3 xDeviation = spawnPoint.right * Random.Range(-spawnDeviationAxisMax.x, spawnDeviationAxisMax.x);
    //        Vector3 yDeviation = spawnPoint.up * Random.Range(-spawnDeviationAxisMax.y, spawnDeviationAxisMax.y);
    //        Vector3 zDeviation = spawnPoint.forward * Random.Range(-spawnDeviationAxisMax.z, spawnDeviationAxisMax.z);

    //        Vector3 spawnDeviationVector = xDeviation + yDeviation + zDeviation;
    //        Vector3 objectSpawnPoint = spawnPoint.position + spawnDeviationVector + spawnOffsetVector;


    //        //Should optimise this later in some object pool 
    //        Enemy enemyObject = Instantiate(enemyPrefab, objectSpawnPoint, spawnPoint.rotation);


    //        //For one time spawning, stop coroutine immedietly if wave interval = 0 
    //        if (waveInterval == 0)
    //            isCoroutineRunning = false;
    //    }

    //}

}
