using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ui : MonoBehaviour
{

    public TMP_Text speed;
    public Rigidbody carRb;
    public RectTransform dial;
    bool settings = false;
    // Start is called before the first frame update
    void Start()
    {
        // carRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Dial();
        Settings();
    }

    void Dial()
    {
        speed.text = "" + Math.Round(carRb.velocity.magnitude * 3.6f);
        if (carRb.velocity.magnitude < 100 / 3.6f)
            dial.rotation = Quaternion.Euler(0,0,110 - carRb.velocity.magnitude * 3.6f * 23 / 15);
        else if (carRb.velocity.magnitude < 200 / 3.6f)
            dial.rotation = Quaternion.Euler(0,0, 110 - 2300 / 15 - (carRb.velocity.magnitude * 3.6f - 100) * 23 / 30);
        else
            dial.rotation = Quaternion.Euler(0, 0, -120 - (float)Math.Sin((double)carRb.velocity.magnitude * 10));
    }

    void Settings()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            settings = !settings;
        if(settings)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}
