using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private CharacterController m_controller;
    private InputManager m_InputManagerReference;

    [SerializeField]
    private CinemachineCamera m_CineMachineCamera;


    [SerializeField]
    Transform m_CineMachineCameraFollow;

    //INPUT 
    private Vector2 m_MoveInput;
    private Vector2 m_LookInput;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask m_GroundLayers;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    //CAMERA VARIBALES
    private const float m_LookThreshold = 0.01f;
    private float m_cinemachineTargetYaw;
    private float m_cinemachineTargetPitch;
    private bool m_IsCurrentDeviceMouse = true;

    //GRAVITY 
    [SerializeField] private float m_gravity = -9.81f;
    [SerializeField] private float m_groundedOffset = -0.1f;
    [SerializeField] private float m_groundedRadius = 0.3f;


    //Ground check Game Object
    [SerializeField] private Transform m_groundCheck;

    private float m_verticalVelocity;
    private bool m_isGrounded;

    void OnEnable()
    {

    }
    private void OnDisable()
    {

    }

    private void Awake()
    {
        if (m_CineMachineCamera == null) Debug.LogError("CineMachine camera not found Error");
        m_controller = GetComponent<CharacterController>();
        m_InputManagerReference = InputManager.Instance;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
    }
    private void LateUpdate()
    {
        CameraRotation();
    }


    private void OnDrawGizmosSelected()
    {
        if (m_groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_groundCheck.position,m_groundedRadius);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

    }

    private void HandleMovement()
    {
        m_MoveInput = m_InputManagerReference.m_Move;
        m_LookInput = m_InputManagerReference.m_Look;


        GroundCheck();
        ApplyGravity();

        Vector3 camForward = m_CineMachineCamera.transform.forward;
        Vector3 camRight = m_CineMachineCamera.transform.right;

        //Remove vertical influence so vector is proyected onto XZ plane 
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();
        // Convert input to camera-relative movement
        Vector3 move = camForward * m_MoveInput.y + camRight * m_MoveInput.x;

        Vector3 velocity = move * 10f;
        velocity.y = m_verticalVelocity;

        m_controller.Move(velocity * Time.deltaTime);

    }
    private void CameraRotation()
    {
        if (m_LookInput.sqrMagnitude >= m_LookThreshold)
        {
            //make a variballe
            float sensitivity = 0.2f; // 
            float deltaTimeMultiplier = m_IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            m_cinemachineTargetYaw += m_LookInput.x * deltaTimeMultiplier * sensitivity;
            m_cinemachineTargetPitch -= m_LookInput.y * deltaTimeMultiplier * sensitivity;
        }

        // Clamp pitch ONLY
        m_cinemachineTargetPitch = Mathf.Clamp(m_cinemachineTargetPitch, BottomClamp, TopClamp);

        // ✅ YAW → rotate player (left/right)
        transform.rotation = Quaternion.Euler(0.0f, m_cinemachineTargetYaw, 0.0f);

        // ✅ PITCH → rotate camera target (up/down)
        m_CineMachineCameraFollow.transform.localRotation = Quaternion.Euler(m_cinemachineTargetPitch, m_cinemachineTargetYaw, 0.0f);
    }

    private void GroundCheck()
    {
        m_isGrounded = Physics.CheckSphere(
        m_groundCheck.position,
        m_groundedRadius,
        m_GroundLayers,
        QueryTriggerInteraction.Ignore
    );

    }

    private void ApplyGravity()
    {
        if (m_isGrounded && m_verticalVelocity < 0)
        {
            m_verticalVelocity = -2f; // small downward force to stick to ground
        }

        m_verticalVelocity += m_gravity * Time.deltaTime;
    }


}
