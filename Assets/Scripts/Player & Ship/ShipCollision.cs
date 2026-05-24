using System;
using UnityEngine;

public class ShipCollision : MonoBehaviour
{

    public static Action<Vector3> onCollisionResolve; 
    public static Action<float> onCollisionRotateResolve; 

    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private float wallResolveDistFactor; 
    [SerializeField] private float wallSteerForce; 



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //works but probably an expensive operation, should try to make my own, for now it will work 
    void HandleShipCollision(Transform collider)
    {
        BoxCollider otherBox = collider.GetComponent<BoxCollider>();
        BoxCollider box = transform.GetComponent<BoxCollider>();

        // We let Unity tell us the direction and exact distance needed to separate them
        Vector3 separationDirection;
        float penetrationDistance;

        // This native function handles all scales, rotations, and box center offsets perfectly
        bool isOverlapping = Physics.ComputePenetration(
            box, transform.position, transform.rotation,
            otherBox, collider.position, collider.rotation,
            out separationDirection, out penetrationDistance
        );

        if (isOverlapping && penetrationDistance > 0.001f)
        {
            Vector3 worldAdjustment = separationDirection * penetrationDistance;
            onCollisionResolve.Invoke(worldAdjustment * wallResolveDistFactor);


            // 1. Check how parallel the vectors are using Dot Product
            // -1 means a perfect head-on smash, 0 means a side-swipe
            float headOnCheck = Vector3.Dot(transform.forward, separationDirection);

            Vector3 finalSeparationDirection = separationDirection;

            // 2. If it's a near-perfect head-on collision, give it a tiny side nudge
            if (headOnCheck < -0.9f)
            {
                // Blend a tiny bit of the car's right vector into the push direction 
                // This forces the cross product to choose a side rather than returning 0
                finalSeparationDirection = (separationDirection + (transform.right * 0.1f)).normalized;
            }

            // 3. Now run your cross product safely!
            Vector3 crossResult = Vector3.Cross(transform.forward, finalSeparationDirection);
            float torqueAdjustment = crossResult.y * wallSteerForce;
            //Debug.Log("TORQUE: " + torqueAdjustment + "CRS: " + crossResult);
            onCollisionRotateResolve.Invoke(torqueAdjustment);


            //Vector3 crossResult = Vector3.Cross(transform.forward, separationDirection);

            //// 3. Extract the rotation value and scale it by your guiding multiplier
            //float torqueAdjustment = crossResult.y * wallSteerForce;

            //Debug.Log("TORQUE: " + torqueAdjustment);
            //// 4. Pass the rotation assist directly to your car controller
            //// (You can pass this as an event, or apply it to a Rigidbody if using physics)
            //onCollisionRotateResolve.Invoke(torqueAdjustment);
        }
    }


    //Seperating Axis fail for some reason, would be nice to make it work 
    //void HandleShipCollision(Transform collider)
    //{
    //    BoxCollider otherBox = collider.GetComponent<BoxCollider>();
    //    Vector3 otherCenter = collider.TransformPoint(otherBox.center);
    //    //Vector3 halfExtents = Vector3.Scale(otherBox.size, collider.lossyScale) / 2f;


    //    BoxCollider box = transform.GetComponent<BoxCollider>();

    //    //Find side that ship is in contact with 
    //    Vector3 contactPoint = box.ClosestPoint(otherCenter);
    //    Vector3 localPoint = transform.InverseTransformPoint(contactPoint);

    //    Vector3 otherExtents = box.size / 2f;
    //    Vector3 dominantFace = GetDominantFace(localPoint, otherExtents);
    //    Debug.Log($"collided on our **{dominantFace}** surface!");


    //    Vector3 localOtherCenter = transform.InverseTransformPoint(otherCenter);
    //    float centerDistance = Mathf.Abs(Vector3.Dot(dominantFace, localOtherCenter));

    //    //Vector3 localHalfExtentsOfOther = transform.InverseTransformDirection(halfExtents);
    //    //float incomingExtentDistance = Mathf.Abs(Vector3.Dot(dominantFace, localHalfExtentsOfOther));


    //    //Vector3 otherLocalX = transform.InverseTransformDirection(collider.right);
    //    //Vector3 otherLocalY = transform.InverseTransformDirection(collider.up);
    //    //Vector3 otherLocalZ = transform.InverseTransformDirection(collider.forward);

    //    //// 4. Project the incoming object's raw unscaled local sizes onto your dominant face axis
    //    //// We use the raw otherBox.size * lossyScale split across each axis.
    //    //Vector3 otherRawHalfExtents = Vector3.Scale(otherBox.size, collider.lossyScale) / 2f;

    //    //float incomingExtentDistance =
    //    //    Mathf.Abs(Vector3.Dot(dominantFace, otherLocalX)) * otherRawHalfExtents.x +
    //    //    Mathf.Abs(Vector3.Dot(dominantFace, otherLocalY)) * otherRawHalfExtents.y +
    //    //    Mathf.Abs(Vector3.Dot(dominantFace, otherLocalZ)) * otherRawHalfExtents.z;


    //    Vector3 worldRightUnscaled = collider.rotation * Vector3.right;
    //    Vector3 worldUpUnscaled = collider.rotation * Vector3.up;
    //    Vector3 worldForwardUnscaled = collider.rotation * Vector3.forward;

    //    // Step B: Convert those pure unscaled world directions into our ship's local space
    //    Vector3 otherLocalX = transform.InverseTransformDirection(worldRightUnscaled);
    //    Vector3 otherLocalY = transform.InverseTransformDirection(worldUpUnscaled);
    //    Vector3 otherLocalZ = transform.InverseTransformDirection(worldForwardUnscaled);

