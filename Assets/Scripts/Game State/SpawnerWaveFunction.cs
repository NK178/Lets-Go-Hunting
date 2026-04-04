using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnerWaveFunction", menuName = "Scriptable Objects/SpawnerWaveFunction")]
public class SpawnerWaveFunction : BaseWaveFunction
{

    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private string shipTargetPointName;
    [SerializeField] private string spawnPointName; 
    [SerializeField] private float waveInterval;

    [Range(0, 10f)]
    [SerializeField] private float spawnWidthDeviateRange; 

    private Transform spawnPoint;

    public override void Excute(SectionManager sectionManager)
    {
        spawnPoint = sectionManager.transform.Find(spawnPointName);

        sectionManager.StartCoroutine(SpawnerCoroutine(spawnPoint));
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
    

            Debug.Log("EXCUTING AT: " + objectSpawnPoint);
        }

    }
}
