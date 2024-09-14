using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

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

    public UnityEvent OnTurnChangedEvent;
    public UnityEvent OnMyTurnStartedEvent;



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
            StartGame_ServerRPC();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            NextTurn_ServerRPC();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            clientOnTurnId = 0;
            isMyTurn = true;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void StartGame_ServerRPC()
    {
        StartGame_ClientRPC(clientOnTurnId);
    }
    [ClientRpc(RequireOwnership = false)]
    private void StartGame_ClientRPC(ulong _clientOnTurnId)
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
        OnTurnChangedEvent.Invoke();

        clientOnTurnId = nextClientOnTurnId;

        if (nextClientOnTurnId == localClientId)
        {
            isMyTurn = true;
            OnMyTurnStartedEvent.Invoke();
        }
    }
}
