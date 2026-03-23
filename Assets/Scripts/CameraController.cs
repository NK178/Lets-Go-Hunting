using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;



public enum CAMERATYPE { 
    FIRST_PERSON, 
    SHIP_CAMERA,
    NUM_TYPE
}

[System.Serializable]
public class CameraCatagory {
    public CAMERATYPE type;
    public CinemachineCamera camera; 
}


public class CameraController : MonoBehaviour
{

    [SerializeField] private List<CameraCatagory> cameraList;

    private CameraCatagory activeCameraCat; 

    //public static Action<float> onFirstPersonCameraRotate;
    public static Action<Vector3, Vector3> onFirstPersonCameraRotate;

    void Awake()
    {

        ChangeCamera(CAMERATYPE.FIRST_PERSON);
        LockCursor(true);

    }


    private void OnEnable()
    {
        ShipWheel.onPlayerAtWheel += ChangeToShipCamera;
    }

    private void OnDisable()
    {
        ShipWheel.onPlayerAtWheel -= ChangeToShipCamera;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeCameraCat == null)
            return; 


        if (activeCameraCat.type == CAMERATYPE.FIRST_PERSON)
            HandleFirstPersonCamera();
    }

    void HandleFirstPersonCamera()
    {
        //float yRotateAngle = activeCameraCat.camera.transform.rotation.eulerAngles.y;

        //onFirstPersonCameraRotate.Invoke(yRotateAngle);


        Vector3 cameraForward = activeCameraCat.camera.transform.forward;
        Vector3 cameraRight = activeCameraCat.camera.transform.right;
        onFirstPersonCameraRotate.Invoke(cameraForward, cameraRight);

    }

    void ChangeToShipCamera(bool condition)
    {
        if (condition)
        {
            ChangeCamera(CAMERATYPE.SHIP_CAMERA);
        }
        else if (!condition)
        {
            ChangeCamera(CAMERATYPE.FIRST_PERSON);
        }
    }


    void ChangeCamera(CAMERATYPE type)
    {
        foreach (CameraCatagory camCategory in cameraList)
        {
            if (camCategory.type == type)
            {
                activeCameraCat = camCategory;
                activeCameraCat.camera.gameObject.SetActive(true);
            }
            else
                camCategory.camera.gameObject.SetActive(false);
        }
    }

    void LockCursor(bool condition)
    {
        if (condition)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }
}
