using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    private void OnEnable()
    {
        playerMoveAndExtraActions.Enable();
    }
    private void OnDisable()
    {
        playerMoveAndExtraActions.Disable();
    }
    public void DisableInput()
    {
        playerMoveAndExtraActions.Disable();
    }

    public void EnableInput()
    {
        playerMoveAndExtraActions.Enable();
    }
}
