using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{

    [SerializeField] private PlayerInput playerInput;


    public static Action<Vector2> onMove;

    public static Action onJump;

    public static Action onInteract;

    public static Action onLeftClick; 

    //hmmm
    //public static Func<Vector2, Vector2> onWalk;

    //public static Action<Vector2> onWalk; 

    //public delegate Vector2 OnWalk();
    //public static OnWalk onWalk;

    private InputAction moveAction; 
    private InputAction jumpAction; 
    private InputAction interactAction; 
    private InputAction leftClickAction; 


    void Awake()
    {
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        interactAction = playerInput.actions["Interact"];
        leftClickAction = playerInput.actions["Attack"];

        if (moveAction != null)
            moveAction.Enable();
        if (jumpAction != null)
            jumpAction.Enable();
        if (interactAction != null)
            interactAction.Enable();
        if (leftClickAction != null)
            leftClickAction.Enable();   

    }

    // Update is called once per frame
    void Update()
    {
        HandleWalkInput();

        HandleJumpInput();

        HandleInteractInput();

        HandleLeftClickInput();
    }

    private void HandleWalkInput()
    {
        Vector2 inputDirection = moveAction.ReadValue<Vector2>();

        //Vector2 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;

        onMove?.Invoke(inputDirection);

        //if (moveDirection != Vector2.zero)
        //{
        //    onWalk?.Invoke(moveDirection);
        //}
    }

    private void HandleJumpInput()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            onJump?.Invoke();
        }
    }

    private void HandleInteractInput()
    {
        if (interactAction.WasPressedThisFrame())
        {
            onInteract?.Invoke();   
        }
    }

    private void HandleLeftClickInput()
    {
        if (leftClickAction.WasPressedThisFrame())
        {
            onLeftClick?.Invoke();
        }
    }


    
}
