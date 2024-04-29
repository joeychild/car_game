using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class daynight : MonoBehaviour
{
    public Material day;
    public Material night;
    public Light sun;
    public bool isDay;

    // Start is called before the first frame update
    void Start()
    {
        if(isDay)
        {
            RenderSettings.skybox = day;
            sun.enabled = true;
        }
        else
        {
            RenderSettings.skybox = night;
            sun.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
