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

    [SerializeField] private Transform shipCameraTarget; 
    [SerializeField] private List<CameraCatagory> cameraList;

    private CameraCatagory activeCameraCat; 
    public static Action<Vector3, Vector3> onFirstPersonCameraRotate;
    public static Action<Transform> onCameraTransformChanged;
    public static Action<Vector2> onMouseMoved;

   
    

    private bool cameraLockRotate = false;

    private Transform shipShootPointTransform;

    private float orbitalCameraDefaultVerticalValue = 0f;

    void Awake()
    {

        ChangeCamera(CAMERATYPE.FIRST_PERSON);
        LockCursor(true);

        //quick set up 
        foreach (CameraCatagory category in cameraList)
        {
            if (category.type == CAMERATYPE.SHIP_CAMERA)
            {
                if (category.camera.TryGetComponent<CinemachineOrbitalFollow>(out CinemachineOrbitalFollow component))
                {
                    orbitalCameraDefaultVerticalValue = component.VerticalAxis.Value;
                    break; 
                }
            }
        }
    }


    private void OnEnable()
    {
        Ship.onShipIsCombatMode += ToggleFreezeRotation;

        ShipCombat.onShipLockTransform += TrackShipRotate;

        Ship.onShipChangedMode += HandleCameraShipMode;
    }

    private void OnDisable()
    {
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

        onCameraTransformChanged.Invoke(activeCameraCat.camera.transform);
    }


    void HandleFirstPersonCamera()
    {
        Vector3 cameraForward = activeCameraCat.camera.transform.forward;
        Vector3 cameraRight = activeCameraCat.camera.transform.right;
        onFirstPersonCameraRotate?.Invoke(cameraForward, cameraRight);
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
                HandleChangedCameraProperties(camCategory);
            }
            else
                camCategory.camera.gameObject.SetActive(false);
        }


    }

    void HandleChangedCameraProperties(CameraCatagory camCatagory)
    {

        CAMERATYPE camType = camCatagory.type; 

        if (camType == CAMERATYPE.SHIP_CAMERA)
        {
            CinemachineOrbitalFollow orbitalFollow = activeCameraCat.camera.gameObject.GetComponent<CinemachineOrbitalFollow>();
            orbitalFollow.HorizontalAxis.Value = 0f;
            orbitalFollow.VerticalAxis.Value = orbitalCameraDefaultVerticalValue;
        }
        else if (camType == CAMERATYPE.FIRST_PERSON)
        {
            float panAngle = Mathf.Atan2(shipCameraTarget.forward.x, shipCameraTarget.forward.z) * Mathf.Rad2Deg;
            CinemachinePanTilt panTilt = activeCameraCat.camera.gameObject.GetComponent<CinemachinePanTilt>();
            panTilt.PanAxis.Value = panAngle;
            panTilt.TiltAxis.Value = 0;
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
