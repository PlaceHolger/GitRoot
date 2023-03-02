using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject HitParticle;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
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
        _manager = FindObjectOfType<GameManager>(true);
        if(!networkAnimator)
            networkAnimator = GetComponentInChildren<NetworkAnimator>();
    }

    void FixedUpdate()
    {
        _skinnedMeshRenderer.SetBlendShapeWeight(0, 100.0f * _collector.CurrentScore / (_manager.WinningScore - 1.0f));
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnHitServerRpc()
    {
        if (!_actions.IsStunned && _collector.CurrentScore > 0)
        {
            _collector.SetScoreServerRpc(_collector.CurrentScore - 1);
            _manager.DropCollectables(transform.position);
        }
        
        OnHitClientRpc();
    }
    
    [ClientRpc]
    private void OnHitClientRpc()
    {
        _actions.InterruptShoot();
        if (animator && IsOwner)
        {
            animator.SetBool("isShooting", false);
            if (networkAnimator)
            {
                networkAnimator.ResetTrigger("Attack");
                networkAnimator.ResetTrigger("Shoot");
                networkAnimator.SetTrigger("Hit");
            }
            else
            {
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("Shoot");
                animator.SetTrigger("Hit");
            }
        }
        EventOnHit.Invoke();
        var hitParticle = Instantiate(HitParticle, transform);
        Destroy(hitParticle.gameObject, HitFeedbackDuration);
    }

    public PlayerInfo GetStats()
    {
        return new PlayerInfo
        {
            score = _collector.CurrentScore
        };
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetServerRpc(Vector3 spawnPos)
    {
        ResetClientRpc(spawnPos);
    }
    
    [ClientRpc]
    public void ResetClientRpc(Vector3 spawnPos)
    {
        transform.position = spawnPos;
        transform.rotation = Quaternion.identity;
        if(IsOwner)
            _collector.Reset();
        _actions.Reset();
    }
}
