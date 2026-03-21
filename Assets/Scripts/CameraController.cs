using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;



public enum CAMERATYPE { 
    FIRST_PERSON, 
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

        EnableCamera(CAMERATYPE.FIRST_PERSON);

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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //float yRotateAngle = activeCameraCat.camera.transform.rotation.eulerAngles.y;

        //onFirstPersonCameraRotate.Invoke(yRotateAngle);


        Vector3 cameraForward = activeCameraCat.camera.transform.forward;
        Vector3 cameraRight = activeCameraCat.camera.transform.right;
        onFirstPersonCameraRotate.Invoke(cameraForward, cameraRight);

    }


    void EnableCamera(CAMERATYPE type)
    {

        foreach (CameraCatagory camera in cameraList)
        {
            if (camera.type == type)
            {
                activeCameraCat = camera;
                return;
            }
        }
    }
}
