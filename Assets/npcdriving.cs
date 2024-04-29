using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class npcdriving : MonoBehaviour
{
    public enum Axle
    {
        Front,
        Rear
    }
    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public ParticleSystem smokeParticle;
        public Axle axle;

    }

    [Header ("Basic Elements")]

    public Rigidbody carRb;
    public List<Wheel> wheels;
    public Vector3 _centerOfMass;
    public driving playerCar;
    public ParticleSystem smoke;
    public CarLights carLights;

    [Header ("Path Elements")]

    public PathCreator path;
    public float speed; //kph
    public float distance; //initial position on path
    public float offset;
    public float laneChangeProbability;
    float lcOffset;
    bool crashed = false;

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
        // StartCoroutine(LaneChange());
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("velocity: " + carRb.velocity.magnitude);
        if(!crashed)
        {
            distance += speed / 3.6f * Time.deltaTime;
            transform.position = path.path.GetPointAtDistance(distance) + Vector3.right * (offset + lcOffset);
            transform.rotation = path.path.GetRotationAtDistance(distance) * Quaternion.Euler(0, 0, 90);
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
        if(collision.gameObject.name.Equals("player car"))
        {
            smoke.Play();
            crashed = true;
            engine.carAudio.Stop();
            StopCoroutine(LCMovement());
            carRb.velocity = path.path.GetDirectionAtDistance(distance) * speed;
            carRb.drag = 1;
            playerCar.Fail(alarm, playerCar.wheels, carLights); //cant convert from driving.Wheel to npcdriving.Wheel
            foreach(var wheel in wheels)
                wheel.wheelCollider.motorTorque = 0;
        }
    }

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
                if(rng < laneChangeProbability / 2 && offset > -10) //leftmost point
                {
                    StartCoroutine(playerCar.Lights(false, alarm, carLights));
                }
                else if (offset < 10) //rightmost point
                {
                    StartCoroutine(playerCar.Lights(true, alarm, carLights));
                }
                else
                    yield break;
                Debug.Log("blinker on");
                yield return new WaitForSeconds(2);
                Debug.Log("movement changed");
                // float prevOffset = offset;
                // float frame = offset;
                float time = 0;
                while(Math.Abs(lcOffset) < 5) //lane change offset
                {
                    // offset += carLights.rBlink ? (float)Math.Sin(Time.deltaTime * 2) : -1 * (float)Math.Sin(Time.deltaTime * 2);
                    lcOffset = 5.5f * (float)Math.Sin(carLights.rBlink ? time : -1 * time);
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
