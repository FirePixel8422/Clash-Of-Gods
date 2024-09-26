using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FlyingTiles : MonoBehaviour
{
    public static FlyingTiles Instance;
    private void Awake()
    {
        Instance = this;
    }



    public IEnumerator FlyTilesToPositionY(FlyGroup group, float endPosY)
    {
        group.speeds = new float[group.tiles.Count];

        for (int i = 0; i < group.tiles.Count; i++)
        {
            group.speeds[i] = Random.Range(group.speedMin, group.speedMax);
        }


        yield return new WaitForSeconds(Random.Range(group.startDelayMin, group.startDelayMax));


        bool[] tilesFinished = new bool[group.tiles.Count];

        while (tilesFinished.Contains(false))
        {
            for (int i = 0; i < group.tiles.Count; i++)
            {
                if (tilesFinished[i] == true)
                {
                    continue;
                }

                Vector3 targetPos = new Vector3(group.tiles[i].position.x, endPosY, group.tiles[i].position.z);

                if (group.instantMoveTowards)
                {
                    group.tiles[i].position = VectorLogic.InstantMoveTowards(group.tiles[i].position, targetPos, group.speeds[i] * Time.deltaTime);
                }
                else
                {
                    group.tiles[i].position = Vector3.MoveTowards(group.tiles[i].position, targetPos, group.speeds[i] * Time.deltaTime);
                }

                if (Vector3.Distance(group.tiles[i].position, targetPos) < 0.0001f)
                {
                    group.tiles[i].position = targetPos;
                    tilesFinished[i] = true;
                }
            }

            yield return null;
        }
    }
}


[System.Serializable]
public class FlyGroup
{
    public float speedMin, speedMax;

    public float startDelayMin, startDelayMax;

    public bool instantMoveTowards;

    public List<Transform> tiles;
    public float[] speeds;
}

[System.Serializable]
public class FlyingTilesStats
{
    public List<Transform> tileTransformList;

    public float startPosY;
    public float endPosY;

    public FlyGroup[] flyGroups;



    public void Start()
    {
        int r = 0;
        while (tileTransformList.Count != 0)
        {
            foreach (FlyGroup group in flyGroups)
            {
                r = Random.Range(0, tileTransformList.Count);


                group.tiles.Add(tileTransformList[r]);

                tileTransformList[r].position = new Vector3(tileTransformList[r].position.x, startPosY, tileTransformList[r].position.z);

                tileTransformList.RemoveAt(r);


                if (tileTransformList.Count == 0)
                {
                    break;
                }
            }
        }


        foreach (FlyGroup group in flyGroups)
        {
            FlyingTiles.Instance.StartCoroutine(FlyingTiles.Instance.FlyTilesToPositionY(group, endPosY));
        }
    }
}
