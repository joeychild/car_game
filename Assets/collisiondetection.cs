using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Side
{
    Left,
    Right,
    Brake,
    Tailgate
}
public class collisiondetection : MonoBehaviour
{
    [HideInInspector]
    public BoxCollider trigger;
    public Side side;
    public npcdriving npc;

    // Start is called before the first frame update
    void Start()
    {
        trigger = GetComponent<BoxCollider>();
    }
    
    // Update is called once per frame
    // void OnTriggerEnter(Collider other) //duplicate for ontriggerexit
    // {
    //     // if(trigger.name == "left")
    //     //     npc.lChange = false;
    //     // else if(trigger.name == "right")
    //     //     npc.rChange = false;
    // }

    void OnTriggerStay(Collider other) //duplicate for ontriggerexit
    {
        Debug.Log("triggered by " + other.transform.parent);
        if(npc.crashed)
        {
            Debug.Log("disabled");
            GetComponent<BoxCollider>().enabled = false;
            
        }
        if(trigger.name.Equals("brake"))
        {
            if(!other.name.Equals("road"))
            {
                Debug.Log("not road and not crashed");
                float other_speed = other.transform.parent.GetComponent<Rigidbody>().velocity.magnitude;
                if(npc.carRb.velocity.magnitude - other_speed > 0)
                {
                    npc.speed = Mathf.Lerp(npc.speed, .95f * other_speed, Time.deltaTime * npc.speed / 100);
                    npc.WheelEffects(true);
                    npc.BrakeSounds(true);
                }
                else
                {
                    npc.speed = Mathf.Lerp(npc.speed, npc.oldSpeed, Time.deltaTime * npc.speed / 100);
                    npc.WheelEffects(false);
                    npc.BrakeSounds(false);
                }
            }
        }
    }

    void OnTriggerExit(Collider other) //duplicate for ontriggerexit
    {
        // if(trigger.name == "left")
        //     npc.lChange = true;
        // else if(trigger.name == "right")
        //     npc.rChange = true;
    }
}
