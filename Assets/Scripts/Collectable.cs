using System.Threading.Tasks;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : NetworkBehaviour
{
    public float pickupDelay = 0.4f;
    public UnityEvent OnPickupEvent;

    [HideInInspector] public bool picked = false;

    [ClientRpc]
    public void OnCollectClientRpc()
    {
        GetComponent<Collider>().enabled = false;
        transform.DOScale(Vector3.zero, pickupDelay);
        DestroyServerRpc();
        picked = true;
        OnPickupEvent.Invoke();
    }

    [ClientRpc]
    public void MarkAsDroppedClientRpc()
    {
        var lightComp = GetComponentInChildren<Light>();
        if (lightComp)
            lightComp.color = Color.red;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        picked = true;
        Destroy(gameObject, pickupDelay + 0.666f);
    }
}
