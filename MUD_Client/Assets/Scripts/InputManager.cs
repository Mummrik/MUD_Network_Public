using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private enum InputId
    {
        None,
        WalkNorth,
        WalkEast,
        WalkSouth,
        WalkWest,
    }

    private const string moveHorizontal = "Horizontal";
    private const string moveVertical = "Vertical";

    private void Update()
    {
        CheckKeyInput();
    }

    private void CheckKeyInput()
    {
        float verticalMove = Input.GetAxisRaw(moveVertical);
        if (verticalMove != 0)
        {
            if (verticalMove > 0)
                Client.KeyInput((int)InputId.WalkNorth);
            else
                Client.KeyInput((int)InputId.WalkSouth);
        }

        float horizontalMove = Input.GetAxisRaw(moveHorizontal);
        if (horizontalMove != 0)
        {
            if (horizontalMove > 0)
                Client.KeyInput((int)InputId.WalkEast);
            else
                Client.KeyInput((int)InputId.WalkWest);
        }
    }
}
