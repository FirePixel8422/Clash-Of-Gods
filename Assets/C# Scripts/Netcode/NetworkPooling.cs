using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class NetworkPooling : NetworkBehaviour
{
    public static NetworkPooling Instance;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }


    public VisualEffectPool[] pooledPrefabs;
    public bool dynamicRefillOnEmpty;

    public List<List<List<VisualEffect>>> pooledList = new List<List<List<VisualEffect>>>();


    public override void OnNetworkSpawn()
    {
        pooledList = new List<List<List<VisualEffect>>>();

        if (IsServer)
        {
            for (int i = 0; i < pooledPrefabs.Length; i++)
            {
                pooledList.Add(new List<List<VisualEffect>>());
                AddVisualEffectToPool(i, pooledPrefabs[i].amount);
            }
        }
    }

    private void AddVisualEffectToPool(int index, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            List<VisualEffect> altPrefabsList = new List<VisualEffect>();

            for (int i2 = 0; i2 < pooledPrefabs[index].visualEffectPrefabs.Length; i2++)
            {
                GameObject obj = Instantiate(pooledPrefabs[index].visualEffectPrefabs[i2], Vector3.zero, Quaternion.identity, transform);

                altPrefabsList.Add(obj.GetComponent<VisualEffect>());
            }

            pooledList[index].Add(altPrefabsList);
        }
    }

    public GameObject GetPulledObj(int index, Vector3 pos, Quaternion rot)
    {
        int r = Random.Range(0, pooledPrefabs[index].visualEffectPrefabs.Length);

        foreach (List<VisualEffect> obj in pooledList[index])
        {
            if (obj[r].HasAnySystemAwake() == false)
            {
                obj[r].Play();
                obj[r].transform.SetPositionAndRotation(pos, rot);
                return obj[r].gameObject;
            }
        }
        if (dynamicRefillOnEmpty == false)
        {
            return null;
        }
        GameObject spawnedObj = Instantiate(pooledPrefabs[index].visualEffectPrefabs[Random.Range(0, pooledPrefabs[index].visualEffectPrefabs.Length)], pos, rot, transform);
        VisualEffect visualEffect = spawnedObj.GetComponent<VisualEffect>();


        List<VisualEffect> altPrefabsList = new List<VisualEffect>();

        for (int i2 = 0; i2 < pooledPrefabs[index].visualEffectPrefabs.Length; i2++)
        {
            GameObject obj = Instantiate(pooledPrefabs[index].visualEffectPrefabs[i2], Vector3.zero, Quaternion.identity, transform);

            altPrefabsList.Add(obj.GetComponent<VisualEffect>());
        }

        pooledList[index].Add(altPrefabsList);
        pooledPrefabs[index].amount += 1;

        visualEffect.Play();
        
        return spawnedObj;
    }

    //really complicated -__- not worth it


    [ServerRpc(RequireOwnership = false)]
    private void SetPositionAndRotation_ServerRPC(ulong networkObjectId, Vector3 pos, Quaternion rot)
    {
        SetPositionAndRotation_ClientRPC(networkObjectId, pos, rot);
    }
    [ClientRpc(RequireOwnership = false)]
    private void SetPositionAndRotation_ClientRPC(ulong networkObjectId, Vector3 pos, Quaternion rot)
    {
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
        networkObject.transform.SetPositionAndRotation(pos, rot);
    }


    [System.Serializable]
    public class VisualEffectPool
    {
        public GameObject[] visualEffectPrefabs;
        public int amount;
    }
}