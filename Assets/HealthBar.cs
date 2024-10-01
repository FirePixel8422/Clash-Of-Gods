using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform healthBar;


    private float maxHealth;
    private float health;


    private void Start()
    {
        maxHealth = GetComponent<TowerCore>().health;
        health = maxHealth;
    }

    public void UpdateHealthBar(float healthLeft)
    {

    }
}
