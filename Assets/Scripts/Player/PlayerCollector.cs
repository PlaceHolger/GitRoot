using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerCollector : NetworkBehaviour
{
    //private int currentScore = 0;
    public LayerMask collectablesMask = 1 << 10;
    public float collectRadius = 1;
    Collider[] collisionResults = new Collider[16];
    private NetworkVariable<int> m_CurrentScore = new();

    public int CurrentScore => m_CurrentScore.Value;

    //set => m_CurrentScore.Value = value;
    public override void OnNetworkSpawn()
     {
         base.OnNetworkSpawn();
         if (!IsServer)
             enabled = false;
     }

    private void FixedUpdate()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, collectRadius, collisionResults, collectablesMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hitCount; i++)
        {
            var collectableComp = collisionResults[i].transform.GetComponent<Collectable>();
            if (collectableComp && !collectableComp.picked)
            {
                collectableComp.OnCollectClientRpc();
                SetScoreServerRpc(CurrentScore + 1);
            }            
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetScoreServerRpc(int i)
    {
        m_CurrentScore.Value = i;
    }

    public void Reset()
    {
        SetScoreServerRpc(0);
    }
}
