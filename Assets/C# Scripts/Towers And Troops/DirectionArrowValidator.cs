using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionArrowValidator : MonoBehaviour
{
    private DirectionArrow[] direactionArrows;

    public GameObject directionArrowPrefab;

    public Vector2Int[] directions;

    public Sprite arrowSprite;
    public Sprite diagonalArrowSprite;


    public bool drawGizmos;
    public float refTileSize;



    public void Init()
    {
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject spawnedObj = Instantiate(directionArrowPrefab, transform, true);

            DirectionArrow directionArrow = spawnedObj.GetComponent<DirectionArrow>();


            directionArrow.transform.localPosition = new Vector3(directions[i].x * GridManager.Instance.tileSize, 0.03f, directions[i].y * GridManager.Instance.tileSize);


            float angle = Mathf.Atan2(directions[i].y, directions[i].x) * Mathf.Rad2Deg;
            
            //Round 45 Degrees upwards to 90 degrees
            angle = Mathf.Ceil(angle / 90) * 90 - 90;


            directionArrow.transform.localRotation = Quaternion.Euler(90, 0, angle);


            bool diagonalArrow = directions[i].x != 0 && directions[i].y != 0;

            directionArrow.Setup(directions[i], diagonalArrow ? diagonalArrowSprite : arrowSprite);
        }


        direactionArrows = GetComponentsInChildren<DirectionArrow>();
    }



    public void ValideDirectionArrows()
    {
        foreach (var movementArrow in direactionArrows)
        {
            movementArrow.OnValidateArrow();
        }
    }




    private void OnDrawGizmos()
    {
        if (drawGizmos == false)
        {
            return;
        }

        for (int i = 0; i < directions.Length; i++)
        {
            Gizmos.DrawWireCube(transform.position + new Vector3(directions[i].x * refTileSize, 0.03f, directions[i].y * refTileSize), new Vector3(refTileSize * .9f, .15f, refTileSize * .9f));
        }
    }
}
