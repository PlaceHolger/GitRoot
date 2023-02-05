using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Root : MonoBehaviour
{
    [SerializeField]
    private int hitPoints = 3;
    [SerializeField]
    private float destroyDelay = 0.2f;

    public UnityEvent EventOnCreate;
    public UnityEvent EventOnHit;
    public UnityEvent EventOnDestroy;

    public void Awake()
    {
        EventOnCreate.Invoke();
    }

    public async void OnAttack(int damage = 1)
    {
        if (hitPoints < 0)
            return; //Already dead
        
        hitPoints -= damage;
        if (hitPoints <= 0)
        {
            EventOnDestroy.Invoke();
            transform.DOScale(Vector3.zero, destroyDelay).OnComplete(() =>
            {
                if(gameObject) 
                    gameObject.SetActive(false);
            });
            
            //if we die, we also break our siblings
            var siblingRoots = transform.parent.gameObject.GetComponentsInChildren<Root>();
            for (var index = 0; index < siblingRoots.Length; index++)
            {
                Root sibling = siblingRoots[index];
                if (sibling == this)
                {
                    await Task.Delay(100);
                    if (index < siblingRoots.Length - 1)
                    {
                        Root prevSibling = siblingRoots[index + 1];
                        prevSibling.OnAttack(10);
                    }
                    if (index > 0)
                    {
                        Root nextSibling = siblingRoots[index - 1];
                        nextSibling.OnAttack(10);
                    }
                    break;
                }
            }

            Destroy(gameObject, destroyDelay + 1);
        }
        else EventOnHit.Invoke();
    }
}
