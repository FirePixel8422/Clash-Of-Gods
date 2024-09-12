using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionArrowValidator : MonoBehaviour
{
    private DirectionArrow[] movementArrows;


    private void Start()
    {
        movementArrows = GetComponentsInChildren<DirectionArrow>();
    }



    public void ValideDirectionArrows()
    {
        foreach (var movementArrow in movementArrows)
        {
            movementArrow.VaidateMovementAndAttacks();
        }
    }
}
