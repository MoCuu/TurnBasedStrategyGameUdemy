using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePoint : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (Input.mousePosition.y > Screen.height * .1f)
        {
            GameManager.instance.activePlayer.MoveToPoint(transform.position);

            MoveGrid.instance.HideMovePoints();

            PlayerInputMenu.instance.HideMenus();
        }
    }
}
