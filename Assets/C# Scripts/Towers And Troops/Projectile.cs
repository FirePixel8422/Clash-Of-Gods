using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Projectile : NetworkBehaviour
{
    public TowerCore target;
    public int dmg;

    public float targetSize;
    public float size;

    public float moveSpeed;



    public void Init(TowerCore _target, int _dmg)
    {
        target = _target;
        dmg = _dmg;
        StartCoroutine(Updateloop());
    }





    private IEnumerator Updateloop()
    {
        while (true)
        {
            yield return null;


            if (Vector3.Distance(transform.position, target.centerPoint.position) < target.size + size)
            {
                NetworkObject.Despawn(true);

                if (target != null)
                {
                    target.GetAttacked(dmg, GodCore.Instance.RandomStunChance());
                }

                yield break;
            }
            else
            {
                transform.position = VectorLogic.InstantMoveTowards(transform.position, target.centerPoint.position, moveSpeed * Time.deltaTime);

                SyncPositionClientRPC(transform.position);
            }
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncPositionClientRPC(Vector3 pos)
    {
        if (IsServer)
        {
            return;
        }

        transform.position = pos;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, Vector3.one * size);
    }
}
