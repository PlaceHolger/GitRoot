using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public float pickupDelay = 0.4f;
    
    public void OnCollect()
    {
        transform.DOScale(Vector3.zero, pickupDelay);
        Destroy(gameObject, pickupDelay + 0.25f);
    }
}
