using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public bool isMyTurn;
    public ulong localClientId;
    public ulong clientOnTurnId;



    public override void OnNetworkSpawn()
    {
        localClientId = NetworkManager.LocalClientId;
        if (IsServer)
        {
            clientOnTurnId = (ulong)Random.Range(0, 2);

            if (clientOnTurnId == localClientId)
            {
                isMyTurn = true;
            }
        }
        else
        {
            RequestClientOnTurnId_ServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestClientOnTurnId_ServerRPC()
    {
        RequestClientOnTurnId_ClientRPC(clientOnTurnId);
    }
    [ClientRpc(RequireOwnership = false)]
    private void RequestClientOnTurnId_ClientRPC(ulong _clientOnTurnId)
    {
        clientOnTurnId = _clientOnTurnId;

        if (clientOnTurnId == localClientId)
        {
            isMyTurn = true;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void NextTurn_ServerRPC()
    {
        ulong nextClientOnTurnId = clientOnTurnId + 1;

        if((int)nextClientOnTurnId == NetworkManager.ConnectedClientsIds.Count)
        {
            nextClientOnTurnId = 0;
        }

        NextTurn_ClientRPC(nextClientOnTurnId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void NextTurn_ClientRPC(ulong nextClientOnTurnId)
    {
        clientOnTurnId = nextClientOnTurnId;

        if (nextClientOnTurnId == localClientId)
        {
            isMyTurn = true;
        }
    }
}
