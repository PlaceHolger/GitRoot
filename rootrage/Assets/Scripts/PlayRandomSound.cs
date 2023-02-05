using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayRandomSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSourceToUse;
    [SerializeField] private List<AudioClip> clips;

    private void Awake()
    {
        if (!audioSourceToUse)
            audioSourceToUse = GetComponent<AudioSource>();
    }

    [ContextMenu("Play Random Sound")]
    public void Play()
    {
        int randomId = Random.Range(0, clips.Count);
        audioSourceToUse.PlayOneShot(clips[randomId]);
    }
}
