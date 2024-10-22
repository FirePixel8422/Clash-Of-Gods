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
    public TextMeshProUGUI text4;


    private bool towerSelected;
    private string savedName;
    private string savedHealth;
    private string savedDamage;

    public void SelectTower(string name, string health, string damage)
    {
        troopInfoUI.SetActive(true);
        towerSelected = true;

        savedName = name;
        savedHealth = health;
        savedDamage = damage;

        ReselectTower();
    }

    public void ReselectTower()
    {
        text4.text = "";

        text1.text = "Tower Name:" + savedName;
        text2.text = "Damage: " + savedHealth;
        text3.text = "Health:" + savedDamage;

        if (towerSelected == false)
        {
            troopInfoUI.SetActive(false);
        }
    }

    public void ShowAbility(string text)
    {
        troopInfoUI.SetActive(true);

        text1.text = "";
        text2.text = "";
        text3.text = "";

        text4.text = text;
    }
    public void DeselectTower()
    {
        troopInfoUI.SetActive(false);
        towerSelected = false;
    }
}