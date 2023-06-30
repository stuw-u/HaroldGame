using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayEvent : MonoBehaviour
{
    AudioSource source;
    private void Awake ()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play ()
    {
        source?.Play();
    }
}
