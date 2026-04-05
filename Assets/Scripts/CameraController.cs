using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;



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
    public static Action<Vector3, Vector3> onFirstPersonCameraRotate;
    public static Action<Vector2> onMouseMoved; 

    private bool cameraLockRotate = false;

    private Transform shipShootPointTransform; 

    void Awake()
    {

        ChangeCamera(CAMERATYPE.FIRST_PERSON);
        LockCursor(true);
    }


    private void OnEnable()
    {
        ShipWheel.onPlayerAtWheel += ChangeToShipCamera;
        Ship.onShipIsCombatMode += ToggleLockCameraRotate;

        Ship.onShipLockTransform += TrackShipRotate;

    }

    private void OnDisable()
    {
        ShipWheel.onPlayerAtWheel -= ChangeToShipCamera;
        Ship.onShipIsCombatMode -= ToggleLockCameraRotate;
        Ship.onShipLockTransform -= TrackShipRotate;

    }

    // Update is called once per frame
    void Update()
    {
        if (activeCameraCat == null)
            return;

        HandleMouse();

        if (activeCameraCat.type == CAMERATYPE.FIRST_PERSON)
            HandleFirstPersonCamera();

        if (cameraLockRotate)
        {
            activeCameraCat.camera.gameObject.transform.rotation = shipShootPointTransform.rotation;
        }
    }

    void HandleFirstPersonCamera()
    {
        Vector3 cameraForward = activeCameraCat.camera.transform.forward;
        Vector3 cameraRight = activeCameraCat.camera.transform.right;
        onFirstPersonCameraRotate.Invoke(cameraForward, cameraRight);
    }

    void HandleMouse()
    {
        if (cameraLockRotate)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            onMouseMoved?.Invoke(mousePos);
        }

    }

    void TrackShipRotate(Transform transform)
    {
        shipShootPointTransform = transform;    
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

    void ToggleLockCameraRotate(bool condition)
    {
        cameraLockRotate = condition;

        if (condition)
        {
            if (activeCameraCat.type == CAMERATYPE.FIRST_PERSON)
            {
                var comp = activeCameraCat.camera.GetComponent<CinemachinePanTilt>();
                if (comp != null)
                {
                    LockCursor(false);
                    comp.enabled = false;
                }
            }
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
            Cursor.visible = false;
        }

    }
}
