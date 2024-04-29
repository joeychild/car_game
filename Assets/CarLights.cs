using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarLights : MonoBehaviour
{
    public enum Side
    {
        Front,
        Back,
        Left,
        Right,
        Reverse
    }

    [System.Serializable]
    new public struct light
    {
        public Light lightObj;
        public Side side;
    }

    [HideInInspector]
    public bool isFrontLightOn;
    [HideInInspector]
    public bool isBackLightOn;

    [HideInInspector]
    public bool lBlink = false;
    [HideInInspector]
    public bool rBlink = false;

    public daynight dayNightScript;

    public List<light> lights;

    void Start()
    {
        if(dayNightScript.isDay)
            isFrontLightOn = true;
        else
            isFrontLightOn = false;
        OperateFrontLights();
    }

    public void OperateFrontLights()
    {
        isFrontLightOn = !isFrontLightOn;
        if (isFrontLightOn)
        {
            //Turn On Lights
            foreach(var light in lights)
            {
                if(light.side == Side.Front)
                {
                    light.lightObj.intensity = 5;
                }
            }
        }
        else
        {
            //Turn Off Lights
            foreach (var light in lights)
            {
                if (light.side == Side.Front)
                {
                    light.lightObj.intensity = .5f;
                }
            }
        }
    }

    public void OperateBackLights()
    {
        

        if (isBackLightOn)
        {
            //Turn On Lights
            foreach (var light in lights)
            {
                if (light.side == Side.Back)
                {
                    light.lightObj.intensity = 2;
                }
            }
        }
        else
        {
            //Turn Off Lights
            foreach (var light in lights)
            {
                if (light.side == Side.Back)
                {
                    light.lightObj.intensity = .5f;
                }
            }
        }
    }

    public void OperateBlinkerLights()
    {
        foreach(var light in lights)
        {
            if(light.side == Side.Left || light.side == Side.Right)
            {
                if((light.side == Side.Left && lBlink) || (light.side == Side.Right && rBlink))
                {
                    light.lightObj.enabled = !light.lightObj.enabled;
                }
                else
                {
                    light.lightObj.enabled = false;
                }
            }
        }
    }

    public void OperateReverseLights()
    {
        foreach(var light in lights)
        {
            if(light.side == Side.Reverse)
            {
                if(Input.GetKey("s"))
                    light.lightObj.enabled = true;
                else
                    light.lightObj.enabled = false;
            }
        }
    }
}