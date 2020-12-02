using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeButton : MonoBehaviour
{
    public void AwakeFromSuspend()
    {
        AkSoundEngine.WakeupFromSuspend();
    }
}
