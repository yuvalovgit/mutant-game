using UnityEngine;

namespace MutantSurvivors
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Camera Rig")]
        [SerializeField] private Transform cameraRig;    // child at offset (0, 3.5, 0)
        [SerializeField] private Transform cameraPivot;  // child of rig
        [SerializeField] private Transform mainCamera;   // child of pivot at offset (0, 0, -6)

        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float pitchMin = -30f;
        [SerializeField] private float pitchMax =  60f;

        [Header("Movement")]
        [SerializeField] private float gravity    = -18f;
        [SerializeField] private float jumpHeight =  2.5f;

        // ── Components ───────────────────────────────────────────────────────────
        private CharacterController characterController;
        private PlayerStats         playerStats;
        private PlayerCombat        playerCombat;

        // ── Internal state ───────────────────────────────────────────────────────
        private float verticalVelocity;
        private float yaw;
        private float pitch;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerCombat        = GetComponent<PlayerCombat>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void Start()
        {
            playerStats = PlayerStats.Instance;
            yaw   = transform.eulerAngles.y;
            pitch = 0f;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            HandleActions();
        }

        // ── Look ─────────────────────────────────────────────────────────────────

        private void HandleLook()
        {
            yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch  = Mathf.Clamp(pitch, pitchMin, pitchMax);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // ── Movement ─────────────────────────────────────────────────────────────

        private void HandleMovement()
        {
            bool grounded = characterController.isGrounded;

            if (grounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Input.GetKeyDown(KeyCode.Space) && grounded)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            verticalVelocity += gravity * Time.deltaTime;

            float h = Input.GetAxisRaw("Horizontal");   // A/D
            float v = Input.GetAxisRaw("Vertical");     // W/S

            Vector3 move = Vector3.zero;
            if (h != 0f || v != 0f)
            {
                move = new Vector3(h, 0f, v).normalized;
                move = Quaternion.Euler(0f, yaw, 0f) * move;

                bool sprinting = Input.GetKey(KeyCode.LeftShift);
                float speed = (playerStats != null)
                    ? (sprinting ? playerStats.SprintSpeed : playerStats.MoveSpeed)
                    : (sprinting ? 14f : 8f);

                move *= speed;
            }

            move.y = verticalVelocity;
            characterController.Move(move * Time.deltaTime);
        }

        // ── Actions ───────────────────────────────────────────────────────────────

        private void HandleActions()
        {
            if (Input.GetMouseButtonDown(0))   playerCombat?.TryFire();
            if (Input.GetKeyDown(KeyCode.E))   playerCombat?.TryInteract();
            if (Input.GetKeyDown(KeyCode.Q))   playerCombat?.UseActiveAbility();
        }
    }
}
