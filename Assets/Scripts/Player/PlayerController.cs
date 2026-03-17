using UnityEngine;

namespace MutantSurvivors
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Camera Rig")]
        [SerializeField] private Transform cameraRig;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform mainCamera;
        [SerializeField] private float cameraRigHeight = 3.5f;

        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float pitchMin = -30f;
        [SerializeField] private float pitchMax = 60f;

        [Header("Movement")]
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float jumpHeight = 2.5f;

        private CharacterController characterController;
        private PlayerStats playerStats;
        private PlayerCombat playerCombat;
        private Animator animator;

        private float verticalVelocity;
        private float yaw;
        private float pitch;
        private float animSpeed;
        private bool isSprinting;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerCombat = GetComponent<PlayerCombat>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            playerStats = PlayerStats.Instance;
            yaw = transform.eulerAngles.y;
            pitch = 0f;
            animator = GetComponentInChildren<Animator>(true);
            if (animator == null)
                Debug.LogError("[PlayerController] ANIMATOR NOT FOUND!");
            else
                animator.applyRootMotion = false;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            HandleActions();
            UpdateAnimator();
        }

        private void LateUpdate()
        {
            if (cameraRig != null)
                cameraRig.position = transform.position + Vector3.up * cameraRigHeight;
        }

        private void HandleLook()
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            bool grounded = characterController.isGrounded;
            if (grounded && verticalVelocity < 0f) verticalVelocity = -2f;
            if (Input.GetKeyDown(KeyCode.Space) && grounded)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            verticalVelocity += gravity * Time.deltaTime;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            isSprinting = Input.GetKey(KeyCode.LeftShift);

            Vector3 move = Vector3.zero;
            if (h != 0f || v != 0f)
            {
                move = new Vector3(h, 0f, v).normalized;
                move = Quaternion.Euler(0f, yaw, 0f) * move;
                float speed = (playerStats != null)
                    ? (isSprinting ? playerStats.SprintSpeed : playerStats.MoveSpeed)
                    : (isSprinting ? 14f : 8f);
                move *= speed;
            }

            float inputMag = Mathf.Clamp01(new Vector2(h, v).magnitude);
            animSpeed = inputMag * (isSprinting ? 2f : 1f);

            move.y = verticalVelocity;
            characterController.Move(move * Time.deltaTime);
        }

        private void UpdateAnimator()
        {
            if (animator != null)
                animator.SetFloat("Speed", animSpeed);
        }

        private void HandleActions()
        {
            if (Input.GetMouseButtonDown(0)) playerCombat?.TryFire();
            if (Input.GetKeyDown(KeyCode.E)) playerCombat?.TryInteract();
            if (Input.GetKeyDown(KeyCode.Q)) playerCombat?.UseActiveAbility();
        }
    }
}
```

**Save the file.**

---

Then in Unity make sure your hierarchy looks like this:
```
Player
  └── CameraRig (Position: 0, 3.5, 0)
        └── CameraPivot
              └── Main Camera (Position: 0, 0, -6)