using UnityEngine;
using System.Collections;

public class EnemyPro_SilkyWoods : EnemyBase_SilkyWoods
{
    [Header("Dash Settings")]
    public float dashCooldown = 5f;
    public float dashChargeTime = 2f;
    public float dashSpeed = 15f;
    public float dashDistance = 5f;

    [Header("Visual")]
    public GameObject chargeVFX;

    private float lastDashTime = -999f;
    private bool isCharging = false;
    private bool isDashing = false;
    private GameObject currentChargeVFX;
    private Quaternion chargeRotation; // Lưu rotation khi bắt đầu charge

    protected override void AIBehavior()
    {
        if (isDashing || player == null) return;

        // Kiểm tra cooldown dash
        if (Time.time - lastDashTime >= dashCooldown && !isCharging)
        {
            StartCoroutine(DashSequence());
        }
        else if (!isCharging)
        {
            // Di chuyển bình thường
            MoveTowardsPlayer();
        }
    }

    IEnumerator DashSequence()
    {
        isCharging = true;

        // Dừng lại để charge
        rb.linearVelocity = Vector3.zero;

        // LƯU ROTATION HƯỚNG VỀ PLAYER VÀ CỐ ĐỊNH
        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                chargeRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = chargeRotation;
            }
        }

        // Spawn VFX charge
        if (chargeVFX != null)
        {
            currentChargeVFX = Instantiate(chargeVFX, transform.position, Quaternion.identity);
            currentChargeVFX.transform.SetParent(transform);
        }

        // Chờ charge - GIỮ NGUYÊN ROTATION
        float chargeTimer = 0f;
        while (chargeTimer < dashChargeTime)
        {
            // Cố định rotation trong khi charge
            transform.rotation = chargeRotation;

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        // Xóa VFX charge
        if (currentChargeVFX != null)
        {
            Destroy(currentChargeVFX);
        }

        isCharging = false;

        // Bắt đầu dash
        if (player != null && !isDying)
        {
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        isDashing = true;

        // Tính hướng dash (sử dụng rotation đã lưu)
        Vector3 dashDirection = transform.forward;
        dashDirection.y = 0;

        float dashDuration = dashDistance / dashSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration && !isDying)
        {
            rb.linearVelocity = new Vector3(
                dashDirection.x * dashSpeed,
                rb.linearVelocity.y,
                dashDirection.z * dashSpeed
            );

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Dừng lại
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        isDashing = false;
        lastDashTime = Time.time;
    }
}