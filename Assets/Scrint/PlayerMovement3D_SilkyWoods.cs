using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement3D_SilkyWoods : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 8f;
    public float rotateSpeed = 15f;

    [Header("Input Method")]
    public bool useJoystick = true;
    public VariableJoystick_SilkyWoods joystick;

    [Header("Debug")]
    public bool showDebug = true;

    [HideInInspector] public bool isKnockedBack = false;

    private Rigidbody rb;
    [SerializeField] private Animator animator;
    private Vector3 movementInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;

        // Tự động tìm joystick
        if (joystick == null && useJoystick)
        {
            joystick = FindFirstObjectByType<VariableJoystick_SilkyWoods>();
            if (joystick == null)
            {
                Debug.LogWarning("[Player] Joystick not found! Using keyboard input.");
                useJoystick = false;
            }
            else
            {
                Debug.Log($"[Player] Found joystick: {joystick.gameObject.name}");
            }
        }
    }

    void Update()
    {
        // Get input
        
        // Get input
        float moveX = 0f;
        float moveZ = 0f;

        if (useJoystick && joystick != null)
        {
            moveX = joystick.Horizontal;
            moveZ = joystick.Vertical;
        }
        else
        {
            moveX = Input.GetAxisRaw("Horizontal");
            moveZ = Input.GetAxisRaw("Vertical");
        }

        // ✅ FIX LỖI: Thay vì .normalized ngay lập tức, ta kiểm tra độ lớn trước
        Vector3 rawInput = new Vector3(moveX, 0f, moveZ);

        if (rawInput.magnitude > 0.1f) // Chỉ di chuyển nếu input đủ lớn (Deadzone của Player)
        {
            // Nếu dùng Joystick, giữ nguyên độ lớn để đi chậm/nhanh (nếu muốn)
            // Hoặc normalized nếu muốn luôn đi max tốc độ
            movementInput = rawInput.normalized; 
        }
        else
        {
            // ✅ QUAN TRỌNG: Nếu input quá nhỏ (do thả tay), ép về 0 tuyệt đối
            movementInput = Vector3.zero;
        }

        UpdateAnimator();
    }
    

    void FixedUpdate()
    {
        if (!isKnockedBack)
        {
            MovePlayer();
            RotatePlayer();
        }
    }

    void MovePlayer()
    {
        // ✅ Chỉ di chuyển khi có input
        if (movementInput.magnitude > 0.01f)
        {
            Vector3 targetVelocity = movementInput * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            
        }
        else
        {
            // ✅ DỪNG NGAY khi không có input
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            
           
        }
    }

    void RotatePlayer()
    {
        if (movementInput != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementInput, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotateSpeed * Time.deltaTime);
        }
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            if (isKnockedBack)
            {
                animator.SetBool("run", false);
            }
            else
            {
                bool isMoving = movementInput.magnitude > 0.1f;
                animator.SetBool("run", isMoving);
            }
        }
    }

    
}