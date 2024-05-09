using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class daynight : MonoBehaviour
{
    public Material day;
    public Material night;
    public Light sun;
    public bool isDay;
    public GameObject warehouse;

    // Start is called before the first frame update
    void Start()
    {
        if(isDay)
        {
            RenderSettings.skybox = day;
            sun.enabled = true;
            foreach (Light light in warehouse.GetComponentsInChildren<Light>())
                light.enabled = false;
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
