using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camerafollow : MonoBehaviour
{
    public Rigidbody carRb;
    public Camera cam;
    public int minFOV;
    public int maxFOV;
    // public bool detach = false;
    // public driving playercar;
    public Vector3 offset;
    // public Vector3 rotation;

    // Start is called before the first frame update
    void Start()
    {
        // carRb = GetComponent<Rigidbody>();
        // offset = GetComponent<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        // transform.position = carRb.position + offset;
        // if(Input.GetKeyDown("a") || Input.GetKeyDown("d"))
        //     transform.rotation = Quaternion.Slerp(transform.rotation, carRb.rotation * Quaternion.Euler(rotation), 5f);
        // transform.rotation = carRb.rotation * Quaternion.Euler(rotation);
        cam.fieldOfView = minFOV + (maxFOV - minFOV) * carRb.velocity.magnitude / 200 * 3.6f;
        // Invoke("Detach", 2f);
    }

    public void Detach()
    {
        transform.parent = null;
    }

    public void Attach()
    {
        transform.parent = carRb.transform;
        transform.localPosition = offset / 100;
        Debug.Log("offsetted");
    }

    public IEnumerator FollowCar()
    {
        while (true)
        {
            if((carRb.position - transform.position).magnitude > offset.magnitude * 1.25)
            {
                transform.parent = carRb.transform;
                transform.localPosition = offset / 100;
                transform.parent = null;
            }
            
            transform.rotation = Quaternion.Euler(  
                                                    Mathf.Rad2Deg * 
                                                    Mod(Mathf.Atan((transform.position.y - carRb.position.y) / 
                                                    (transform.position.x - carRb.position.x)), 180),
                                                    Mod(Mathf.Rad2Deg * 
                                                    Mathf.Atan((carRb.position.x - transform.position.x) / 
                                                    (carRb.position.z - transform.position.z)), 180) + 180, 
                                                    0);
            // Debug.Log(transform.rotation);
            yield return null;
        }
    }

    float Mod(float value, float length)
    {
        return (value % length + length) % length;
    }
}
