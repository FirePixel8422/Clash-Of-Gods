using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealthBar : NetworkBehaviour
{
    public Transform healthBar;
    public Transform healthBarDamage;

    private float fullHealthScaleX;
    public float noHealthPosX;

    public float damageAnimationScaleSpeed;
    public float damageAnimationPosSpeed;


    private float maxHealth;
    private float health;


    private void Start()
    {
        fullHealthScaleX = healthBar.localScale.x;

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

        Vector3 localScale = healthBar.localScale;
        Vector3 localPos = healthBar.localPosition;

        healthBar.localScale = new Vector3(fullHealthScaleX * (health / maxHealth), localScale.y, localScale.z);
        healthBar.localPosition = new Vector3(noHealthPosX * (1 - health / maxHealth), localPos.y, localPos.z);

        while (Vector3.Distance(healthBarDamage.localScale, healthBar.localScale) > 0.0001f || Vector3.Distance(healthBarDamage.localPosition, healthBar.localPosition) > 0.0001f)
        {
            yield return null;
            healthBarDamage.localScale = VectorLogic.InstantMoveTowards(healthBarDamage.localScale, healthBar.localScale, damageAnimationScaleSpeed * Time.deltaTime);
            healthBarDamage.localPosition = VectorLogic.InstantMoveTowards(healthBarDamage.localPosition, healthBar.localPosition, damageAnimationPosSpeed * Time.deltaTime);
        }
    }
}
