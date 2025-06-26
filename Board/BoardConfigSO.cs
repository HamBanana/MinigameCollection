using UnityEngine;

namespace Ham.Board{
[CreateAssetMenu(fileName = "BoardConfigSO", menuName = "Board/BoardConfig")]
public class BoardConfigSO : ScriptableObject
{
    public int[][] CellMask;
    public int XRotation;
}
}
