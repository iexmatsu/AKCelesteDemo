using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class suspendButton : MonoBehaviour
{
    public bool in_bRenderAnyway = true;
    public void suspend()
    {
        AkSoundEngine.Suspend();
    }
}
