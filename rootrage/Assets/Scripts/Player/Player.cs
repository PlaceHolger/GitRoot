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
    private GameManager _manager;
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Mesh _skinnedMesh;

    [Serializable]
    public class PlayerInfo
    {
        public int score;
    }

    private void Awake()
    {
        _actions = GetComponent<Actions>();
        _collector = GetComponent<PlayerCollector>();
        _manager = FindObjectOfType<GameManager>();
    }

    void FixedUpdate()
    {
        _skinnedMeshRenderer.SetBlendShapeWeight(0, 100.0f * _collector.CurrentScore / (_manager.WinningScore - 1.0));
    }

    public void OnHit()
    {
        if (!_actions.IsStunned && _collector.CurrentScore > 0)
        {
            _collector.CurrentScore = _collector.CurrentScore - 1;
            _manager.DropCollectables(transform.position);
        }

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

    public void Reset(GameObject spawnpoint)
    {
        if (spawnpoint)
        {
            transform.position = spawnpoint.transform.position;
            transform.rotation = spawnpoint.transform.rotation;
        }
        _collector.Reset();
        _actions.Reset();
    }
}
