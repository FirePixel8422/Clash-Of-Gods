using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TroopInfoManager : MonoBehaviour
{
    public static TroopInfoManager Instance;
    private void Awake()
    {
        Instance = this;
    }


    public GameObject troopInfoUI;

    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;



    public void SelectTower(string name, string health, string damage)
    {
        troopInfoUI.SetActive(true);

        text1.text = "Tower Name:" + name;
        text2.text = "Damage: " + damage;
        text3.text = "Health:" + health;
    }
    public void DeselectTower()
    {
        troopInfoUI.SetActive(false);
    }
}
