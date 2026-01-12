using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ElectricZone_SilkyWoods : MonoBehaviour
{
    [Header("Cấu hình hình ảnh")]
    public Material webMaterial;
    public Material activeLineMaterial;

    [Header("VFX")]
    public GameObject zoneCenterVFX;

    private List<PowerNode3D_SilkyWoods> affectedNodes = new List<PowerNode3D_SilkyWoods>();
    private List<GameObject> linesToDestroy = new List<GameObject>();
    private List<GameObject> linesBeforeLoop = new List<GameObject>();
    private float duration = 2f;
    private GameObject vfxInstance;

    public void Initialize(List<PowerNode3D_SilkyWoods> nodes, List<GameObject> loopLines, List<GameObject> beforeLoopLines, float lifeTime)
    {
        affectedNodes = new List<PowerNode3D_SilkyWoods>(nodes);
        linesToDestroy = new List<GameObject>(loopLines);
        linesBeforeLoop = new List<GameObject>(beforeLoopLines);
        duration = lifeTime;

        // Khóa node
        foreach (var node in affectedNodes)
        {
            if (node != null)
            {
                node.isLocked = true;

                // GỌI HÀM DEPLETENERGY ĐỂ BẮT ĐẦU ANIMATION HẾT NĂNG LƯỢNG
                node.DepleteEnergy();
            }
        }

        // Thay đổi Material cho các dây TRONG loop
        if (activeLineMaterial != null)
        {
            foreach (GameObject lineObj in linesToDestroy)
            {
                if (lineObj != null)
                {
                    ElectricLine3D_SilkyWoods lineScript = lineObj.GetComponent<ElectricLine3D_SilkyWoods>();
                    if (lineScript != null)
                    {
                        lineScript.SetLineMaterial(activeLineMaterial);
                    }
                }
            }
        }

        // XÓA NGAY các lines TRƯỚC loop
        foreach (GameObject lineObj in linesBeforeLoop)
        {
            if (lineObj != null)
            {
                Destroy(lineObj);
            }
        }

        // Gán Material cho vùng lưới
        if (webMaterial != null)
        {
            GetComponent<MeshRenderer>().material = webMaterial;
        }

        // Tạo lưới mặt phẳng
        CreateFlatWebMesh(nodes);

        // Tạo VFX tại trung tâm zone
        SpawnCenterVFX(nodes);

        Destroy(gameObject, duration);
    }

    void SpawnCenterVFX(List<PowerNode3D_SilkyWoods> nodes)
    {
        if (zoneCenterVFX == null) return;

        Vector3 center = Vector3.zero;
        foreach (var node in nodes)
        {
            if (node != null)
            {
                center += node.transform.position;
            }
        }
        center /= nodes.Count;

        center.y = 2f;

        float maxDistance = 0f;
        foreach (var node in nodes)
        {
            if (node != null)
            {
                float distance = Vector3.Distance(center, node.transform.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
        }

        vfxInstance = Instantiate(zoneCenterVFX, center, Quaternion.identity);

        float scaleFactor = Mathf.Clamp(maxDistance / 2f, 1.2f, 6f);
        vfxInstance.transform.localScale = Vector3.one * scaleFactor;

        vfxInstance.transform.SetParent(transform);
    }

    void CreateFlatWebMesh(List<PowerNode3D_SilkyWoods> nodes)
    {
        int count = nodes.Count;
        if (count < 3) return;

        Vector3[] vertices = new Vector3[count * 2];

        transform.position = nodes[0].transform.position;

        float minX = Mathf.Infinity, minZ = Mathf.Infinity;
        float maxX = -Mathf.Infinity, maxZ = -Mathf.Infinity;

        for (int i = 0; i < count; i++)
        {
            Vector3 localPos = nodes[i].transform.position - transform.position;
            localPos.y += 0.05f;

            vertices[i] = localPos;
            vertices[i + count] = localPos;

            if (localPos.x < minX) minX = localPos.x;
            if (localPos.z < minZ) minZ = localPos.z;
            if (localPos.x > maxX) maxX = localPos.x;
            if (localPos.z > maxZ) maxZ = localPos.z;
        }

        List<int> triangles = new List<int>();

        for (int i = 1; i < count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        for (int i = 1; i < count - 1; i++)
        {
            triangles.Add(count);
            triangles.Add(count + i + 1);
            triangles.Add(count + i);
        }

        Vector2[] uvs = new Vector2[count * 2];
        float width = maxX - minX;
        float depth = maxZ - minZ;
        if (width <= 0) width = 1;
        if (depth <= 0) depth = 1;

        for (int i = 0; i < count; i++)
        {
            Vector2 uv = new Vector2((vertices[i].x - minX) / width, (vertices[i].z - minZ) / depth);
            uvs[i] = uv;
            uvs[i + count] = uv;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        mc.convex = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyBase_SilkyWoods enemy = other.GetComponent<EnemyBase_SilkyWoods>();
            if (enemy != null)
            {
                enemy.Electrocute();
            }
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Enemy"))
        {
            EnemyBase_SilkyWoods enemy = coll.gameObject.GetComponent<EnemyBase_SilkyWoods>();
            if (enemy != null)
            {
                enemy.Electrocute();
            }
        }
    }

    private void OnDestroy()
    {
        // KHÔNG CẦN UNLOCK NỮA vì PowerNode3D tự unlock sau khi recharge xong

        // Chỉ xóa lines trong loop
        foreach (var line in linesToDestroy)
        {
            if (line != null) Destroy(line);
        }

        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }
    }
}