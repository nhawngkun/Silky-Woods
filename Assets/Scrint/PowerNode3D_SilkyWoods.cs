using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerNode3D_SilkyWoods : MonoBehaviour
{
    [Header("Lock Settings")]
    public bool isLocked = false;

    [Header("Energy Settings")]
    public float depletionDuration = 3f;
    public float rechargeDuration = 3f;
    public Transform[] energyObjects;

    [Header("Animation Settings")]
    public float fallSpeed = 2f;
    public float hideYPosition = -1f;

    [Header("Material Settings")]
    public Material targetMaterial;

    [Header("Color Settings")]
    public Color normalColor = Color.cyan;
    public Color depletedColor = Color.red;
    public float colorChangeDuration = 0.5f;

    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private bool isDepleted = false;
    private bool isRecharging = false;

    private List<Renderer> allRenderers = new List<Renderer>();
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

    private void Awake()
    {
        if (energyObjects != null && energyObjects.Length > 0)
        {
            foreach (Transform obj in energyObjects)
            {
                if (obj != null)
                {
                    originalPositions[obj] = obj.localPosition;
                }
            }
        }

        CollectAllRenderers();
    }

    void CollectAllRenderers()
    {
        if (targetMaterial != null)
        {
            Renderer mainRenderer = GetComponent<Renderer>();
            if (mainRenderer != null)
            {
                allRenderers.Add(mainRenderer);
                mainRenderer.material = targetMaterial;
                mainRenderer.material.color = normalColor;
            }

            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in childRenderers)
            {
                if (!allRenderers.Contains(rend))
                {
                    allRenderers.Add(rend);
                    rend.material = targetMaterial;
                    rend.material.color = normalColor;
                }
            }
        }
        else
        {
            Renderer mainRenderer = GetComponent<Renderer>();
            if (mainRenderer != null)
            {
                allRenderers.Add(mainRenderer);
            }

            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in childRenderers)
            {
                if (!allRenderers.Contains(rend))
                {
                    allRenderers.Add(rend);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLocked || isDepleted || isRecharging) return;

        if (other.CompareTag("Player"))
        {
            CableManager3D_SilkyWoods playerCable = other.GetComponent<CableManager3D_SilkyWoods>();
            if (playerCable != null)
            {
                playerCable.ConnectToNode(this);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isLocked || isDepleted || isRecharging) return;

        ElectricLine3D_SilkyWoods line = other.GetComponent<ElectricLine3D_SilkyWoods>();
        if (line != null)
        {
            if (line.endObj != null)
            {
                CableManager3D_SilkyWoods playerCable = line.endObj.GetComponentInParent<CableManager3D_SilkyWoods>();
                if (playerCable != null)
                {
                    playerCable.ConnectToNode(this);
                }
            }
        }
    }

    public void DepleteEnergy()
    {
        if (isDepleted || isRecharging) return;
        StartCoroutine(DepletionSequence());
    }

    private IEnumerator DepletionSequence()
    {
        isDepleted = true;
        isLocked = true;

        StartCoroutine(ChangeColorTo(depletedColor));

        if (energyObjects != null && energyObjects.Length > 0)
        {
            float delayBetweenFall = depletionDuration / energyObjects.Length;

            foreach (Transform obj in energyObjects)
            {
                if (obj != null)
                {
                    StartCoroutine(FallAndHide(obj));
                    yield return new WaitForSeconds(delayBetweenFall);
                }
            }

            yield return new WaitForSeconds(0.1f);

            foreach (Transform obj in energyObjects)
            {
                if (obj != null && originalPositions.ContainsKey(obj))
                {
                    Vector3 targetPos = originalPositions[obj];
                    obj.localPosition = targetPos;
                }
            }

            yield return new WaitForSeconds(3f);

            StartCoroutine(RechargeSequence());
        }
        else
        {
            yield return new WaitForSeconds(depletionDuration + 3f);
            isDepleted = false;
            isLocked = false;
            StartCoroutine(ChangeColorTo(normalColor));
        }
    }

    private IEnumerator FallAndHide(Transform obj)
    {
        if (obj == null) yield break;

        Vector3 startPos = obj.localPosition;
        Vector3 hidePos = new Vector3(startPos.x, hideYPosition, startPos.z);

        float elapsed = 0f;
        float fallDuration = Mathf.Abs(startPos.y - hideYPosition) / fallSpeed;

        while (elapsed < fallDuration && obj != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            obj.localPosition = Vector3.Lerp(startPos, hidePos, t);
            yield return null;
        }

        if (obj != null)
        {
            obj.localPosition = hidePos;
            obj.gameObject.SetActive(false);
        }
    }

    private IEnumerator RechargeSequence()
    {
        isRecharging = true;

        if (energyObjects != null && energyObjects.Length > 0)
        {
            float delayBetweenShow = rechargeDuration / energyObjects.Length;

            foreach (Transform obj in energyObjects)
            {
                if (obj != null)
                {
                    if (originalPositions.ContainsKey(obj))
                    {
                        obj.localPosition = originalPositions[obj];
                    }

                    obj.gameObject.SetActive(true);
                    yield return new WaitForSeconds(delayBetweenShow);
                }
            }
        }

        isDepleted = false;
        isRecharging = false;
        isLocked = false;

        StartCoroutine(ChangeColorTo(normalColor));
    }

    private IEnumerator ChangeColorTo(Color targetColor)
    {
        if (allRenderers.Count == 0) yield break;

        Dictionary<Renderer, Color> startColors = new Dictionary<Renderer, Color>();

        foreach (Renderer rend in allRenderers)
        {
            if (rend != null && rend.material != null)
            {
                startColors[rend] = rend.material.color;
            }
        }

        float elapsed = 0f;

        while (elapsed < colorChangeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / colorChangeDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            foreach (Renderer rend in allRenderers)
            {
                if (rend != null && rend.material != null && startColors.ContainsKey(rend))
                {
                    rend.material.color = Color.Lerp(startColors[rend], targetColor, t);
                }
            }

            yield return null;
        }

        foreach (Renderer rend in allRenderers)
        {
            if (rend != null && rend.material != null)
            {
                rend.material.color = targetColor;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (isDepleted)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else if (isRecharging)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}