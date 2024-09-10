using UnityEngine;


public struct GridObjectData
{
    public Vector2Int gridPos;
    public Vector3 worldPos;

    public TowerCore tower;

    public bool full
    {
        get
        {
            return tower != null;
        }
    }

    public int coreType;
    public int type;
}