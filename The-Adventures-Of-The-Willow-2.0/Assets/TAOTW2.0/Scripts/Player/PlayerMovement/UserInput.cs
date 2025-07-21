using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;
    [HideInInspector] public PlayerMoveAndExtraActions playerMoveAndExtraActions;
    [HideInInspector] public Vector2 moveInput;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        playerMoveAndExtraActions = new PlayerMoveAndExtraActions();

        playerMoveAndExtraActions.PlayerActions.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        EnhancedTouchSupport.Enable();
    }

    private void OnEnable()
    {
        playerMoveAndExtraActions.Enable();
        EnhancedTouchSupport.Enable();
    }
    private void OnDisable()
    {
        playerMoveAndExtraActions.Disable();
        EnhancedTouchSupport.Disable();
    }
    public void DisableInput()
    {
        playerMoveAndExtraActions.Disable();
        EnhancedTouchSupport.Disable();
    }

    public void EnableInput()
    {
        playerMoveAndExtraActions.Enable();
        EnhancedTouchSupport.Enable();
    }
}
