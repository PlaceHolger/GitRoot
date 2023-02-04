using System;
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

    private Actions _actions;

    private PlayerCollector _collector;

    [Serializable]
    public class PlayerInfo
    {
        public int score;
    }

    private void Awake()
    {
        _actions = GetComponent<Actions>();
        _collector = GetComponent<PlayerCollector>();
    }

    public void OnHit()
    {
        _actions.InterruptShoot();
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

    public PlayerInfo GetStats()
    {
        PlayerInfo stats = new PlayerInfo();
        stats.score = _collector.CurrentScore;
        return stats;
    }

    public void Reset()
    {
        _collector.Reset();
        _actions.Reset();
    }
}
