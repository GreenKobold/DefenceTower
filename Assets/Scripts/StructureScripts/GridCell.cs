using UnityEngine;

[CreateAssetMenu(fileName = "GridCell", menuName = "Scriptable Objects/GridCell")]
public class GridCell : ScriptableObject
{
        public int x, y;
        public bool hasTower;
        public GridCell(int x, int y)
        {
            this.x = x; this.y = y;
            hasTower = false;
        }
}
