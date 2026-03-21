using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{

    [SerializeField] private PlayerInput playerInput;


    public static Action<Vector2> onWalk;

    public static Action onJump; 


    //hmmm
    //public static Func<Vector2, Vector2> onWalk;

    //public static Action<Vector2> onWalk; 

    //public delegate Vector2 OnWalk();
    //public static OnWalk onWalk;

    private InputAction moveAction; 
    private InputAction jumpAction; 

    void Awake()
    {
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        if (moveAction != null)
            moveAction.Enable();
        if (jumpAction != null)
            jumpAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        HandleWalkInput();

        HandleJumpInput();
    }

    private void HandleWalkInput()
    {
        Vector2 inputDirection = moveAction.ReadValue<Vector2>();

        //Vector2 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;

        onWalk?.Invoke(inputDirection);

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


    
}
