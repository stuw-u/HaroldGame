using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinHelmet : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] float slowdown = 0.1f;
    [SerializeField] AudioSource hatSource;
    [SerializeField] float speedToPitch;
    [SerializeField] float maxPitch = 1.5f;
    [SerializeField] float speedToVolume;

    private float lastPosition;
    private float rotSpeed;
    private float defaultVolume;
    private float rotSlowdownVel;

    private void Start ()
    {
        defaultVolume = hatSource.volume;
    }

    void Update()
    {
        if(Time.deltaTime == 0f)
        {
            hatSource.volume = 0;
            return;
        }

        float vel = (lastPosition - transform.position.y) / Time.deltaTime;
        rotSpeed += vel * speed * Time.deltaTime;

        //var blend = 1f - Mathf.Pow(1f - slowdown, Time.deltaTime * 6000);
        //rotSpeed *= blend;
        rotSpeed = Mathf.SmoothDamp(rotSpeed, 0f, ref rotSlowdownVel, slowdown);

        transform.Rotate(Vector3.forward * rotSpeed, Space.Self);

        lastPosition = transform.position.y;

        hatSource.pitch = Mathf.Min(maxPitch,speedToPitch * Mathf.Abs(rotSpeed));
        float volume = Mathf.Clamp01(speedToVolume * Mathf.Abs(rotSpeed));
        volume *= volume * volume;
        hatSource.mute = !GameManager.IsGameStarted;
        hatSource.volume = volume * defaultVolume;
    }
}
