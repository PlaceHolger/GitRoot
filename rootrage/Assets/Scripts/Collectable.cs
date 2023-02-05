using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour
{
    public float pickupDelay = 0.4f;
    public UnityEvent OnPickupEvent;

    [HideInInspector] public bool picked = false;

    public void OnCollect()
    {
        transform.DOScale(Vector3.zero, pickupDelay).OnComplete(() => gameObject.SetActive(false));
        Destroy(gameObject, pickupDelay + 0.666f);
        picked = true;
        OnPickupEvent.Invoke();
    }
}
