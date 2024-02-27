using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] woodHit;
    public AudioClip[] stoneHit;
    public AudioClip[] entityHit;
    public AudioClip[] generalAudio;

    public AudioList[] audioLists;
    public AudioMixerGroup sfxAudioGroup;

    public GameObject audioSourcePrefab;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    public static void PlayHitSound(Vector3 position, HitType type)
    {
        GameObject audioSource = Instantiate(instance.audioSourcePrefab, position, Quaternion.identity);
        AudioSource source = audioSource.GetComponent<AudioSource>();
        switch (type)
        {
            case HitType.Wood:
                source.clip = instance.woodHit[Random.Range(0, instance.woodHit.Length)];
                break;
            case HitType.Stone:
                source.clip = instance.stoneHit[Random.Range(0, instance.stoneHit.Length)];
                break;
            case HitType.Entity:
                source.clip = instance.entityHit[Random.Range(0, instance.entityHit.Length)];
                break;
        }
        source.outputAudioMixerGroup = instance.sfxAudioGroup;
        source.Play();
        Destroy(audioSource, source.clip.length + 0.1f);
    }

    public static void PlaySound(Vector3 position, SoundType type)
    {
        if(type == SoundType.None) return;
        GameObject audioSource = Instantiate(instance.audioSourcePrefab, position, Quaternion.identity);
        AudioSource source = audioSource.GetComponent<AudioSource>();
        source.clip = instance.audioLists[(int)type].audioClips[Random.Range(0, instance.audioLists[(int)type].audioClips.Length)];
        source.outputAudioMixerGroup = instance.sfxAudioGroup;
        source.Play();
        Destroy(audioSource, source.clip.length + 0.1f);
    }
}

public enum HitType
{
    Wood,
    Stone,
    Entity
}

public enum SoundType
{
    None = -1,
    Player_Dash = 0,
    Dog_Bark = 1,
    Dog_Cry = 2
}
