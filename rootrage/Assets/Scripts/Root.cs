using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Root : MonoBehaviour
{
    private int hitPoints = 3;
    private float destroyDelay = 0.2f;

    public UnityEvent EventOnHit;
    public UnityEvent EventOnDestroy;
    
    public void OnAttack()
    {
        if (hitPoints-- <= 0)
        {
            EventOnDestroy.Invoke();
            transform.DOScale(Vector3.zero, destroyDelay);
            Destroy(gameObject, destroyDelay + 0.25f);
        }
        else EventOnHit.Invoke();
    }
}
