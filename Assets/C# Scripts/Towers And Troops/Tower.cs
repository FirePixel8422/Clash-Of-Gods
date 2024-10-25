using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Tower : TowerCore
{
    public Animator attackAnimator;

    public Transform rotPoint;
    public bool yRotOnly;

    private Transform lookAtTransform;
    public Transform shootPoint;

    [HideInInspector]
    public SpriteRenderer towerPreviewRenderer;

    public float rotSpeed;

    public float animShootTime;

    public GameObject projectilePrefab;



    protected override void OnSetupTower()
    {   
        towerPreviewRenderer = GetComponentInChildren<SpriteRenderer>();

        attackAnimator.transform.rotation = Quaternion.identity;

        GetComponentInChildren<DirectionArrowValidator>().Init();

        if (rotPoint != null)
        {
            StartCoroutine(LookAtTarget_UpdateLoop());
        }
    }


    #region Tower Select/Deselect

    protected override void OnSelectTower()
    {
        attackAnimator.SetBool("Enabled", true);
    }

    protected override void OnDeSelectTower()
    {
        attackAnimator.SetBool("Enabled", false);
    }
    #endregion


    protected override IEnumerator AttackTargetAnimation(Vector3 targetPos, float combinedSize, TowerCore target = null)
    {
        if (target != null)
        {
            lookAtTransform = target.centerPoint;
        }
        yield return new WaitUntil(() => lookingAtTarget == true || rotPoint == null);

        lookingAtTarget = false;

        anim.SetTrigger("Attack");
        StartCoroutine(SoundDelay(soundDelay));

        yield return new WaitForSeconds(animShootTime);

        if (target != null)
        {
            ulong targetId = target.NetworkObjectId;
            SpawnProjectile_ServerRPC(targetId, dmg);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void SpawnProjectile_ServerRPC(ulong targetId, int damage)
    {
        Vector3 forwardDirection;
        if (shootPoint != null)
        {
            forwardDirection = shootPoint.forward;
        }
        else
        {
            forwardDirection = centerPoint.position - NetworkManager.SpawnManager.SpawnedObjects[targetId].transform.position;
        }
        Quaternion rotation = Quaternion.LookRotation(forwardDirection);

        GameObject projectileObj = Instantiate(projectilePrefab, shootPoint.position, rotation);
        NetworkObject projectileNetwork = projectileObj.GetComponent<NetworkObject>();
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        projectileNetwork.Spawn(true);

        projectile.Init(NetworkManager.SpawnManager.SpawnedObjects[targetId].GetComponent<TowerCore>(), damage);
    }

    private IEnumerator SoundDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioController.Play();
    }



    public bool lookingAtTarget;

    private IEnumerator LookAtTarget_UpdateLoop()
    {
        while (true)
        {
            yield return null;
            yield return new WaitUntil(() => lookAtTransform != null);

            Vector3 targetPosition = lookAtTransform.position;
            if (yRotOnly)
            {
                targetPosition.y = rotPoint.position.y;
            }

            Vector3 direction = targetPosition - rotPoint.position;

            Quaternion targetRotation = Quaternion.LookRotation(direction);


            if (yRotOnly)
            {
                rotPoint.rotation = Quaternion.Euler(rotPoint.rotation.x, Quaternion.RotateTowards(rotPoint.rotation, targetRotation, rotSpeed * Time.deltaTime).eulerAngles.y, rotPoint.rotation.z);
            }
            else
            {
                rotPoint.rotation = Quaternion.RotateTowards(rotPoint.rotation, targetRotation, rotSpeed * Time.deltaTime);
            }

            lookingAtTarget = Quaternion.Angle(rotPoint.rotation, targetRotation) < 0.001f;

            SyncYRotationServerRPC(rotPoint.rotation, lookingAtTarget);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncYRotationServerRPC(Quaternion rotation, bool lookingAtTarget, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        SyncYRotation_ClientRPC(senderClientId, lookingAtTarget, rotation);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncYRotation_ClientRPC(ulong clientId, bool _lookingAtTarget, Quaternion rotation)
    {
        if(NetworkManager.LocalClientId == clientId)
        {
            return;
        }

        lookingAtTarget = _lookingAtTarget;
        rotPoint.rotation = rotation;
    }
}
