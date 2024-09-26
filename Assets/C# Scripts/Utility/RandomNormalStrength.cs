using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RandomNormalStrength : NetworkBehaviour
{
    public float min, max;


    public float[] randomList;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int childCount = GetComponentsInChildren<MeshRenderer>().Length;

            randomList = new float[childCount];

            for (int i = 0; i < childCount; i++)
            {
                randomList[i] = Random.Range(min, max);
            }
            RandomNormalMap_ClientRPC(randomList);
        }
        else
        {
            RequestRandomList_ServerRPC();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestRandomList_ServerRPC()
    {
        RandomNormalMap_ClientRPC(randomList);
    }


    [ClientRpc(RequireOwnership = false)]
    private void RandomNormalMap_ClientRPC(float[] randomList)
    {
        Renderer[] renderers = GetComponentsInChildren<MeshRenderer>();


        for (int i = 0; i < Mathf.Min(randomList.Length); i++)
        {
            renderers[i].material.SetFloat("_Normal_Strength", randomList[i]);
        }
    }
}
