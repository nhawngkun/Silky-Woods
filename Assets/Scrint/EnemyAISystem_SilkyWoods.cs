// ==================== EnemyBase.cs ====================
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public abstract class EnemyBase_SilkyWoods : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float moveSpeed = 3f;

    [Header("VFX")]
    public GameObject electrocutionVFX; // VFX bị giật điện
    public GameObject destructionVFX;   // VFX khi bị phá hủy

    protected Transform player;
    protected Rigidbody rb;
    protected CapsuleCollider col;
    protected bool isDying = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        if (electrocutionVFX != null)
            electrocutionVFX.SetActive(false);

        if (destructionVFX != null)
            destructionVFX.SetActive(false);

        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Collision tốt hơn

        // Đảm bảo collider hoạt động
        col.isTrigger = false;

        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    protected virtual void FixedUpdate()
    {
        if (isDying || player == null) return;
        AIBehavior();
    }

    protected abstract void AIBehavior();

    protected void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Không bay lên trời

        rb.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            rb.linearVelocity.y,
            direction.z * moveSpeed
        );

        // Quay mặt về player
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
    }

    // Bị giật điện bởi zone
    public void Electrocute()
    {
        if (isDying) return;
        isDying = true;

        // Tắt collider
        col.enabled = false;

        // Dừng di chuyển
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Bật VFX giật điện
        if (electrocutionVFX != null)
        {
            electrocutionVFX.SetActive(true);
        }

        // Sau 0.7s thì chuyển sang destruction VFX
        Invoke("SwitchToDestructionVFX", 0.7f);

        // Sau 1s thì destroy enemy
        Invoke("DestroyEnemy", 1f);
    }

    void SwitchToDestructionVFX()
    {
        // Tắt electrocution VFX
        if (electrocutionVFX != null)
        {
            electrocutionVFX.SetActive(false);
        }

        // Bật destruction VFX
        if (destructionVFX != null)
        {
            destructionVFX.SetActive(true);
        }
    }

    void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // ... (Kiểm tra isDying giữ nguyên)

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth_SilkyWoods playerHealth = collision.gameObject.GetComponent<PlayerHealth_SilkyWoods>();
            if (playerHealth != null)
            {
                // Truyền vị trí của Enemy để Player tính toán hướng Knockback
                playerHealth.TakeDamage(1, transform.position);
            }
        }
    }
}