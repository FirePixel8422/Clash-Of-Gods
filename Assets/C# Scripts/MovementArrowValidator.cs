using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementArrowValidator : MonoBehaviour
{
    private MovementArrow[] movementArrows;


    private void Start()
    {
        movementArrows = GetComponentsInChildren<MovementArrow>();
    }



    public void ValideAllMovementArrows()
    {
        foreach (var movementArrow in movementArrows)
        {
            movementArrow.VaidateForVaildTile();
        }
    }
}
