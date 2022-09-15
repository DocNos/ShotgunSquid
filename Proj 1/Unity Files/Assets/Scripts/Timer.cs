using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float dt;
    public float endTime;
    public float currTime;
    public bool atTime;
    public float originalEnd;
    public bool doUpdate;
    public bool isIncrement;


    public Timer(float _endTime
               , float _currTime
               , bool _doUpdate)
    {
        atTime = false;
        endTime = _endTime;
        currTime = _currTime;
        originalEnd = _endTime;
        doUpdate = _doUpdate;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        atTime = false;
        dt = Time.deltaTime;
        if (currTime < endTime)
        {
            atTime = false;
        }
        if (currTime >= endTime)
        {
            atTime = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        dt = Time.deltaTime;
        
        if (currTime < endTime)
        {
            atTime = false;
        }
        if(currTime >= endTime)
        {
            atTime = true;
        }
        if(doUpdate) //&& !atTime
        {            
            currTime += dt;
            if(currTime >= endTime)
            {
                atTime = true;
                doUpdate = false;
            }
        }
    }

    public void Reset()
    {
        currTime = 0;
        atTime = false;

    }

    public void SetEndTime(float _endtime)
    {
        endTime = _endtime;
    }

    public void ResetEndTime()
    {
        endTime = originalEnd;
    }

    
}
