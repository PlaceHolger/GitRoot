using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public float pickupDelay = 0.4f;

    [HideInInspector] public bool picked = false;

    public void OnCollect()
    {
        transform.DOScale(Vector3.zero, pickupDelay).OnComplete(() => gameObject.SetActive(false));
        Destroy(gameObject, pickupDelay + 0.666f);
        picked = true;
    }
}
