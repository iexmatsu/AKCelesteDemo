using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFreeze : MonoBehaviour
{
    private ParticleSystem ps;
    float rtpcValue;
    public string rtpcID = "TimeFreezeL";

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        //var main = ps.main;
        int valueType = (int)AkQueryRTPCValue.RTPCValue_Global;
        AKRESULT result = AkSoundEngine.GetRTPCValue(rtpcID, gameObject, 0, out rtpcValue, ref valueType);
        //main.simulationSpeed = map(rtpcValue, -48.0f, 0.0f, 1.0f, 0.2f);
        Time.timeScale = map(rtpcValue, -48.0f, 0.0f, 1.0f, 0.2f);
    }
	
    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1+(s-a1)*(b2-b1)/(a2-a1);
    }
}
