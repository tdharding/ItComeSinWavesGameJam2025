using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "WaveGrid/Grid Data")]
public class GridData : ScriptableObject
{
    public const int GridSize = 32;
    public int[] cells = new int[GridSize * GridSize]; 
    // 0 = empty, 1 = red/prefab1, 2 = green/prefab2, 3 = blue/prefab3
}
