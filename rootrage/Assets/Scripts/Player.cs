using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject HitParticle;
    [SerializeField] private float HitFeedbackDuration = 0.5f;
    
    public void OnHit()
    {
        var hitParticle = Instantiate(HitParticle, transform);
        Destroy(hitParticle.gameObject, HitFeedbackDuration);
    }
}
