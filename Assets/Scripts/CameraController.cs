using System;
using System.Collections.Generic;
using Unity.Cinemachine;
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
    public static Action<Vector3> onMouseMoved3DPos; 
    

    private bool cameraLockRotate = false;

    private Transform shipShootPointTransform; 

    void Awake()
    {

        ChangeCamera(CAMERATYPE.FIRST_PERSON);
        LockCursor(true);
    }


    private void OnEnable()
    {
        //ShipWheel.onPlayerAtWheel += ChangeToShipCamera;
        Ship.onShipIsCombatMode += ToggleFreezeRotation;

        ShipCombat.onShipLockTransform += TrackShipRotate;

        Ship.onShipChangedMode += HandleCameraShipMode; 

    }

    private void OnDisable()
    {
        //ShipWheel.onPlayerAtWheel -= ChangeToShipCamera;
        Ship.onShipIsCombatMode -= ToggleFreezeRotation;
        ShipCombat.onShipLockTransform -= TrackShipRotate;

        Ship.onShipChangedMode -= HandleCameraShipMode;

    }

    private void HandleCameraShipMode(SHIPMODE shipMode)
    {
        switch (shipMode)
        {

            case SHIPMODE.IDLE:
                ToggleFreezeRotation(false);
                ChangeCamera(CAMERATYPE.FIRST_PERSON);
                break;
            case SHIPMODE.MANUAL_DRIVE:
                ToggleFreezeRotation(false);
                ChangeCamera(CAMERATYPE.SHIP_CAMERA);
                break;
            case SHIPMODE.COMBAT:
                ChangeCamera(CAMERATYPE.FIRST_PERSON);
                ToggleFreezeRotation(true);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (activeCameraCat == null)
            return;

        HandleMouse();

        if (activeCameraCat.type == CAMERATYPE.FIRST_PERSON)
            HandleFirstPersonCamera();

        //bad method but will do for now 
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

            //Needa figure out what to do for z???
            Vector3 mousePos3D = mousePos;
            mousePos3D.z = 100f;

            //float x01 = mousePos.x / Screen.width;
            //float y01 = mousePos.y / Screen.height;

            //// 3. Remap to -1 to 1 range
            //float xNorm = (x01 * 2) - 1;
            //float yNorm = (y01 * 2) - 1;


            //Vector3 mousePos3D = new Vector3(xNorm, yNorm, 5);

            onMouseMoved3DPos?.Invoke(mousePos3D);
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

    void ToggleFreezeRotation(bool condition)
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
        else if (!condition)
        {
            if (activeCameraCat.type == CAMERATYPE.FIRST_PERSON)
            {
                var comp = activeCameraCat.camera.GetComponent<CinemachinePanTilt>();
                if (comp != null)
                {
                    LockCursor(true);
                    comp.enabled = true;
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
