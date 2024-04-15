using UnityEngine;
using System;
using System.Collections.Generic;

public class driving : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Buttons
    };

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

    public ControlMode control;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    public Rigidbody carRb;
    private CarLights carLights;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
        carRb.constraints = RigidbodyConstraints.FreezeRotationZ;
        carLights = GetComponent<CarLights>();
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
        WheelEffects();
    }

    void LateUpdate()
    {
        Move();
        Steer();
        Brake();
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
        if(control == ControlMode.Keyboard)
        {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
        }
    }

    void Move()
    {
        foreach(var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach(var wheel in wheels)
        {
            if (wheel.axle == Axle.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * (37f - Mathf.Clamp(GetComponent<Rigidbody>().velocity.magnitude * 0.35f - 2f, 0f, 17f));
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300f * brakeAcceleration * Time.deltaTime;
            }

            carLights.isBackLightOn = true;
            carLights.OperateBackLights();
        }
        else if(moveInput == 0)
        {
            GetComponent<Rigidbody>().velocity *= .9999999f * Time.deltaTime / Time.deltaTime;
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }

            carLights.isBackLightOn = false;
            carLights.OperateBackLights();
        }
    }

    void AnimateWheels()
    {
        foreach(var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            rot = rot * Quaternion.Euler(new Vector3(0, -90, 0));
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void WheelEffects()
    {
        foreach (var wheel in wheels)
        {
            //var dirtParticleMainSettings = wheel.smokeParticle.main;

            if (Input.GetKey(KeyCode.Space) && wheel.axle == Axle.Rear && wheel.wheelCollider.isGrounded == true && carRb.velocity.magnitude >= 10.0f)
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