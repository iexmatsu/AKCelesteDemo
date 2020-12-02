using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicInit : MonoBehaviour
{
    public AK.Wwise.State InitState;
    void Start()
    {
        InitState.SetValue();
    }
}
