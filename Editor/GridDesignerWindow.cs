using UnityEngine;
using UnityEditor;

public class GridDesignerWindow : EditorWindow
{
    const int GridSize = 32;
    const int CellCount = GridSize * GridSize;
    const int CellSize = 18;
    const int GridPixelSize = GridSize * CellSize; // 576px

    int[] grid = new int[CellCount];

    GameObject prefab1;
    GameObject prefab2;
    GameObject prefab3;

    int activeDraw = 0; // 0 = erase, 1 = red, 2 = green, 3 = blue

    Color c1 = Color.red;
    Color c2 = Color.green;
    Color c3 = Color.blue;

    GridData loadedData;

    // ============================
    // Circle texture generator
    // ============================
    Texture2D circleTex;

    Texture2D GetCircleTexture(int size)
    {
        if (circleTex != null && circleTex.width == size)
            return circleTex;

        circleTex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        circleTex.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0, 0, 0, 0);
        Color white = new Color(1, 1, 1, 0.25f);

        int r = size / 2;
        int r2 = r * r;

        int thickness = 3;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = x - r;
                int dy = y - r;
                int d2 = dx * dx + dy * dy;

                if (Mathf.Abs(d2 - r2) <= thickness * r)
                    circleTex.SetPixel(x, y, white);
                else
                    circleTex.SetPixel(x, y, clear);
            }
        }

        circleTex.Apply();
        return circleTex;
    }

    // ============================

    [MenuItem("Tools/Waves/Grid Designer")]
    public static void Open()
    {
        GetWindow<GridDesignerWindow>("Grid Designer");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // ======== PREFAB INPUTS ========
        EditorGUILayout.LabelField("Prefab Inputs", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        prefab1 = (GameObject)EditorGUILayout.ObjectField(prefab1, typeof(GameObject), false);
        DrawColorBox(c1);
        if (GUILayout.Button("Draw", GUILayout.Width(60))) activeDraw = 1;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        prefab2 = (GameObject)EditorGUILayout.ObjectField(prefab2, typeof(GameObject), false);
        DrawColorBox(c2);
        if (GUILayout.Button("Draw", GUILayout.Width(60))) activeDraw = 2;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        prefab3 = (GameObject)EditorGUILayout.ObjectField(prefab3, typeof(GameObject), false);
        DrawColorBox(c3);
        if (GUILayout.Button("Draw", GUILayout.Width(60))) activeDraw = 3;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // ======== ERASER ========
        if (GUILayout.Button("Eraser", GUILayout.Height(25)))
            activeDraw = 0;

        EditorGUILayout.Space();

        // ======== DETERMINE GRID RECT ========
        float windowWidth = position.width;
        float gridX = (windowWidth - GridPixelSize) / 2f;
        float gridY = 180f;

        Rect gridRect = new Rect(gridX, gridY, GridPixelSize, GridPixelSize);

        if (position.width < GridPixelSize + 40)
        {
            EditorGUILayout.HelpBox("Expand the window to view the full grid.", MessageType.Info);
            return;
        }

        // Draw the grid + circle
        DrawGrid(gridRect);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // ======== BUTTON BAR ========
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("CLEAR ALL", GUILayout.Height(30)))
        {
            for (int i = 0; i < CellCount; i++) grid[i] = 0;
        }

        if (GUILayout.Button("SAVE AS...", GUILayout.Height(30)))
            SaveGrid();

        if (GUILayout.Button("LOAD...", GUILayout.Height(30)))
            LoadGrid();

        EditorGUILayout.EndHorizontal();
    }

    // ============================

    void DrawColorBox(Color c)
    {
        Rect r = GUILayoutUtility.GetRect(20, 20);
        EditorGUI.DrawRect(r, c);
    }

    // ============================
    // Draw the grid + circle
    // ============================
  void DrawGrid(Rect rect)
{
    // Draw the circle behind the grid
    DrawCircleGuide(rect);

    Event e = Event.current;

    for (int y = 0; y < GridSize; y++)
    {
        for (int x = 0; x < GridSize; x++)
        {
            int index = y * GridSize + x;

            Rect cell = new Rect(
                rect.x + x * CellSize,
                rect.y + y * CellSize,
                CellSize,
                CellSize
            );

            // -------- FULL TILE FILL (drawn tiles only) --------
            if (grid[index] != 0)
            {
                Color col = grid[index] switch
                {
                    1 => c1,
                    2 => c2,
                    3 => c3,
                    _ => new Color(0, 0, 0, 0)
                };

                EditorGUI.DrawRect(cell, col);
            }
            // Empty tiles stay transparent (so the circle shows through)

            // -------- CLICK TO DRAW --------
            if (e.type == EventType.MouseDown && cell.Contains(e.mousePosition))
            {
                grid[index] = activeDraw;
                Repaint();
            }

            // -------- TILE BORDER (always visible) --------
            Color border = new Color(0f, 0f, 0f, 0.55f);
            EditorGUI.DrawRect(new Rect(cell.x, cell.y, CellSize, 1), border);      // top
            EditorGUI.DrawRect(new Rect(cell.x, cell.y, 1, CellSize), border);      // left
        }
    }
}


    // ============================
    // Circle (texture-based)
    // ============================
    void DrawCircleGuide(Rect rect)
    {
        Texture2D tex = GetCircleTexture((int)rect.width);

        GUI.color = new Color(1, 1, 1, 0.25f);
        GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, true);
        GUI.color = Color.white;
    }

    // ============================
    // SAVE / LOAD
    // ============================
    void SaveGrid()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Grid", "GridData", "asset", "Save GridData");
        if (path == "") return;

        GridData data = ScriptableObject.CreateInstance<GridData>();
        data.cells = (int[])grid.Clone();

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void LoadGrid()
    {
        string path = EditorUtility.OpenFilePanel("Load Grid", "Assets", "asset");
        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);
        loadedData = AssetDatabase.LoadAssetAtPath<GridData>(path);

        if (!loadedData) return;

        grid = (int[])loadedData.cells.Clone();
    }
}
