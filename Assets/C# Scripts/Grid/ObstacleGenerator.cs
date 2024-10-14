using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class ObstacleGenerator : NetworkBehaviour
{
    public static ObstacleGenerator Instance;
    private void Awake()
    {
        Instance = this;
    }


    public GameObject[] obstacles;

    public int obstacleAmount;


    public FlyingTilesStats flyingTilesStats;

    private bool done;




    public void CreateObstacles()
    {
        if (IsServer)
        {
            List<GridObjectData> gridTiles = new List<GridObjectData>(GridManager.Instance.p1GridTiles.Count);

            for (int i = 0; i < GridManager.Instance.p1GridTiles.Count; i++)
            {
                gridTiles.Add(GridManager.Instance.p1GridTiles[i]);
            }


            Vector3[] positions = new Vector3[obstacleAmount * 2];
            Vector2Int[] gridPositions = new Vector2Int[obstacleAmount * 2];

            for (int player = 0; player < 2; player++)
            {

                for (int i = 0; i < obstacleAmount; i++)
                {
                    int r = Random.Range(0, gridTiles.Count);

                    positions[player * obstacleAmount + i] = gridTiles[r].worldPos;
                    gridPositions[player * obstacleAmount + i] = gridTiles[r].gridPos;

                    gridTiles.RemoveAt(r);
                }


                gridTiles = new List<GridObjectData>(GridManager.Instance.p2GridTiles.Count);

                for (int i = 0; i < GridManager.Instance.p2GridTiles.Count; i++)
                {
                    gridTiles.Add(GridManager.Instance.p2GridTiles[i]);
                }
            }

            SpawnObstacles_ServerRPC(positions, gridPositions);
        }
        else
        {
            SyncGridState_ServerRPC();
        }
    }




    private ulong[] networkObjectIds;
    private Vector2Int[] gridPositions;


    [ServerRpc(RequireOwnership = false)]
    private void SpawnObstacles_ServerRPC(Vector3[] positions, Vector2Int[] _gridPositions)
    {
        networkObjectIds = new ulong[positions.Length];
        gridPositions = _gridPositions;

        for (int i = 0; i < positions.Length; i++)
        {
            int r = Random.Range(0, obstacles.Length);

            GameObject obj = Instantiate(obstacles[r], positions[i] + obstacles[r].transform.position, Quaternion.Euler(0, Random.Range(1, 5) * 90, 0));

            NetworkObject networkObject = obj.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership((ulong)(i < obstacleAmount ? 10 : 20), true);

            networkObjectIds[i] = networkObject.NetworkObjectId;
        }

        SyncGridState_ClientRPC(networkObjectIds, gridPositions);
    }




    [ServerRpc(RequireOwnership = false)]
    private void SyncGridState_ServerRPC()
    {
        SyncGridState_ClientRPC(networkObjectIds, gridPositions);
    }


    [ClientRpc(RequireOwnership = false)]
    private void SyncGridState_ClientRPC(ulong[] networkObjectIds, Vector2Int[] gridPositions)
    {
        for (int i = 0; i < gridPositions.Length; i++)
        {
            Obstacle obstacle = NetworkManager.SpawnManager.SpawnedObjects[networkObjectIds[i]].GetComponent<Obstacle>();

            flyingTilesStats.tileTransformList.Add(obstacle.transform);

            MeshRenderer renderer = obstacle.underAttackArrowAnim.GetComponentInChildren<MeshRenderer>();

            renderer.material.SetColor(Shader.PropertyToID("_Base_Color"), PlacementManager.Instance.playerColors[i < obstacleAmount ? 0 : 1]);


            GridManager.Instance.UpdateTowerData(gridPositions[i], obstacle);
        }

        if (done == false)
        {
            flyingTilesStats.Start(this);
            done = true;
        }
    }
}
