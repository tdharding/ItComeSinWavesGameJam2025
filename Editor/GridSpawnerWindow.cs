using UnityEngine;
using UnityEditor;

public class GridSpawnerWindow : EditorWindow
{
    GridData grid;
    GameObject prefab1;
    GameObject prefab2;
    GameObject prefab3;
    Transform plane;

    bool applyRotationOffset = false;
    bool createParent = true;
    string parentName = "SpawnedGrid";

    [MenuItem("Tools/Waves/Grid Spawner")]
    public static void Open()
    {
        GetWindow<GridSpawnerWindow>("Grid Spawner");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        grid = (GridData)EditorGUILayout.ObjectField("Grid Data", grid, typeof(GridData), false);
        plane = (Transform)EditorGUILayout.ObjectField("Plane Transform", plane, typeof(Transform), true);

        prefab1 = (GameObject)EditorGUILayout.ObjectField("Prefab 1 (Red)", prefab1, typeof(GameObject), false);
        prefab2 = (GameObject)EditorGUILayout.ObjectField("Prefab 2 (Green)", prefab2, typeof(GameObject), false);
        prefab3 = (GameObject)EditorGUILayout.ObjectField("Prefab 3 (Blue)", prefab3, typeof(GameObject), false);

        EditorGUILayout.Space();

        applyRotationOffset = EditorGUILayout.Toggle("Apply -90° X Rotation", applyRotationOffset);

        EditorGUILayout.Space();

        createParent = EditorGUILayout.Toggle("Create Parent Object", createParent);
        if (createParent)
            parentName = EditorGUILayout.TextField("Parent Name", parentName);

        EditorGUILayout.Space();

        if (GUILayout.Button("SPAWN", GUILayout.Height(40)))
        {
            if (grid && plane)
                SpawnGrid();
        }
    }

    void SpawnGrid()
    {
        Renderer r = plane.GetComponent<Renderer>();
        if (!r)
        {
            Debug.LogError("Plane must have a Renderer with bounds.");
            return;
        }

        Bounds b = r.bounds;

        float width = b.size.x;
        float height = b.size.z;

        float tileX = width / GridData.GridSize;
        float tileZ = height / GridData.GridSize;

        Vector3 origin = b.min;

        // -------------------------------------
        // PARENT OBJECT (FIX #1)
        // -------------------------------------
        Transform parentTransform = null;

        if (createParent)
        {
            GameObject root = new GameObject(parentName);
            Undo.RegisterCreatedObjectUndo(root, "Create Spawn Parent");

            // Align the parent exactly to the plane
            root.transform.position = plane.position;
            root.transform.rotation = plane.rotation;

            parentTransform = root.transform;
        }

        // -------------------------------------
        // SPAWN LOOP
        // -------------------------------------
        for (int y = 0; y < GridData.GridSize; y++)
        {
            // FIXED: Flip Y so the spawned grid matches the editor UI
            int flippedY = GridData.GridSize - 1 - y;

            for (int x = 0; x < GridData.GridSize; x++)
            {
                int index = flippedY * GridData.GridSize + x;
                int cell = grid.cells[index];

                if (cell == 0) continue;

                GameObject prefab = cell switch
                {
                    1 => prefab1,
                    2 => prefab2,
                    3 => prefab3,
                    _ => null
                };

                if (!prefab) continue;

                Vector3 pos = new Vector3(
                    origin.x + x * tileX + tileX * 0.5f,
                    plane.position.y,
                    origin.z + y * tileZ + tileZ * 0.5f
                );

                Quaternion rot = plane.rotation;

                if (applyRotationOffset)
                    rot *= Quaternion.Euler(-90, 0, 0);

                // Undo support for spawned objects
                GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                Undo.RegisterCreatedObjectUndo(obj, "Spawn Grid Object");

                obj.transform.position = pos;
                obj.transform.rotation = rot;

                if (parentTransform != null)
                    obj.transform.SetParent(parentTransform);
            }
        }

        Debug.Log("Spawn Complete.");
    }
}
