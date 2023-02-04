using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    private int currentScore;
    public LayerMask collectablesMask = 1 << 10;
    public SphereCollider checkCollider;
    private void FixedUpdate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkCollider.radius, collectablesMask, QueryTriggerInteraction.Collide);
        foreach (var hitCollectable in hitColliders)
        {
            var collectableComp = hitCollectable.transform.GetComponent<Collectable>();
            if (collectableComp)
            {
                collectableComp.OnCollect();
                currentScore++;
                //TODO: Update UI and stuff...
            }
        }
    }
}