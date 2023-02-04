using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Root : MonoBehaviour
{
    private int hitPoints = 3;
    private float destroyDelay = 0.1f;

    public UnityEvent EventOnHit;
    public UnityEvent EventOnDestroy;
    
    public void OnAttack()
    {
        if (hitPoints-- <= 0)
        {
            EventOnDestroy.Invoke();
            Destroy(gameObject, destroyDelay);
        }
        else EventOnHit.Invoke();
    }
}
