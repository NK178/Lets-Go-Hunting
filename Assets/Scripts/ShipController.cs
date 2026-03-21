using NUnit.Framework.Constraints;
using UnityEngine;

public class Ship : MonoBehaviour
{

    [SerializeField] private Vector3 DEBUG_travelDirection;

    [SerializeField] private float moveSpeed; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += DEBUG_travelDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("COLLIDE: " + other.gameObject.name);

        if (other.gameObject.tag == "Player")
            other.gameObject.transform.parent = this.transform;
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            other.gameObject.transform.parent = null; 
    }
}
