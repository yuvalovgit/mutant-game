using UnityEngine;

namespace MutantSurvivors
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float sprintSpeed = 14f;
        [SerializeField] private float jumpHeight = 2.5f;
        [SerializeField] private float gravity = -18f;

        [Header("Camera")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float pitchMin = -20f;
        [SerializeField] private float pitchMax = 60f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 3f, -6f);

        [Header("References")]
        [SerializeField] private Transform playerModel;
        [SerializeField] private PlayerCombat playerCombat;

        private Transform mainCamera;
        private CharacterController cc;
        private Animator animator;
        private PlayerStats playerStats;

        private float yaw;
        private float pitch;
        private float verticalVelocity;
        private float animSpeed;

        private void Awake()
        {
            // Get or add CharacterController
            cc = GetComponent<CharacterController>();
            if (cc == null) cc = gameObject.AddComponent<CharacterController>();
            cc.center = new Vector3(0, 1, 0);
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.skinWidth = 0.08f;
            cc.stepOffset = 0.4f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            playerStats = PlayerStats.Instance;
            animator = GetComponentInChildren<Animator>(true);

            // Auto-find camera
            mainCamera = Camera.main?.transform;
            if (mainCamera != null) mainCamera.SetParent(null);

            // Auto-find player model
            if (playerModel == null && transform.childCount > 0)
                playerModel = transform.GetChild(0);

            yaw = transform.eulerAngles.y;

            Debug.Log("[PC] Start — cc=" + (cc != null) + " cam=" + (mainCamera != null) + " model=" + (playerModel != null ? playerModel.name : "NULL"));
        }

        private void Update()
        {
            HandleCamera();
            HandleMovement();
            HandleActions();
            UpdateAnimator();
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;

            Quaternion camRot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPos = transform.position + camRot * cameraOffset;

            // Camera collision
            Vector3 playerHead = transform.position + Vector3.up * 2f;
            if (Physics.Linecast(playerHead, desiredPos, out RaycastHit hit))
                desiredPos = hit.point + hit.normal * 0.3f;

            mainCamera.position = Vector3.Lerp(mainCamera.position, desiredPos, Time.deltaTime * 10f);
            mainCamera.LookAt(transform.position + Vector3.up * 1.5f);
        }

        private void HandleCamera()
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            bool grounded = cc.isGrounded;
            if (grounded && verticalVelocity < 0f) verticalVelocity = -2f;
            if (Input.GetKeyDown(KeyCode.Space) && grounded)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, -20f);

            Vector3 move = Vector3.zero;
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);

            if (h != 0f || v != 0f)
            {
                // Movement relative to camera
                Vector3 camForward = Vector3.ProjectOnPlane(mainCamera.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(mainCamera.right, Vector3.up).normalized;
                Vector3 moveDir = (camForward * v + camRight * h).normalized;

                float speed = isSprinting ? sprintSpeed : moveSpeed;
                if (playerStats != null)
                    speed = isSprinting ? playerStats.SprintSpeed : playerStats.MoveSpeed;

                move = moveDir * speed;

                // Rotate model to face movement direction
                if (playerModel != null && moveDir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir);
                    playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRot, Time.deltaTime * 12f);
                }

                animSpeed = isSprinting ? 2f : 1f;
            }
            else
            {
                animSpeed = 0f;
            }

            move.y = verticalVelocity;
            cc.Move(move * Time.deltaTime);
        }

        private void UpdateAnimator()
{
    if (animator != null)
        animator.SetFloat("Speed", animSpeed);
    
    bool isShooting = Input.GetMouseButton(0);
    playerCombat?.SetShooting(isShooting);
}

private void HandleActions()
{
    if (Input.GetMouseButtonDown(0)) playerCombat?.TryFire();
    if (Input.GetKeyDown(KeyCode.E)) playerCombat?.TryInteract();
    if (Input.GetKeyDown(KeyCode.Q)) playerCombat?.UseActiveAbility();
}
    }
}
