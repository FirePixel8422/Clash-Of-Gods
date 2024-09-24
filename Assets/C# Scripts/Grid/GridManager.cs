using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class GridManager : NetworkBehaviour
{
    public static GridManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public LayerMask p1;
    public LayerMask p2;
    public LayerMask neutral;
    public LayerMask obstruction;


    public bool drawMasterGizmos;
    public bool drawTileGizmos;
    public bool drawTileDataGizmos;

    public Vector3 gridPosition;
    public Vector3 gridSize;
    public float tileSize;

    public int gridSizeX, gridSizeZ;

    [SerializeField] private GridObjectData[,] grid;

    public List<GridObjectData> p1GridTiles = new List<GridObjectData>();
    public List<GridObjectData> p2GridTiles = new List<GridObjectData>();




    public override void OnNetworkSpawn()
    {
        CreateGrid();

        PlacementManager.Instance.Init(NetworkManager.LocalClientId == 0 ? p1 : p2, neutral, p1 + p2 + neutral, NetworkManager.LocalClientId);
    }

    private void CreateGrid()
    {
        gridSizeX = Mathf.RoundToInt(gridSize.x / tileSize);
        gridSizeZ = Mathf.RoundToInt(gridSize.z / tileSize);

        grid = new GridObjectData[gridSizeX, gridSizeZ];

        Vector3 worldBottomLeft = gridPosition - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.z / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 _worldPos = worldBottomLeft + Vector3.right * (x * tileSize + tileSize / 2) + Vector3.forward * (z * tileSize + tileSize / 2);


                int _type = 10;
                //neutral

                if (Physics.Raycast(_worldPos + Vector3.up, Vector3.down, 20, obstruction))
                {
                    _type = 5;
                    //obstruction
                }
                else if (Physics.Raycast(_worldPos + Vector3.up, Vector3.down, 20, p1))
                {
                    _type = 0;

                    p1GridTiles.Add(
                        new GridObjectData()
                        {
                            gridPos = new Vector2Int(x, z),
                            worldPos = _worldPos,
                            coreType = _type,
                            type = _type,
                            full = false,
                        });
                    //player 1
                }
                else if (Physics.Raycast(_worldPos + Vector3.up, Vector3.down, 20, p2))
                {
                    _type = 1;

                    p2GridTiles.Add(
                        new GridObjectData()
                        {
                            gridPos = new Vector2Int(x, z),
                            worldPos = _worldPos,
                            coreType = _type,
                            type = _type,
                            full = false,
                        });

                    //player 2
                }
                else
                {
                    _type = 100;
                }

                grid[x, z] = new GridObjectData()
                {
                    gridPos = new Vector2Int(x, z),
                    worldPos = _worldPos,
                    coreType = _type,
                    type = _type,
                    full = _type == 5 || _type == 100,
                };
            }
        }

        ObstacleGenerator.Instance.CreateObstacles();
    }


    public GridObjectData GridObjectFromWorldPoint(Vector3 worldPosition)
    {
        worldPosition -= gridPosition;
        float percentX = Mathf.Clamp01((worldPosition.x + gridSize.x / 2) / gridSize.x);
        float percentZ = Mathf.Clamp01((worldPosition.z + gridSize.z / 2) / gridSize.z);

        int x = Mathf.FloorToInt(percentX * gridSizeX);
        int z = Mathf.FloorToInt(percentZ * gridSizeZ);

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        z = Mathf.Clamp(z, 0, gridSizeZ - 1);

        return grid[x, z];
    }


    public GridObjectData GetGridData(Vector2Int gridPos)
    {
        gridPos.Clamp(new Vector2Int(0, 0), new Vector2Int(gridSizeX - 1, gridSizeZ - 1));
        return grid[gridPos.x, gridPos.y];
    }

    public void UpdateGridDataFullState(Vector2Int gridPos, bool newState)
    {
        grid[gridPos.x, gridPos.y].full = newState;
    }
    public void UpdateGridDataOnFireState(Vector2Int gridPos, bool newState)
    {
        grid[gridPos.x, gridPos.y].onFire += newState ? 1 : -1;
    }
    public void UpdateGridDataType(Vector2Int gridPos, int type)
    {
        grid[gridPos.x, gridPos.y].type = type;
    }
    public void ResetGridDataFieldType(Vector2Int gridPos)
    {
        grid[gridPos.x, gridPos.y].type = grid[gridPos.x, gridPos.y].coreType;
        grid[gridPos.x, gridPos.y].tower = null;
    }
    public void UpdateTowerData(Vector2Int gridPos, TowerCore tower)
    {
        grid[gridPos.x, gridPos.y].tower = tower;
        
        grid[gridPos.x, gridPos.y].full = tower != null;
    }

    public bool IsInGrid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridSizeX && gridPos.y >= 0 && gridPos.y < gridSizeZ;
    }


    public void OnDrawGizmos()
    {
        if (drawMasterGizmos == false)
        {
            return;
        }

        Gizmos.DrawWireCube(gridPosition, new Vector3(gridSize.x, gridSize.y, gridSize.z));

        Vector3 worldBottomLeft = gridPosition - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.z / 2;
        if (drawTileGizmos)
        {
            gridSizeX = Mathf.RoundToInt(gridSize.x / tileSize);
            gridSizeZ = Mathf.RoundToInt(gridSize.z / tileSize);
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(worldBottomLeft + Vector3.right * (x * tileSize + tileSize / 2) + Vector3.forward * (z * tileSize + tileSize / 2), new Vector3(tileSize, gridSize.y, tileSize));
                }
            }
        }
        if (drawTileDataGizmos && Application.isPlaying)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    if (grid[x, z].full)
                    {
                        Gizmos.color = Color.gray;
                    }
                    else if (grid[x, z].type == 10)
                    {
                        Gizmos.color = Color.black;
                    }
                    else if (grid[x, z].type == 0)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if (grid[x, z].type == 1)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else if (grid[x, z].type == 100)
                    {
                        Gizmos.color = Color.white;
                    }
                    
                    Gizmos.DrawCube(worldBottomLeft + Vector3.right * (x * tileSize + tileSize / 2) + Vector3.forward * (z * tileSize + tileSize / 2), new Vector3(tileSize / 2, tileSize / 2, tileSize / 2));
                }
            }
        }
    }
}