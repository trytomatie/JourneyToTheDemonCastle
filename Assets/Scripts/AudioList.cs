using UnityEngine;

[CreateAssetMenu(fileName = "AudioList", menuName = "Audio/AudioList", order = 1)]

public class AudioList : ScriptableObject
{
    public AudioClip[] audioClips;
    public float pitchStart = 1;
    public float pitchRandomness = 0;
    public float trimStart = 0;
}
