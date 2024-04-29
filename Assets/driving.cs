using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class driving : MonoBehaviour
{

    public enum Axle
    {
        Front,
        Rear
    }
    int cam = 1; //starting camera
    Camera[] cams;

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
    private CarLights carLights;

    public ParticleSystem explosionPrefab;
    public ParticleSystem smoke;

    [Header ("Basic Parameters")]

    public Vector3 _centerOfMass;
    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;
    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;
    float moveInput;
    float steerInput;

    [HideInInspector]
    public bool crashed;
    bool brakeEnd;

    [Header ("Audio Elements")]

    public CarSounds engine;
    public CarSounds alarm;
    public AudioSource crashPrefab;
    public CarSounds[] brakes;
    camerafollow camScript;
        
    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
        // carRb.constraints = RigidbodyConstraints.FreezeRotationZ;
        carLights = GetComponent<CarLights>();
        cams = FindObjectsOfType<Camera>();
        crashed = false;
        carLights.InvokeRepeating("OperateBlinkerLights", 0f, .5f);
        
        engine.SetClip(0);
        engine.carAudio.Play();
        // engine.carAudio.loop = true;
        StartCoroutine(EngineSounds());

        //MUST START WITH CINEMATIC CAM
        camScript = cams[cam].GetComponent<camerafollow>();
        camScript.StartCoroutine(camScript.FollowCar());
    }

    void Update()
    {
        CameraSwitch();
        GetInputs();
        AnimateWheels();
        WheelEffects();
        BrakeSounds();

        if(!crashed)
        {
            Move();
            Steer();
            Brake();
            //StartCoroutine(BlinkerReverse());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Vector3 PoC = collision.contacts[0].point;
        Quaternion quat = Quaternion.Euler(0, 0, 0);
        Debug.Log(PoC);
        ParticleSystem sparks = Instantiate(explosionPrefab, PoC, quat);
        Destroy(sparks, sparks.main.duration);

        if(!(collision.gameObject.name.Equals("road")))
        {
            if(!GameObject.Find("crash (Clone)"))
            {
                AudioSource sound = Instantiate(crashPrefab, PoC, quat);
                Destroy(sound, sound.clip.length);
            }
            Debug.Log("not road");
            Fail(alarm, wheels, carLights);
        }
        else
        {
            Debug.Log("collision with road");
        }
    }

    public void Fail(CarSounds alarm, List<Wheel> wheels, CarLights carLights)
    {
        crashed = true;
        
        alarm.carAudio.loop = true;
        alarm.SetClip(0);
        alarm.carAudio.Play();

        carLights.lBlink = true;
        carLights.rBlink = true;
        foreach (var wheel in wheels)
            wheel.wheelCollider.motorTorque = 0;
        smoke.Play(); 
        Debug.Log("crash complete");
    }
    void CameraSwitch()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            cams[cam].enabled = false;
            cams[cam].GetComponent<AudioListener>().enabled = false;
            cam += Input.GetMouseButtonDown(0) ? 1 : -1;
            cam = (cam % cams.Length + cams.Length) % cams.Length;
            cams[cam].enabled = true;
            cams[cam].GetComponent<AudioListener>().enabled = true;
            // Debug.Log(cams[cam].name);
            if(cams[cam].name == "Cinematic")
            {
                // Debug.Log("if");
                camScript.Attach();
                camScript.Detach();
                camScript.StartCoroutine(camScript.FollowCar());
            }
            else
            {
                camScript.StopCoroutine(camScript.FollowCar());
            }
        }
    }

    public void MoveInput(float input)
    {
        moveInput = input;
    }

    public void SteerInput(float input)
    {
        steerInput = input;
    }

    void GetInputs()
    {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
    }

    void Move()
    {
        // Debug.Log(moveInput);
        foreach(var wheel in wheels)
        {
            if(moveInput != 0 && !Input.GetKey(KeyCode.Space))
                wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
            else
            {
                //Debug.Log("cruising: " + carRb.velocity.magnitude);
                wheel.wheelCollider.motorTorque *= 1 - Time.deltaTime * .15f;
            }
        }

                carLights.OperateReverseLights();
        
        if(Input.GetKeyDown("q"))
        {
            StartCoroutine(Lights(false, alarm, carLights));
        }
        if(Input.GetKeyDown("e"))
        {
            StartCoroutine(Lights(true, alarm, carLights));
        }
    }

    void Steer()
    {
        foreach(var wheel in wheels)
        {
            if (wheel.axle == Axle.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * (37f - Mathf.Clamp(carRb.velocity.magnitude * 0.35f - 2f, 0f, 17f));
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        Vector3 carRot = carRb.rotation.eulerAngles;
        carRot.z = 0;
        carRot.x = 0;

        if (Input.GetKey(KeyCode.Space) || (moveInput * transform.InverseTransformDirection(carRb.velocity).z < 0 && carRb.velocity.magnitude > 4))
        {
            foreach (var wheel in wheels)
                wheel.wheelCollider.motorTorque = 0;

            carLights.isBackLightOn = true;
            carLights.OperateBackLights();
            carRot.x = Mathf.Clamp(carRot.x, -20, 20);
        }
        else if(moveInput * transform.InverseTransformDirection(carRb.velocity).z < 0)
        {
            carLights.isBackLightOn = true;
            carLights.OperateBackLights();
        }
        else
        {
            carLights.isBackLightOn = false;
            carLights.OperateBackLights();
        }

        carRb.rotation = Quaternion.Euler(carRot);
    }

    void AnimateWheels()
    {
        foreach(var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            rot = rot * Quaternion.Euler(new Vector3(0, -90, 0));
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void WheelEffects()
    {
        foreach (var wheel in wheels)
        {
            //var dirtParticleMainSettings = wheel.smokeParticle.main;

            if (
                (Input.GetKey(KeyCode.Space) || 
                moveInput * transform.InverseTransformDirection(carRb.velocity).z < 0 ||
                (steerInput != 0 && carRb.velocity.magnitude < 4) //modify?
                ) &&
                wheel.wheelCollider.isGrounded == true && 
                carRb.velocity.magnitude > 4)
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

    void BrakeSounds()
    {
        if (
            (Input.GetKey(KeyCode.Space) || 
            moveInput * transform.InverseTransformDirection(carRb.velocity).z < 0 ||
            (steerInput != 0 && carRb.velocity.magnitude < 4) //modify?
            ) &&
            carRb.velocity.magnitude > 4)
            {
                foreach(var brake in brakes)
                {
                brake.SetClip(brake.carAudio.isPlaying ? 1 : 0);
                brake.carAudio.loop = brake.carAudio.isPlaying;
                }
                brakeEnd = true;
            }
        else if(brakeEnd)
        {
                // Debug.Log("else if");
                foreach(var brake in brakes)
                {
                brake.SetClip(2);
                brake.carAudio.loop = false;
                }
                brakeEnd = false;
        }
    }

    IEnumerator EngineSounds()
    {
        // Debug.Log("clip: " + engine.carAudio.clip);
        // Debug.Log("audio: " + engine.clips[0]);
            // Debug.Log("be here");
            while(!crashed)
            {
                if(engine.carAudio.clip == engine.clips[0])
                {
                    crashed = true;
                    yield return new WaitForSeconds(engine.carAudio.clip.length - 1.5f);
                    crashed = false;
                    engine.carAudio.loop = true;
                    engine.SetClip(1); //fix audio change later
                }
                if(!(carRb.velocity.magnitude > engine.maxSpeed || carRb.velocity.magnitude < engine.minSpeed))
                {
                    engine.SetClip(2);
                    engine.ClipPitch();
                }
                yield return null;
            }
            engine.carAudio.Stop();
    }

    public IEnumerator Lights(bool right, CarSounds alarm, CarLights carLights) //blinkers are right or left
    {
        bool off = false;
        bool toggle = right ? !carLights.rBlink : !carLights.lBlink;

        carLights.lBlink = right ? off : toggle;
        carLights.rBlink = right ? toggle : off;

        if(toggle)
        {
            alarm.SetClip(1);
            alarm.carAudio.loop = false;
            yield return new WaitForSeconds(.3f);
            alarm.SetClip(2);
            alarm.carAudio.loop = true;
        }
        else
        {
            alarm.SetClip(3);
            alarm.carAudio.loop = false;
        }
    }

    // Quaternion ClampRotation(Quaternion q, Vector3 bounds)
    // {
    //     q.x /= q.w;
    //     q.y /= q.w;
    //     q.z /= q.w;
    //     q.w = 1.0f;
    
    //     float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
    //     angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
    //     q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
    
    //     float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
    //     angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
    //     q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);
    
    //     float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
    //     angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);
    //     q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);
    
    //     return q.normalized;
    // }
}