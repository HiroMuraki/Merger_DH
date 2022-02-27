using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    static SoundManager _instance;
    AudioSource _audioSource;

    public static SoundManager Instance {
        get {
            return _instance;
        }
    }
    public AudioClip MergedSound;
    public AudioClip TouchGroundSound;
    public AudioClip MeowSound;

    void Awake() {
        _instance = this;
        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayOneShot(AudioClip clip) {
        _audioSource.PlayOneShot(clip);
    }
}
