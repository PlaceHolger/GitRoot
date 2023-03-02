using System;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Root : NetworkBehaviour
{
    [SerializeField]
    private int hitPoints = 3;
    [SerializeField]
    private float destroyDelay = 0.2f;

    public UnityEvent EventOnCreate;
    public UnityEvent EventOnHit;
    public UnityEvent EventOnDestroy;

    public override void OnNetworkSpawn()
    {
        EventOnCreate.Invoke();
    }

    public override void OnDestroy()
    {
        if(IsServer && transform.parent.childCount <= 1)  //if all siblings are gone, we destroy also our parent  
            Destroy(transform.parent.gameObject, destroyDelay + 0.2f);
        base.OnDestroy();
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnHitServerRpc(int damage = 1)
    {
        OnHitClientRpc(damage);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DestroyRootServerRpc()
    {
        var siblingCount = transform.parent.childCount;
        Destroy(gameObject, destroyDelay + 1);
        //gameObject.GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void OnHitClientRpc(int damage = 1)
    {
        OnHit(damage);
    }
    
    private async void OnHit(int damage = 1)
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
                    await Task.Delay(75);
                    if (index < siblingRoots.Length - 1)
                    {
                        Root prevSibling = siblingRoots[index + 1];
                        prevSibling.OnHit(10);
                    }
                    if (index > 0)
                    {
                        Root nextSibling = siblingRoots[index - 1];
                        nextSibling.OnHit(10);
                    }
                    break;
                }
            }
            DestroyRootServerRpc();
        }
        else EventOnHit.Invoke();
    }
}
