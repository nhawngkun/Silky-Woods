using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AssetRenamerWindow : EditorWindow
{
    private string projectName = "OrbHop";
    private int renamedCount = 0;

    [MenuItem("Window/Asset Renamer")]
    public static void ShowWindow()
    {
        GetWindow<AssetRenamerWindow>("Asset Renamer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Asset Renamer Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Nhập tên Project:");
        projectName = EditorGUILayout.TextField("Project Name", projectName);

        GUILayout.Space(20);

        if (GUILayout.Button("Apply", GUILayout.Height(40)))
        {
            RenameAssets();
        }

        GUILayout.Space(10);
        if (renamedCount > 0)
        {
            GUILayout.Label($"✓ Đã rename {renamedCount} file thành công!", EditorStyles.helpBox);
        }
    }

    private void RenameAssets()
    {
        if (string.IsNullOrEmpty(projectName))
        {
            EditorUtility.DisplayDialog("Lỗi", "Vui lòng nhập tên project!", "OK");
            return;
        }

        string assetsPath = "Assets";
        renamedCount = 0;

        // Danh sách extension cần rename
        string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".tga", ".psd" };
        string[] modelExtensions = { ".fbx", ".obj", ".blend", ".dae", ".gltf", ".glb" };

        List<string> allExtensions = new List<string>();
        allExtensions.AddRange(imageExtensions);
        allExtensions.AddRange(modelExtensions);

        // Tìm tất cả file ảnh và model
        string[] allFiles = Directory.GetFiles(assetsPath, "*.*", SearchOption.AllDirectories);

        foreach (string filePath in allFiles)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            if (allExtensions.Contains(extension))
            {
                string directory = Path.GetDirectoryName(filePath);
                string oldFileName = Path.GetFileNameWithoutExtension(filePath);
                string metaFilePath = filePath + ".meta";

                // Tạo tên mới: OrbHop_tên cũ
                string newFileName = projectName + "_" + oldFileName + extension;
                string newFilePath = Path.Combine(directory, newFileName);

                // Rename file
                if (File.Exists(filePath) && !File.Exists(newFilePath))
                {
                    File.Move(filePath, newFilePath);

                    // Rename file .meta nếu tồn tại
                    if (File.Exists(metaFilePath))
                    {
                        string newMetaFilePath = newFilePath + ".meta";
                        if (File.Exists(newMetaFilePath))
                            File.Delete(newMetaFilePath);
                        File.Move(metaFilePath, newMetaFilePath);
                    }

                    renamedCount++;
                    Debug.Log($"Renamed: {oldFileName}{extension} → {newFileName}");
                }
            }
        }

        // Refresh Asset Database
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Hoàn thành", $"Đã rename thành công {renamedCount} file!", "OK");
    }
}