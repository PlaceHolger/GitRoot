using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public float FadeTime = 0.77f;
    public AudioSource MenuMusic;
    public AudioSource DramaticMusic;
    public AudioSource NormalMusic;

    public void StartMenuMusic()
    {
        MenuMusic.Play();
        MenuMusic.DOFade(1.0f, FadeTime);
        DramaticMusic.DOFade(0.0f, FadeTime).OnComplete(() => DramaticMusic.Stop());
        NormalMusic.DOFade(0.0f, FadeTime).OnComplete(() => NormalMusic.Stop());
    }
    
    public void StartNormalMusic()
    {
        DramaticMusic.Play();
        NormalMusic.Play();
        DramaticMusic.DOFade(0.0f, FadeTime);
        NormalMusic.DOFade(1.0f, FadeTime);
        MenuMusic.DOFade(0.0f, FadeTime).OnComplete(() => MenuMusic.Stop());
    }
    
    public void StartDramaticMusic()
    {
        DramaticMusic.Play();
        NormalMusic.Play();
        DramaticMusic.DOFade(1.0f, FadeTime);
        NormalMusic.DOFade(0.0f, FadeTime);
        MenuMusic.DOFade(0.0f, FadeTime).OnComplete(() => MenuMusic.Stop());
    }
}
