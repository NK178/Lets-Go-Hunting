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
    [Range(0, 10f)]
    [SerializeField] private float spawnWidthDeviateRange;


    [Header("Follow Ship Movement")]
    [SerializeField] private bool followShipMovement;
    [SerializeField] private Vector3 followDirectionOffset;
    //[SerializeField] private float followDistanceOffset;

    [Header("Follow Ship Rotation")]
    [SerializeField] private bool followShipRotation;
    [SerializeField] private Vector3 followRotationOffset; 

    private Transform spawnPoint;
    private Transform shipTransform = null;
    private float startingPosY; 
    public override void Excute(SectionManager sectionManager)
    {
        spawnPoint = sectionManager.transform.Find(spawnPointName);

        sectionManager.StartCoroutine(SpawnerCoroutine(spawnPoint));


        //maybe i can go find the target points instead 
        shipTransform = GameObject.FindGameObjectWithTag("Ship").gameObject.transform;

        startingPosY = spawnPoint.position.y;
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

        //Follow ship movement based on rotation i think is good 
        if (followShipMovement)
        {
            Vector3 localOffset = new Vector3(followDirectionOffset.x, 0, followDirectionOffset.z);

            //interesting function 
            Vector3 rotatedOffset = shipTransform.TransformDirection(localOffset);

            Vector3 newSpawnPosition = rotatedOffset + new Vector3(shipTransform.position.x, startingPosY, shipTransform.position.z);
            spawnPoint.position = newSpawnPosition;
        }


        if (followShipRotation)
        {
            //shld add rotational offsets in the future 
            spawnPoint.rotation = shipTransform.rotation; 
        }



        //Might not be so good
        //if (followShipMovement)
        //{
        //    //gonna have to move the spawnpoint lol 
        //    Vector3 positionOffset = new Vector3(followDirectionOffset.x,
        //                                         startingPosY,
        //                                         followDirectionOffset.z);

        //    //Vector3 newSpawnPosition = new Vector3(positionOffset.x + shipTransform.position.x,
        //    //                                       positionOffset.y,
        //    //                                       positionOffset.z + shipTransform.position.z);

        //    //Vector3 projectedOffset = Vector3.Project(positionOffset, shipTransform.position);


        //    //for some reason it works better this way, then uno reverse it
        //    Vector3 projectedOffset = -Vector3.Project(shipTransform.position, positionOffset);
        //    //projectedOffset.y = 0;

        //    //Vector3 projectedPosition = followDistanceOffset * projectedOffset + new Vector3(shipTransform.position.x, startingPosY, shipTransform.position.z);

        //    //new Vector3(followDistanceOffset * projectedOffset.x + shipTransform.position.x,
        //    //                                    followDistanceOffset * positionOffset.y, +startingPosY,
        //    //                                    followDistanceOffset * projectedOffset.z + shipTransform.position.z);


        //    Vector3 projectedPosition = new Vector3(followDistanceOffset * projectedOffset.x + shipTransform.position.x,
        //                                            startingPosY,
        //                                            followDistanceOffset * projectedOffset.z + shipTransform.position.z);


        //    spawnPoint.position = projectedPosition;

        //    //Debug.Log("NEW SPAWN: " + newSpawnPosition);
        //}

        //if (followShipMovement)
        //{

        //    //gonna have to move the spawnpoint lol 
        //    Vector3 positionOffset = new Vector3(followDirectionOffset.x,
        //                                         spawnPoint.position.y,
        //                                         followDirectionOffset.z);

        //    Vector3 newSpawnPosition = new Vector3(followDistanceOffset * positionOffset.x + shipTransform.position.x,
        //                                           positionOffset.y,
        //                                           followDistanceOffset * positionOffset.z + shipTransform.position.z);

        //    spawnPoint.position = newSpawnPosition;

        //    //Debug.Log("NEW SPAWN: " + newSpawnPosition);
        //}
    }

    private IEnumerator SpawnerCoroutine(Transform spawnPoint)
    {
        while (true)
        {
            yield return new WaitForSeconds(waveInterval);

            float widthDeviation = Random.Range(-spawnWidthDeviateRange, spawnWidthDeviateRange);

            Vector3 objectSpawnPoint = spawnPoint.position + spawnPoint.right * widthDeviation;

            //Should optimise this later in some object pool 
            Enemy enemyObject = Instantiate(enemyPrefab, objectSpawnPoint, spawnPoint.rotation);


            ////Hmmm yeah I cant do this way 
            //enemyObject.gameObject.transform.parent = spawnPoint;

            //Debug.Log("EXCUTING AT: " + objectSpawnPoint);
        }

    }
}
