using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealthBar : NetworkBehaviour
{
    public Transform healthBar;
    public Transform healthBarDamage;

    private Vector3 initialScale;
    private Vector3 initialPosition;

    public float damageAnimationScaleSpeed;
    public float damageAnimationPosSpeed;

    private float maxHealth;
    private float health;


    private void Start()
    {
        initialScale = healthBar.localScale;
        initialPosition = healthBar.position;

        maxHealth = GetComponentInParent<TowerCore>().health;
        health = maxHealth;
    }



    public void UpdateHealthBar(float healthLeft)
    {
        UpdateHealthBar_ServerRPC(healthLeft);

        StartCoroutine(UpdateHealthBarAnimation(healthLeft));        
    }


    [ServerRpc(RequireOwnership = false)]
    private void UpdateHealthBar_ServerRPC(float healthLeft, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        UpdateHealthBar_ClientRPC(healthLeft, senderClientId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void UpdateHealthBar_ClientRPC(float healthLeft, ulong clientId)
    {
        if (clientId == NetworkManager.LocalClientId)
        {
            return;
        }

        StartCoroutine(UpdateHealthBarAnimation(healthLeft));
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(UpdateHealthBarAnimation(health - 1));
        }
    }

    private IEnumerator UpdateHealthBarAnimation(float healthLeft)
    {
        health = healthLeft;

        float healthPercentage = health / maxHealth;

        Vector3 newScale = initialScale;
        newScale.x = initialScale.x * healthPercentage;

        healthBar.localScale = newScale;


        float offsetX = (initialScale.x - newScale.x) / 2.0f;
        healthBar.position = new Vector3(initialPosition.x - offsetX, initialPosition.y, initialPosition.z);
    

        while (Vector3.Distance(healthBarDamage.localScale, healthBar.localScale) > 0.0001f || Vector3.Distance(healthBarDamage.localPosition, healthBar.localPosition) > 0.0001f)
        {
            yield return null;
            healthBarDamage.localScale = VectorLogic.InstantMoveTowards(healthBarDamage.localScale, healthBar.localScale, damageAnimationScaleSpeed * Time.deltaTime);
            healthBarDamage.localPosition = VectorLogic.InstantMoveTowards(healthBarDamage.localPosition, healthBar.localPosition, damageAnimationPosSpeed * Time.deltaTime);
        }
    }
}
