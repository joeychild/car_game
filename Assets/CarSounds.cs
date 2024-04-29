using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSounds : MonoBehaviour
{
    public float minSpeed;
    public float maxSpeed;
    private float currentSpeed;

    public Rigidbody carRb;
    public AudioSource carAudio;
    public float minPitch;
    public float maxPitch;
    public AudioClip[] clips;

    private float pitchFromCar;

    void Start()
    {
        carAudio = GetComponent<AudioSource>();
    }

    public void SetClip(int index)
    {
        if(carAudio.clip != clips[index])
        {
            carAudio.clip = clips[index];
            carAudio.Play();
        }
    }

    public void ClipPitch()
    {
        carAudio.pitch = minPitch + carRb.velocity.magnitude / 50f * (maxPitch - minPitch);
    }
}