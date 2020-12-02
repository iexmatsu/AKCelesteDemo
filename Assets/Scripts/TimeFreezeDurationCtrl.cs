using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeFreezeDurationCtrl : MonoBehaviour
{
    public Movement movement;
    private Slider TimeFrzDur;
    // Start is called before the first frame update
    void Start()
    {
        TimeFrzDur = GetComponent<Slider>();
    }

    // Update is called once per frame
    public void changeFrzDur()
    {
        movement.MaxTimeFrzDur = TimeFrzDur.value * 15f;
    }
    
}
