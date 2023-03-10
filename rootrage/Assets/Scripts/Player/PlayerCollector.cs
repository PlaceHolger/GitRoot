using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    private int currentScore = 0;
    public LayerMask collectablesMask = 1 << 10;
    public float collectRadius = 1;

    public int CurrentScore
    {
        get { return currentScore; }
        set { currentScore = value; }
    }

    private void FixedUpdate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, collectRadius, collectablesMask, QueryTriggerInteraction.Collide);
        foreach (var hitCollectable in hitColliders)
        {
            var collectableComp = hitCollectable.transform.GetComponent<Collectable>();
            if (collectableComp && !collectableComp.picked)
            {
                collectableComp.OnCollect();
                currentScore++;
                //TODO: Update UI and stuff...
            }
        }
    }

    public void Reset()
    {
        currentScore = 0;
    }
}
