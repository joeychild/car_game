using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using Unity.VisualScripting;

public class npcdriving : MonoBehaviour
{

    [Header ("Basic Elements")]

    public Rigidbody carRb;
    public List<Wheel> wheels;
    public List<CarSounds> brakes;
    public Vector3 _centerOfMass;
    public driving playerCar;
    public ParticleSystem smoke;
    public CarLights carLights;
    public collisiondetection detection; //left, right, swerve, brake, tailgate
    public CarSpawner carSpawner;

    [Header ("Path Elements")]

    public PathCreator path;
    public float speed; //kph
    [HideInInspector]
    public float oldSpeed;
    public float distance; //initial position on path
    public float offset;
    public float laneChangeProbability;
    float lcOffset;
    
    [HideInInspector]
    public bool lChange = true;
    [HideInInspector]
    public bool rChange = true;
    Vector3 prevPosition;
    [HideInInspector]
    public bool crashed = false;

    [Header ("Audio Elements")]

    public CarSounds alarm;
    public CarSounds engine;

    // Start is called before the first frame update
    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
        carRb.constraints = RigidbodyConstraints.FreezeRotationZ;
        carLights.InvokeRepeating("OperateBlinkerLights", 0f, .5f);
        StartCoroutine(LCMovement());
        oldSpeed = speed;
        Debug.Log(path.path.ToString());
        // StartCoroutine(LaneChange());
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("velocity: " + carRb.velocity.magnitude);
        if(!crashed)
        {
            distance += UnityEngine.Random.Range(speed * .9f, speed * 1.1f) / 3.6f * Time.deltaTime;
            // distance += Time.deltaTime * speed / 100;
            prevPosition = transform.position;
            transform.position = path.path.GetPointAtDistance(distance) + Vector3.right * (offset + lcOffset);
            transform.rotation = path.path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90); //change offset: (transform.position.x - prevPosition.x) / Time.deltaTime
            engine.ClipPitch();
        }
        Vector3 carRot = carRb.rotation.eulerAngles;
        carRot.z = 0;
        carRot.x = 0;
        carRb.rotation = Quaternion.Euler(carRot);
        AnimateWheels();
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("colliding");
        Debug.Log("detection: " + detection.trigger.enabled);
        if(!collision.gameObject.name.Equals("road") && !collision.gameObject.name.Equals("brake"))
        {
            smoke.Play();
            engine.carAudio.Stop();
            detection.trigger.enabled = false;
            Debug.Log("in loop " + detection.trigger.enabled);
            StopCoroutine(LCMovement());
            if(!crashed)
                carRb.velocity = path.path.GetDirectionAtDistance(distance) * speed;
            crashed = true;
            carRb.drag = 1;
            playerCar.Fail(alarm, wheels, carLights);
        }
    }

    /**************************************************************************************
                                      COLLISION DETECTION
    **************************************************************************************/

    // void OnTriggerEnter(Collider other) //duplicate for ontriggerexit
    // {
    //     if(other.name == "left" || other.name == "right" || other.name == "brake")
    //     {
    //     if(other.transform.parent.name == "detection")
    //     {
    //         if(other.GetComponent<collisiondetection>().side == Side.Left)
    //             lChange = false;
    //         else if(other.GetComponent<collisiondetection>().side == Side.Right)
    //             rChange = false;
    //     }
    //     }
    // }

    // void OnTriggerStay(Collider other)
    // {
    //     if(other.name == "left" || other.name == "right" || other.name == "brake")
    //     {
    //     Debug.Log("parent: " + other.transform.parent);
    //     Debug.Log("grandparent: " + other.transform.parent.parent);
    //     float other_speed = other.transform.parent.parent.GetComponent<Rigidbody>().velocity.magnitude;
    //     if(other.transform.parent.name == "detection" && other.GetComponent<collisiondetection>().side == Side.Brake && carRb.velocity.magnitude - other_speed > 5)
    //     {
    //         speed = Mathf.Lerp(speed, other_speed, Time.deltaTime * speed);
    //         WheelEffects(true);
    //         BrakeSounds(true);
    //     }
    //     else
    //     {
    //         speed = Mathf.Lerp(speed, oldSpeed, Time.deltaTime * speed);
    //         WheelEffects(false);
    //         BrakeSounds(false);
    //     }
    //     }
    // }

    // void OnTriggerExit(Collider other) //duplicate for ontriggerexit
    // {
    //     if(other.name == "left" || other.name == "right" || other.name == "brake")
    //     {
    //     if(other.transform.parent.name == "detection")
    //     {
    //         if(other.GetComponent<collisiondetection>().side == Side.Left)
    //             lChange = true;
    //         else if(other.GetComponent<collisiondetection>().side == Side.Right)
    //             rChange = true;
    //     }
    //     }
    // }
    

    void AnimateWheels()
    {
        foreach(var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            rot *= Quaternion.Euler(0, -90, 250 * carRb.velocity.magnitude);
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    public void WheelEffects(bool start) //start or stop effects
    {
        foreach (var wheel in wheels)
        {
            //var dirtParticleMainSettings = wheel.smokeParticle.main;

            if(start)
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                wheel.smokeParticle.Emit(1);
            }
            else
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }

    public void BrakeSounds(bool start)
    {
        if (start)
            {
                foreach(var brake in brakes)
                {
                brake.SetClip(brake.carAudio.isPlaying ? 1 : 0);
                brake.carAudio.loop = brake.carAudio.isPlaying;
                }
            }
        else
        {
                // Debug.Log("else if");
                foreach(var brake in brakes)
                {
                brake.SetClip(2);
                brake.carAudio.loop = false;
                }
        }
    }

    // IEnumerator LaneChange()
    // {
    //     if(UnityEngine.Random.Range(0, 1f) < 1f) //change probability
    //     {
    //         carLights.lBlink = true;
    //         Debug.Log("blinker on");
    //         // Timer(2);
    //         // timer = 0;
    //         // InvokeRepeating("LCMovement", 0, Time.deltaTime);
    //         yield return new WaitForSeconds(2);
    //         changing_lane = true;
    //         foreach(var wheel in wheels)
    //         {
    //             Debug.Log("wheels");
    //             wheel.wheelCollider.motorTorque = 600 * speed * Time.deltaTime;
    //             if (wheel.axle == Axle.Front)
    //             {
    //                 Debug.Log("front axle turning");
    //                 wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, -15, 1f);
    //                 Debug.Log("wheel collider angle: " + wheel.wheelCollider.steerAngle);
    //                 Quaternion rot;
    //                 Vector3 pos;
    //                 wheel.wheelCollider.GetWorldPose(out pos, out rot);
    //                 rot *= Quaternion.Euler(0, -90, 1000 * speed * Time.deltaTime);
    //                 wheel.wheelModel.transform.rotation = rot;
    //             }
    //         }
    //         yield return new WaitForSeconds(2);
    //         Debug.Log("turning back");
    //         foreach(var wheel in wheels)
    //         {
    //             if (wheel.axle == Axle.Front)
    //             {
    //                 wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, 15, 1f);
    //                 Quaternion rot;
    //                 Vector3 pos;
    //                 wheel.wheelCollider.GetWorldPose(out pos, out rot);
    //                 rot *= Quaternion.Euler(0, -90, 1000 * speed * Time.deltaTime);
    //                 wheel.wheelModel.transform.rotation = rot;
    //             }
    //         }
    //         yield return new WaitForSeconds(2);
    //         changing_lane = false;
    //         carLights.lBlink = false;
    //         Debug.Log("blinker off");
    //         yield return null;
    //     }
    // }

    // void Timer(float time)
    // {
    //     timer = 0;
    //     Debug.Log("timer: " + timer);
    //     while(timer < time)
    //     {
    //         timer += Time.deltaTime;
    //         Debug.Log("timer: " + timer);
    //     }
    //     Debug.Log("timer: " + timer);
    // }

    IEnumerator LCMovement()
    {
        while(!crashed)
        {
            Debug.Log("wait");
            yield return new WaitForSeconds(UnityEngine.Random.Range(10f, 20f));
            Debug.Log("wait ended");
            float rng = UnityEngine.Random.Range(0, 1);
            if(rng < laneChangeProbability)
            {
                if(rng < laneChangeProbability / 2 && offset > -10 && lChange) //leftmost point
                {
                    StartCoroutine(playerCar.Lights(false, alarm, carLights));
                }
                else if (offset < 10 && rChange) //rightmost point
                {
                    StartCoroutine(playerCar.Lights(true, alarm, carLights));
                }
                else
                    yield break;
                Debug.Log("blinker on");
                yield return new WaitForSeconds(2);
                Debug.Log("movement changed");
                // float frame = offset;
                float time = 0;
                while(Math.Abs(lcOffset) < 4.99f) //lane change offset
                {
                    // offset += carLights.rBlink ? (float)Math.Sin(Time.deltaTime * 2) : -1 * (float)Math.Sin(Time.deltaTime * 2);
                    lcOffset = 5f * (float)Math.Sin(carLights.rBlink ? time : -1 * time);
                    // Debug.Log(Math.Sin(time));
                    // frame = offset;
                    time += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                    // rOffset *= Quaternion.Euler(0, -1 * (float)Math.Sin(Time.deltaTime), 0);
                }

                offset += lcOffset;
                lcOffset = 0;
                
                // Debug.Log("lerped");
                // yield return new WaitForSeconds(2);
                yield return new WaitForSeconds(2);
                if(carLights.lBlink)
                    StartCoroutine(playerCar.Lights(false, alarm, carLights));
                else
                    StartCoroutine(playerCar.Lights(true, alarm, carLights));
                Debug.Log("blinker off");
            }
        }
        // CancelInvoke("LCMovement");
    }
}
