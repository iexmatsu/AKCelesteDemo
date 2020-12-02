using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDetector : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Debug.LogError("It's the jump button");
        }
        if (Input.GetButtonDown("Shoot"))
        {
            Debug.LogError("It's the Shoot button");
        }
        if (Input.GetButtonDown("Dash"))
        {
            Debug.LogError("It's the Dash button");
        }
        if (Input.GetButtonDown("WallGrab"))
        {
            Debug.LogError("It's the WallGrab button");
        }
    }
}
