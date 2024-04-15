using System;
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

    public Rigidbody carRb;
    public List<Wheel> wheels;
    public PathCreator path;
    public float speed = 5;
    float distance;
    public Vector3 _centerOfMass;

    // Start is called before the first frame update
    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
        carRb.constraints = RigidbodyConstraints.FreezeRotationZ;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var wheel in wheels)
        {
            //steer angle is wrong
            // if (wheel.axle == Axle.Front)
            // {
            //     wheel.wheelCollider.steerAngle = path.path.GetRotationAtDistance(distance).eulerAngles.y;
            //     Debug.Log(path.path.GetRotationAtDistance(distance).eulerAngles.y);
            // }
        }
        distance += speed * Time.deltaTime;
        transform.position = path.path.GetPointAtDistance(distance);
        transform.rotation = path.path.GetRotationAtDistance(distance) * Quaternion.Euler(new Vector3(0, 0, 90));
        AnimateWheels();
    }

    void AnimateWheels()
    {
        foreach(var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            rot = rot * Quaternion.Euler(new Vector3(0, -90, 10000* speed * Time.deltaTime));
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }
}
