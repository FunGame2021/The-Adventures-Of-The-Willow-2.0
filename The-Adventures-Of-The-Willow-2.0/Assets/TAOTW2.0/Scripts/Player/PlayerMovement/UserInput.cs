using EasyJoystick;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class UserInput : MonoBehaviour
{
    public static UserInput instance;
    [HideInInspector] public PlayerMoveAndExtraActions playerMoveAndExtraActions;
    [HideInInspector] public Vector2 moveInput;
    public Joysticknew joystickUI;

    // Estados dos botões
    [HideInInspector] public bool jumpButtonPressed = false;
    [HideInInspector] public bool jumpButtonReleased = false;
    private bool lastJumpButtonPressed = false;

    [HideInInspector] public bool grabButtonPressed = false;
    [HideInInspector] public bool grabButtonReleased = false;
    private bool lastGrabButtonPressed = false;

    [HideInInspector] public bool shootButtonPressed = false;
    [HideInInspector] public bool shootButtonReleased = false;
    private bool lastShootButtonPressed = false;

    // Métodos públicos para ligar nos botões UI (Event Trigger)
    public void OnJumpButtonDown() { jumpButtonPressed = true; }
    public void OnJumpButtonUp() { jumpButtonPressed = false; }

    public void OnGrabButtonDown() { grabButtonPressed = true; }
    public void OnGrabButtonUp() { grabButtonPressed = false; }

    public void OnShootButtonDown() { shootButtonPressed = true; }
    public void OnShootButtonUp() { shootButtonPressed = false; }


    [SerializeField] private GameObject[] TouchControls; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        playerMoveAndExtraActions = new PlayerMoveAndExtraActions();

        playerMoveAndExtraActions.PlayerActions.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();

#if UNITY_ANDROID || UNITY_IOS
        EnhancedTouchSupport.Enable();
        
        foreach (var obj in TouchControls)
        {
            if (obj != null)
                obj.SetActive(true);
        }
#else
        if (TouchControls != null)
        {
            foreach (var obj in TouchControls)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
#endif

        EnableInput();
    }

    private void Update()
    {
        // Detecta o momento do botão solto (released) para jump
        jumpButtonReleased = lastJumpButtonPressed && !jumpButtonPressed;
        lastJumpButtonPressed = jumpButtonPressed;

        // Detecta o momento do botão solto (released) para grab
        grabButtonReleased = lastGrabButtonPressed && !grabButtonPressed;
        lastGrabButtonPressed = grabButtonPressed;

        // Detecta o momento do botão solto (released) para shoot
        shootButtonReleased = lastShootButtonPressed && !shootButtonPressed;
        lastShootButtonPressed = shootButtonPressed; 

    }

    public Vector2 GetMoveInput()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (joystickUI == null)
        {
            joystickUI = Object.FindFirstObjectByType<EasyJoystick.Joysticknew>();
            if (joystickUI == null)
            {
                Debug.LogWarning("Joystick UI ainda não atribuído.");
                return Vector2.zero;
            }
        }

        Vector2 touchInput = new Vector2(joystickUI.Horizontal(), joystickUI.Vertical());
        if (touchInput.magnitude > 0.1f)
        {
            //Debug.Log("Input do Joystick ativo");
            return touchInput;
        }
#endif

        //Debug.Log("Input de teclado/alternativo");
        return moveInput;
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
