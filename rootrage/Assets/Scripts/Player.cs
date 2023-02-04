using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject HitParticle;
    [SerializeField] private Animator animator;
    [SerializeField] private float HitFeedbackDuration = 0.5f;
    [SerializeField] private UnityEvent EventOnHit;
    public void OnHit()
    {
        if (animator)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Shoot");
            animator.SetBool("isShooting", false);
            animator.SetTrigger("Hit");
        }
        EventOnHit.Invoke();
        var hitParticle = Instantiate(HitParticle, transform);
        Destroy(hitParticle.gameObject, HitFeedbackDuration);
    }
}