    //    // Step C: Get the actual half-extents (size * scale / 2)
    //    Vector3 otherRawHalfExtents = Vector3.Scale(otherBox.size, collider.lossyScale) / 2f;

    //    // Step D: Project them. Now, each axis is perfectly scaled exactly once!
    //    float incomingExtentDistance =
    //        Mathf.Abs(Vector3.Dot(dominantFace, otherLocalX)) * otherRawHalfExtents.x +
    //        Mathf.Abs(Vector3.Dot(dominantFace, otherLocalY)) * otherRawHalfExtents.y +
    //        Mathf.Abs(Vector3.Dot(dominantFace, otherLocalZ)) * otherRawHalfExtents.z;


    //    float shipExtentDistance = Mathf.Abs(Vector3.Dot(dominantFace, otherExtents));

    //    float penetrationDistance = (shipExtentDistance + incomingExtentDistance) - centerDistance;

    //    Debug.Log("Penetration Distance: " + penetrationDistance);
    //    ResolveCollision(dominantFace, penetrationDistance);




    //        //Quaternion otherRotation = collider.rotation;

    //    //LayerMask wallMask = LayerMask.GetMask("Ship");
    //    //Collider[] hitColliders = Physics.OverlapBox(otherCenter, halfExtents, otherRotation, wallMask);

    //    //    //There should be only the ship collider actually so take the first one 
    //    //    //Collider ship = hitColliders[0];
    //    //    Collider ship = transform.GetComponent<Collider>(); 

    //    //if (ship != null)
    //    //{
    //    //    BoxCollider box = ship.GetComponent<BoxCollider>();

    //    //    //Find side that ship is in contact with 
    //    //    Vector3 contactPoint = box.ClosestPoint(collider.gameObject.transform.position);
    //    //    Vector3 localPoint = ship.transform.InverseTransformPoint(contactPoint);

    //    //    Vector3 otherExtents = box.size / 2f;
    //    //    Vector3 dominantFace = GetDominantFace(localPoint, otherExtents);
    //    //    Debug.Log($"collided on our **{dominantFace}** surface!");


    //    //    Vector3 localOtherCenter = ship.transform.InverseTransformPoint(collider.position);
    //    //    float centerDistance = Mathf.Abs(Vector3.Dot(dominantFace, localOtherCenter));

    //    //    Vector3 localHalfExtentsOfOther = ship.transform.InverseTransformDirection(halfExtents);
    //    //    float incomingExtentDistance = Mathf.Abs(Vector3.Dot(dominantFace, localHalfExtentsOfOther));

    //    //    float shipExtentDistance = Mathf.Abs(Vector3.Dot(dominantFace, otherExtents));

    //    //    float penetrationDistance = (shipExtentDistance + incomingExtentDistance) - centerDistance;

    //    //    ////peneatration distance???
    //    //    //float extentDistance = Vector3.Dot(dominantFace, halfExtents);
    //    //    //float localExtentDistance = Vector3.Dot(dominantFace, otherExtents);

    //    //    //Debug.Log("Extent: " + extentDistance + "HalfSize: " + localExtentDistance);
    //    //    //float penetrationDistance = localExtentDistance - extentDistance;
    //    //    //ResolveCollision(dominantFace, penetrationDistance);
    //    //}
    //}


    void ResolveCollision(Vector3 dominantFace, float d)
    {
        Vector3 adjustment = Vector3.zero; 
        //float penetrationDistance = 5f;

        adjustment = -dominantFace * d;

        Vector3 worldAdjustment = transform.TransformDirection(adjustment);

        onCollisionResolve.Invoke(worldAdjustment);
    }

    Vector3 GetDominantFace(Vector3 localPoint, Vector3 extents)
    {
        Vector3 normalizedPos = new Vector3(
            localPoint.x / extents.x,
            localPoint.y / extents.y,
            localPoint.z / extents.z
        );

        float absX = Mathf.Abs(normalizedPos.x);
        float absY = Mathf.Abs(normalizedPos.y);
        float absZ = Mathf.Abs(normalizedPos.z);


        if (absX > absY && absX > absZ)
        {
            if (normalizedPos.x > 0)
                return new Vector3(1, 0, 0);
            else
                return new Vector3(-1, 0, 0);
        }
        else if (absY > absX && absY > absZ)
        {
            if (normalizedPos.y > 0)
                return new Vector3(0, 1, 0);
            else
                return new Vector3(0, -1, 0);
        }
        else
        {
            if (normalizedPos.z > 0)
                return new Vector3(0, 0, 1);
            else
                return new Vector3(0, 0, -1);
        }
    }


    //string GetDominantFace(Vector3 localPoint, Vector3 extents)
    //{
    //    Vector3 normalizedPos = new Vector3(
    //        localPoint.x / extents.x,
    //        localPoint.y / extents.y,
    //        localPoint.z / extents.z
    //    );

    //    float absX = Mathf.Abs(normalizedPos.x);
    //    float absY = Mathf.Abs(normalizedPos.y);
    //    float absZ = Mathf.Abs(normalizedPos.z);

    //    if (absX > absY && absX > absZ)
    //    {
    //        return normalizedPos.x > 0 ? "Right (+X)" : "Left (-X)";
    //    }
    //    else if (absY > absX && absY > absZ)
    //    {
    //        return normalizedPos.y > 0 ? "Top (+Y)" : "Bottom (-Y)";
    //    }
    //    else
    //    {
    //        return normalizedPos.z > 0 ? "Front (+Z)" : "Back (-Z)";
    //    }
    //}


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            Debug.Log("OBSTACLE");
            HandleShipCollision(other.gameObject.transform);
        }
    }
}
