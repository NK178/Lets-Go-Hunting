using UnityEngine;

public class DriveCheckpoint : MonoBehaviour
{

    [SerializeField] private int orderIndex;
    [SerializeField] private float detectDistance; 

    private Ship shipReference;
    private bool isShipDetected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shipReference = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();

        if (shipReference == null)
        {
            Debug.Log("SHIP NOT FOUND");
            return; 
        }

        isShipDetected = false; 
    }

    // Update is called once per frame
    void Update()
    {

        if (isShipDetected)
            return;

        isShipDetected = IsPlayerInRange();
    }

    private bool IsPlayerInRange()
    {
        float distance = Vector3.Distance(transform.position, shipReference.transform.position);

        if (distance < detectDistance)
        {
            return true;
        }
        else
            return false; 
    }

    public bool IsDetected()
    {
        return isShipDetected; 
    }

    public int GetOrderIndex()
    {
        return orderIndex; 
    }
}
