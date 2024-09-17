using UnityEngine;


[System.Serializable]
public struct GridObjectData
{
    public Vector2Int gridPos;
    public Vector3 worldPos;

    public TowerCore tower;

    public bool full;

    public int onFire;

    public int coreType;
    public int type;
}