using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth_SilkyWoods : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth = 3;

    [Header("UI")]
    public Image[] heartImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Damage Settings")]
    public float invincibilityTime = 1.0f;
    private float lastDamageTime = -999f;

    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;

    [Header("Flash Settings")]
    public float flashDuration = 0.1f;

    [Tooltip("Kéo tất cả các Renderer vào đây")]
    [SerializeField] private List<Renderer> playerRenderers = new List<Renderer>();

    private Rigidbody rb;
    private PlayerMovement3D_SilkyWoods playerMovement;
    private Collider playerCollider;

    [Header("VFX")]
    public GameObject damageVFX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement3D_SilkyWoods>();

        playerCollider = GetComponent<Collider>();
        if (playerCollider == null)
        {
            playerCollider = GetComponentInChildren<Collider>();
        }
        if (playerCollider == null)
        {
            Debug.LogError("Player Collider not found!");
        }

        if (playerRenderers.Count == 0)
        {
            Renderer singleRenderer = GetComponent<Renderer>();
            if (singleRenderer == null)
            {
                singleRenderer = GetComponentInChildren<Renderer>();
            }

            if (singleRenderer != null)
            {
                playerRenderers.Add(singleRenderer);
            }
        }

        // Khởi tạo health từ GameStatsManager nếu có
        if (GameStatsManager_SilkyWoods.Instance != null)
        {
            maxHealth = GameStatsManager_SilkyWoods.Instance.GetPlayerMaxHealth();
            // Không reset currentHealth ở đây, giữ nguyên giá trị ban đầu (3)
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damage, Vector3 otherPosition)
    {
        if (Time.time - lastDamageTime < invincibilityTime) return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        StartCoroutine(Knockback(otherPosition));
        StartCoroutine(FlashSequence(invincibilityTime));

        if (damageVFX != null)
        {
            Instantiate(damageVFX, transform.position, Quaternion.identity);
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator Knockback(Vector3 otherPosition)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found!");
            yield break;
        }

        if (playerMovement != null)
        {
            playerMovement.isKnockedBack = true;
        }

        Vector3 direction = (transform.position - otherPosition).normalized;
        direction.y = 0;

        rb.linearVelocity = direction * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        if (playerMovement != null)
        {
            playerMovement.isKnockedBack = false;
        }

        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.x = 0;
        currentVelocity.z = 0;
        rb.linearVelocity = currentVelocity;
    }

    private IEnumerator FlashSequence(float duration)
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        if (playerRenderers.Count == 0)
        {
            yield return new WaitForSeconds(duration);
        }
        else
        {
            float timer = 0f;
            while (timer < duration)
            {
                foreach (Renderer rend in playerRenderers)
                {
                    if (rend != null)
                    {
                        rend.enabled = !rend.enabled;
                    }
                }

                yield return new WaitForSeconds(flashDuration);
                timer += flashDuration;
            }

            foreach (Renderer rend in playerRenderers)
            {
                if (rend != null)
                {
                    rend.enabled = true;
                }
            }
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        if (heartImages == null || heartImages.Length == 0) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < maxHealth)
            {
                heartImages[i].gameObject.SetActive(true);
                heartImages[i].sprite = (i < currentHealth) ? fullHeart : emptyHeart;
            }
            else
            {
                heartImages[i].gameObject.SetActive(false);
            }
        }
    }

    void Die()
    {
         SoundManager_SilkyWoods.Instance.PlayVFXSound(2);

        Debug.Log("Player Died!");

        // Hiện UI Loss
        if (UIManager_SilkyWoods.Instance != null)
        {
            // Đóng UI Gameplay
            UIManager_SilkyWoods.Instance.CloseUIDirectly<UIGameplay_SilkyWoods>();

            // Mở UI Loss
            UIManager_SilkyWoods.Instance.OpenUI<UILoss_SilkyWoods>();
        }

        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Stop player movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}