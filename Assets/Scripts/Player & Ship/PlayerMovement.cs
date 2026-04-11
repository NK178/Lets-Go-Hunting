using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //[SerializeField] private PlayerInputManager inputManager;

    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform gunPlaceholder; 
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float gravityPower;

    private Vector3 horizontalVelocity = Vector3.zero;
    private Vector3 verticalVelocity = Vector3.zero;
    private Vector3 cameraForward = Vector3.zero;
    private Vector3 cameraRight = Vector3.zero;

    private float yRotation;

    private bool freezeMovement = false;
    private bool freezeRotation = false; 

    //private bool isPlayerDriving = false;

    public static Action<Vector3> onGunPlaceholderMove;


    //private Transform shipShootTransform; 
        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }


    private void OnEnable()
    {
        PlayerInputManager.onMove += CalculateHorizontalVelocity;
        PlayerInputManager.onJump += CalculateJumpVelocity;

        //ShipWheel.onPlayerAtWheel += ToggleMoveFreeze;

        Ship.onShipChangedMode += HandlePlayerShipMode;
        Ship.onShipIsCombatMode += ToggleMoveFreeze;
        Ship.onShipIsCombatMode += ToggleRotateFreeze;
        //ShipCombat.onShipLockTransform += ShipLockTransform; 
        CameraController.onFirstPersonCameraRotate += ReadCameraRotation; 

    }

    private void OnDisable()
    {
        PlayerInputManager.onMove -= CalculateHorizontalVelocity;
        PlayerInputManager.onJump -= CalculateJumpVelocity;

        //ShipWheel.onPlayerAtWheel -= ToggleMoveFreeze;

        Ship.onShipChangedMode -= HandlePlayerShipMode;
        Ship.onShipIsCombatMode -= ToggleMoveFreeze;
        Ship.onShipIsCombatMode -= ToggleRotateFreeze;
        //ShipCombat.onShipLockTransform -= ShipLockTransform;
        CameraController.onFirstPersonCameraRotate -= ReadCameraRotation;
    }



    // Update is called once per frame
    void Update()
    {

        //if (characterController.isGrounded)
        //    verticalVelocity = Vector3.zero;
        //else
        //    verticalVelocity.y += gravityPower * Time.deltaTime;

        if (!characterController.isGrounded)
            verticalVelocity.y += gravityPower * Time.deltaTime;
        if (verticalVelocity.y < -15)
            verticalVelocity.y = -15;

        Vector3 finalVelocity = (horizontalVelocity + verticalVelocity) * Time.deltaTime;
        yRotation = CalculateCharacterRotation().eulerAngles.y;

        //if (!freezeRotation)
        //    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, yRotation, transform.rotation.eulerAngles.z);
        //else
        //    transform.rotation = shipShootTransform.rotation; 

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, yRotation, transform.rotation.eulerAngles.z);

        if (!freezeMovement)
            characterController.Move(finalVelocity);
    }


    private void HandlePlayerShipMode(SHIPMODE shipMode)
    {
        switch (shipMode)
        {
            case SHIPMODE.IDLE:
                ToggleMoveFreeze(false);
                ToggleRotateFreeze(false);
                break;
            case SHIPMODE.MANUAL_DRIVE:
                ToggleMoveFreeze(true);
                ToggleRotateFreeze(false);
                break;
            case SHIPMODE.COMBAT:
                ToggleMoveFreeze(true);
                ToggleRotateFreeze(true);
                break;

        }
    }

    Quaternion CalculateCharacterRotation()
    {
        return Quaternion.LookRotation(cameraForward, cameraRight); 
    }

    void ReadCameraRotation(Vector3 camForward, Vector3 camRight)
    {
        camForward.y = 0;
        camRight.y = 0;
        cameraForward = camForward; 
        cameraRight = camRight; 
    }


    void CalculateJumpVelocity()
    {
        if (!characterController.isGrounded)
            return;


        verticalVelocity = Vector3.zero; 
        Vector3 jumpVelocity = Vector3.up * jumpPower;
        verticalVelocity += jumpVelocity;

        onGunPlaceholderMove?.Invoke(gunPlaceholder.position);
    }

    void CalculateHorizontalVelocity(Vector2 dir)
    {
        Vector3 directionVector = transform.right * dir.x + transform.forward * dir.y;
        horizontalVelocity = directionVector * moveSpeed;

        onGunPlaceholderMove?.Invoke(gunPlaceholder.position);
    }


    //void ShipLockTransform(Transform transform)
    //{
    //    shipShootTransform = transform; 
    //}


    void ToggleMoveFreeze(bool condition)
    {
        freezeMovement = condition;
    }

    void ToggleRotateFreeze(bool condition)
    {
        freezeRotation = condition; 
    }



    //void HandlePlayerDriveShip(bool condition)
    //{
    //    isPlayerDriving = condition; 
    //}

}
