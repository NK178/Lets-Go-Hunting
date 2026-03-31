using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnerWaveFunction", menuName = "Scriptable Objects/SpawnerWaveFunction")]
public class SpawnerWaveFunction : BaseWaveFunction
{

    [SerializeField] private string spawnPointName; 
    [SerializeField] private float waveInterval;


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

            Debug.Log("EXCUTING AT: " + spawnPoint.transform.position);
        }

    }

        //public override void Excute(SectionManager sectionManager)
        //{
        //    spawnPoint = sectionManager.transform.Find(spawnPointName);   

        //    Debug.Log("EXCUTING AT: " + spawnPoint.transform.position);
        //}
    }
