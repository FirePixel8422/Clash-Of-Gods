using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GodCore : NetworkBehaviour
{
    public static GodCore Instance;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == "MainGame")
        {
            if(chooseGodMenu == null)
            {
                print("null");
            }
            chooseGodMenu.SetActive(true);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (SceneManager.GetActiveScene().name == "MainGame")
        {
            chooseGodMenu.SetActive(true);
        }

        if (IsServer)
        {
            useObstacleMapButtonObj.SetActive(true);
        }
    }




    #region God Base Data

    public enum God
    {
        Athena,
        Hades,
        Zeus
    };


    public float damageMultiplier;
    public float healthMultiplier;
    public int addedMoves;
    public bool IsAthena
    {
        get
        {
            return god == God.Athena;
        }
    }

    public float zeusTroopStunChance;
    public int fails;
    public bool RandomStunChance()
    {
        if (god == God.Zeus && (Random.Range(0, 100f) < zeusTroopStunChance * (1 + fails)))
        {
            fails -= 1;
            return true;
        }
        else
        {
            fails += 1;
        }
        return false;
    }

    public int fireDamage;
    public bool IsHades
    {
        get
        {
            return god == God.Hades;
        }
    }
    public bool IsZeus
    {
        get
        {
            return god == God.Zeus;
        }
    }



    public God god;

    #endregion


    public GameObject chooseGodMenu;
    public GameObject confirmGodButton;

    public GameObject useObstacleMapButtonObj;

    public bool[] confirmed;
    public Color imageReadyColor;
    public Color dontUseObstaclesColor;

    public int[] chosenGods;

    public TextMeshProUGUI[] zeusNames;
    public TextMeshProUGUI[] hadesNames;
    public TextMeshProUGUI[] athenaNames;

    public GameObject[] godMaps;


    #region Choose God

    public void ChooseGod(int _god)
    {
        if (confirmed[NetworkManager.LocalClientId] || NetworkManager.ConnectedClientsIds.Count == 1)
        {
            return;
        }

        SyncChosenGod_ServerRPC(_god);
    }


    [ServerRpc(RequireOwnership = false)]
    private void SyncChosenGod_ServerRPC(int chosenGod, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        SyncChosenGod_ClientRPC(senderClientId, chosenGod);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncChosenGod_ClientRPC(ulong cliendId, int chosenGod)
    {
        if (chosenGods[cliendId == 0 ? 1 : 0] != chosenGod)
        {
            chosenGods[cliendId] = chosenGod;


            if (NetworkManager.LocalClientId == cliendId)
            {
                god = (God)chosenGod;

                confirmGodButton.SetActive(true);
            }            

            switch (chosenGod)
            {
                case 0:
                    {
                        athenaNames[cliendId].text = "P" + (cliendId + 1).ToString();
                        hadesNames[cliendId].text = "";
                        zeusNames[cliendId].text = "";
                        break;
                    }
                case 1:
                    {
                        hadesNames[cliendId].text = "P" + (cliendId + 1).ToString();
                        athenaNames[cliendId].text = "";
                        zeusNames[cliendId].text = "";
                        break;
                    }
                case 2:
                    {
                        zeusNames[cliendId].text = "P" + (cliendId + 1).ToString();
                        hadesNames[cliendId].text = "";
                        athenaNames[cliendId].text = "";
                        break;
                    }
            }
        }
    }

    #endregion


    #region Confirm God/Ready

    public void ConfirmGod(Image image)
    {
        SyncReadyState_ServerRPC(true);

        image.color = imageReadyColor;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncReadyState_ServerRPC(bool state, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        SyncReadyState_ClientRPC(senderClientId, state);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncReadyState_ClientRPC(ulong cliendId, bool state)
    {
        confirmed[cliendId] = state;

        if (confirmed[0] == true && confirmed[1] == true)
        {
            if (IsServer)
            {
                StartGame();
            }

            chooseGodMenu.SetActive(false);

            CameraController.Instance.control = true;

            Athena.Instance.Init();
            Hades.Instance.Init();
            Zeus.Instance.Init();

            GetComponentInChildren<Canvas>().sortingOrder = -1;
        }
    }



    public void ToggleUseObstaclesState(Image image)
    {
        GridManager.Instance.useObstacles = !GridManager.Instance.useObstacles;

        image.color = GridManager.Instance.useObstacles ? imageReadyColor : dontUseObstaclesColor;
    }

    #endregion


#if UNITY_EDITOR

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (IsServer)
            {
                StartGame();
            }

            chooseGodMenu.SetActive(false);

            CameraController.Instance.control = true;

            Athena.Instance.Init();
            Hades.Instance.Init();
            Zeus.Instance.Init();

            GetComponentInChildren<Canvas>().sortingOrder = -1;
        }
    }
#endif


    private void StartGame()
    {
        GameObject map0 = Instantiate(godMaps[chosenGods[0]], Vector3.zero, Quaternion.Euler(0, 180, 0));
        NetworkObject map0Network = map0.GetComponent<NetworkObject>();
        map0Network.SpawnWithOwnership(0, true);



        GameObject map1 = Instantiate(godMaps[chosenGods[1]], Vector3.zero, Quaternion.identity);
        NetworkObject map1Network = map1.GetComponent<NetworkObject>();

        map1Network.SpawnWithOwnership(1, true);

        SetupGrid_ClientRPC(map0Network.NetworkObjectId, map1Network.NetworkObjectId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SetupGrid_ClientRPC(ulong map0Id, ulong map1Id)
    {
        PlayerBase map0 = NetworkManager.SpawnManager.SpawnedObjects[map0Id].GetComponent<PlayerBase>();
        PlayerBase map1 = NetworkManager.SpawnManager.SpawnedObjects[map1Id].GetComponent<PlayerBase>();


        GridManager.Instance.Init(new PlayerBase[] { map0, map1 });
    }
}
