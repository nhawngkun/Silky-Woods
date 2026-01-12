using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CableManager3D_SilkyWoods : MonoBehaviour
{
    [Header("Setup")]
    public GameObject electricLinePrefab;
    public GameObject electricZonePrefab;
    public Transform buttPosition;

    private ElectricLine3D_SilkyWoods currentLineScript;
    private PowerNode3D_SilkyWoods lastNodeScript;

    private List<PowerNode3D_SilkyWoods> nodeHistory = new List<PowerNode3D_SilkyWoods>();
    private List<GameObject> lineHistory = new List<GameObject>();

    private class NodeConnection
    {
        public PowerNode3D_SilkyWoods nodeA;
        public PowerNode3D_SilkyWoods nodeB;
        public GameObject line;

        public bool IsSameConnection(PowerNode3D_SilkyWoods n1, PowerNode3D_SilkyWoods n2)
        {
            return (nodeA == n1 && nodeB == n2) || (nodeA == n2 && nodeB == n1);
        }
    }

    private List<NodeConnection> activeConnections = new List<NodeConnection>();

    private void OnEnable()
    {
        ResetConnectionState();
    }

    private void OnDisable()
    {
        CleanupAllConnections();
    }

    public void ConnectToNode(PowerNode3D_SilkyWoods nodeScript)
    {
        Debug.Log($"[CableManager] ConnectToNode called: {nodeScript.name}");
        
        if (lastNodeScript == nodeScript) return;

        if (nodeHistory.Contains(nodeScript))
        {
            int startIndex = nodeHistory.IndexOf(nodeScript);
            int loopNodeCount = nodeHistory.Count - startIndex;

            // ✅ KIỂM TRA: Phải có ít nhất 3 node để tạo hình khép kín
            if (loopNodeCount >= 3)
            {
                // Lock line hiện tại về node đóng loop
                if (currentLineScript != null)
                {
                    currentLineScript.LockLine(nodeScript.transform.position);
                }

                // ===== THU THẬP CÁC LINE TRONG LOOP =====
                List<GameObject> loopLines = new List<GameObject>();

                // Bước 1: Thêm các line trong lineHistory từ startIndex
                for (int i = startIndex; i < lineHistory.Count; i++)
                {
                    if (lineHistory[i] != null && !loopLines.Contains(lineHistory[i]))
                    {
                        loopLines.Add(lineHistory[i]);
                        Debug.Log($"[CableManager] Added line from lineHistory[{i}]");
                    }
                }

                // Bước 2: ✅ QUAN TRỌNG - Thêm currentLine (line đóng loop)
                if (currentLineScript != null && !loopLines.Contains(currentLineScript.gameObject))
                {
                    loopLines.Add(currentLineScript.gameObject);
                    Debug.Log($"[CableManager] Added currentLine (closing line)");
                }

                // Bước 3: Thêm các line từ activeConnections trong vùng loop
                for (int i = startIndex; i < nodeHistory.Count; i++)
                {
                    PowerNode3D_SilkyWoods node1 = nodeHistory[i];
                    PowerNode3D_SilkyWoods node2 = (i + 1 < nodeHistory.Count) ? nodeHistory[i + 1] : nodeScript;

                    NodeConnection conn = activeConnections.FirstOrDefault(
                        c => c.IsSameConnection(node1, node2)
                    );

                    if (conn != null && conn.line != null && !loopLines.Contains(conn.line))
                    {
                        loopLines.Add(conn.line);
                        Debug.Log($"[CableManager] Added line from activeConnections: {node1.name} → {node2.name}");
                    }
                }

                // ===== KIỂM TRA SỐ LƯỢNG LINE =====
                // ✅ Công thức: Hình khép kín cần số line = số node
                // Tam giác: 3 node, 3 line (A→B, B→C, C→A)
                // Tứ giác: 4 node, 4 line
                int requiredLines = loopNodeCount;
                
                Debug.Log($"[CableManager] ===== LOOP DETECTED =====");
                Debug.Log($"[CableManager] Closing node: {nodeScript.name}");
                Debug.Log($"[CableManager] Start index: {startIndex}");
                Debug.Log($"[CableManager] Loop node count: {loopNodeCount}");
                Debug.Log($"[CableManager] Loop lines collected: {loopLines.Count}");
                Debug.Log($"[CableManager] Required lines: {requiredLines}");
                
                // In ra tên các line trong loop
                for (int i = 0; i < loopLines.Count; i++)
                {
                    if (loopLines[i] != null)
                    {
                        Debug.Log($"[CableManager]   Line {i}: {loopLines[i].name}");
                    }
                }
                Debug.Log($"[CableManager] ========================");

                // ✅ Chỉ tạo zone nếu có đủ line
                if (loopLines.Count >= requiredLines)
                {
                    // ✅ FIX: Chỉ xóa các line NGOÀI loop (từ đầu đến startIndex - 1)
                    // lineHistory[0 -> startIndex-1] là các line TRƯỚC khi bắt đầu loop
                    List<GameObject> linesToDestroyBeforeLoop = new List<GameObject>();
                    
                    // Tính số line trước loop: startIndex node tạo ra (startIndex - 1) line
                    int linesBeforeLoop = startIndex; // Số line = số node trước loop
                    
                    for (int i = 0; i < linesBeforeLoop && i < lineHistory.Count; i++)
                    {
                        GameObject line = lineHistory[i];
                        
                        // ✅ QUAN TRỌNG: Chỉ thêm vào list xóa nếu line KHÔNG thuộc loop
                        if (line != null && !loopLines.Contains(line))
                        {
                            linesToDestroyBeforeLoop.Add(line);
                        }
                    }

                    Debug.Log($"[CableManager] ✅ Creating zone with {loopNodeCount} nodes and {loopLines.Count} lines");
                    Debug.Log($"[CableManager] Lines to keep in zone: {loopLines.Count}, Lines to destroy before loop: {linesToDestroyBeforeLoop.Count}");
                    
                    CreateElectricLoop(nodeScript, loopLines, linesToDestroyBeforeLoop);

                    ResetConnectionState();
                    currentLineScript = null;
                    return;
                }
                else
                {
                    Debug.LogWarning($"[CableManager] ❌ Not enough lines! Need {requiredLines} but only have {loopLines.Count}");
                    // Không tạo zone, tiếp tục vẽ line bình thường
                }
            }
        }

        // Xử lý line bình thường (không phải loop)
        if (currentLineScript != null && lastNodeScript != null)
        {
            currentLineScript.LockLine(nodeScript.transform.position);

            GameObject lineToKeep = MergeOrAddConnection(lastNodeScript, nodeScript, currentLineScript.gameObject);

            if (lineToKeep == currentLineScript.gameObject)
            {
                lineHistory.Add(currentLineScript.gameObject);
                Debug.Log($"[CableManager] Added line to lineHistory: {lastNodeScript.name} → {nodeScript.name} (total: {lineHistory.Count})");
            }
            else
            {
                Debug.Log($"[CableManager] Line already exists in activeConnections: {lastNodeScript.name} → {nodeScript.name}");
            }
        }

        nodeHistory.Add(nodeScript);
        lastNodeScript = nodeScript;
        
        Debug.Log($"[CableManager] Node added to history: {nodeScript.name} (total nodes: {nodeHistory.Count})");

        CreateNewLine(nodeScript.transform);
    }

    GameObject MergeOrAddConnection(PowerNode3D_SilkyWoods fromNode, PowerNode3D_SilkyWoods toNode, GameObject newLine)
    {
        NodeConnection existingConnection = activeConnections.FirstOrDefault(
            conn => conn.IsSameConnection(fromNode, toNode)
        );

        if (existingConnection != null)
        {
            Destroy(newLine);
            return existingConnection.line;
        }
        else
        {
            NodeConnection newConnection = new NodeConnection
            {
                nodeA = fromNode,
                nodeB = toNode,
                line = newLine
            };
            activeConnections.Add(newConnection);
            return newLine;
        }
    }

    void CreateElectricLoop(PowerNode3D_SilkyWoods closingNode, List<GameObject> loopLines, List<GameObject> linesToDestroyBeforeLoop)
    {
        if (electricZonePrefab == null) return;
        if (SoundManager_SilkyWoods.Instance != null)
        {
            SoundManager_SilkyWoods.Instance.PlayVFXSound(3);
        }

        int startIndex = nodeHistory.IndexOf(closingNode);
        List<PowerNode3D_SilkyWoods> loopNodes = new List<PowerNode3D_SilkyWoods>();
        for (int i = startIndex; i < nodeHistory.Count; i++)
        {
            loopNodes.Add(nodeHistory[i]);
        }

        GameObject zoneObj = Instantiate(electricZonePrefab, Vector3.zero, Quaternion.identity);
        ElectricZone_SilkyWoods zoneScript = zoneObj.GetComponent<ElectricZone_SilkyWoods>();

        if (zoneScript != null)
        {
            zoneScript.Initialize(loopNodes, loopLines, linesToDestroyBeforeLoop, 2.0f);
        }

        activeConnections.Clear();
    }

    void ResetConnectionState()
    {
        nodeHistory.Clear();
        lineHistory.Clear();
        lastNodeScript = null;
        currentLineScript = null;
    }

    void CreateNewLine(Transform startNode)
    {
        if (electricLinePrefab == null) return;
        GameObject newLineObj = Instantiate(electricLinePrefab, Vector3.zero, Quaternion.identity);
        currentLineScript = newLineObj.GetComponent<ElectricLine3D_SilkyWoods>();

        if (currentLineScript != null)
        {
            currentLineScript.startObj = startNode;
            currentLineScript.endObj = buttPosition;
        }
    }

    void CleanupAllConnections()
    {
        if (currentLineScript != null)
        {
            Destroy(currentLineScript.gameObject);
            currentLineScript = null;
        }

        foreach (GameObject line in lineHistory)
        {
            if (line != null)
            {
                Destroy(line);
            }
        }

        foreach (var connection in activeConnections)
        {
            if (connection.line != null)
            {
                Destroy(connection.line);
            }
        }

        ResetConnectionState();
        activeConnections.Clear();
    }

    private void OnDestroy()
    {
        CleanupAllConnections();
    }
}