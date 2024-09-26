using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ZeusFlyingTiles : NetworkBehaviour
{
    public FlyingTilesStats flyingTilesStats;


    public override void OnNetworkSpawn()
    {
        flyingTilesStats.Start();
    }
}
