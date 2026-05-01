using UnityEngine;


//basically my boi will instanly find the wall and cling to it 
public class InsectWallEnemy : Enemy
{
    [SerializeField] private string clingWallTagName;


    [SerializeField] private LayerMask clingWallLayer;
    [SerializeField] private float raycastDistance; 
    private GameObject wallRef; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //double raycast to find the wall 

        bool wallFound = false;
        RaycastHit hitInfo; 
        if (Physics.Raycast(transform.position, transform.right, out hitInfo, raycastDistance, clingWallLayer))
            wallFound = true;

        if (!wallFound)
        {
            if (Physics.Raycast(transform.position, -transform.right, out hitInfo, raycastDistance, clingWallLayer))
                wallFound = true;
        }


        if (wallFound && hitInfo.collider != null)
        {
            Debug.Log("WALL FOUND");

            transform.position = hitInfo.point;

            transform.rotation = Quaternion.FromToRotation(transform.up, hitInfo.normal);
        }
    }



    // Update is called once per frame
    void Update()
    {


        Debug.DrawLine(transform.position, transform.position + transform.right * raycastDistance, Color.red);
        Debug.DrawLine(transform.position, transform.position + -transform.right * raycastDistance, Color.red);

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
