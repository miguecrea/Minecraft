using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions inputActions;

    private InputAction m_moveAction;
    private InputAction m_lookAction;

    //READ ONLY
    public Vector2 m_Move { get; private set; }
    public Vector2 m_Look { get; private set; }

    public event Action m_OnJump;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        inputActions = new InputSystem_Actions();

        m_moveAction = inputActions.Player.Move;
        m_lookAction = inputActions.Player.Look;
    
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        // STATE updates
        m_moveAction.performed += OnMove;
        m_moveAction.canceled += OnMove;

        m_lookAction.performed += OnLook;
        m_lookAction.canceled += OnLook;

    }

    private void OnDisable()
    {
        m_moveAction.performed -= OnMove;
        m_moveAction.canceled -= OnMove;

        m_lookAction.performed -= OnLook;
        m_lookAction.canceled -= OnLook;
        inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        m_Move = ctx.ReadValue<Vector2>();

        Debug.Log("Move");
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        m_Look = ctx.ReadValue<Vector2>();
    }
    
}