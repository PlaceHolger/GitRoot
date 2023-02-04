using System.Threading.Tasks;
using UnityEngine;

public class RootCreator : MonoBehaviour
{
    [SerializeField] private GameObject rootPrefab;
    [SerializeField] private int delayRootParts = 100;
    [SerializeField] private int maxLength = 16;
    [SerializeField] private float attackCheckSize = 1;
    [SerializeField] private float attackCheckRange = 2;
    [SerializeField] private LayerMask layermask = 1 << 8 | 1 << 7 | 1 << 6; //by default 'player' and 'obstacle'
    [SerializeField] private LayerMask enableAttackLayerMask = 1 << 9; //by default 'attackable'

    private bool isShooting;
    
    //Todo: particle in front, mesh in front different from rest, sound...

    public void StopShoot()
    {
        isShooting = false;
    }
    
    // //Draw the BoxCast as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    // void OnDrawGizmos()
    // {
    //     var startRot = transform.rotation;
    //     var startForward = transform.forward;
    //     var currentRootPos = transform.position;
    //     
    //     var hitDetect = Physics.BoxCast(currentRootPos - startForward, Vector3.one * attackCheckSize, startForward, out RaycastHit hit, startRot, attackCheckRange, enableAttackLayerMask, QueryTriggerInteraction.Collide);
    //     if (hitDetect) //something attackable is currently in front of us, instead of shoot, we do an attack
    //     {
    //         Gizmos.color = Color.red;
    //         //Draw a Ray forward from GameObject toward the hit
    //         Gizmos.DrawRay(transform.position, transform.forward * hit.distance);
    //         //Draw a cube that extends to where the hit exists
    //         Gizmos.DrawWireCube(currentRootPos + (startForward * hit.distance), Vector3.one * attackCheckSize);
    //     }
    //     else
    //     {
    //         Gizmos.color = Color.green;
    //         //Draw a Ray forward from GameObject toward the maximum distance
    //         Gizmos.DrawRay(transform.position, transform.forward * attackCheckRange);
    //         //Draw a cube at the maximum distance
    //         Gizmos.DrawWireCube(currentRootPos + (startForward * attackCheckRange), Vector3.one * attackCheckSize);
    //     }
    // }
    
    public async void ShootForward()
    {
        if (isShooting) //dont allow shooting, while shooting, duh
            return;
        
        var rootParent = new GameObject("RootShoot");
        var startRot = transform.rotation;
        var startForward = transform.forward;
        var currentRootPos = transform.position;
        int currentLength = 0;
        
        var hits = Physics.BoxCastAll(currentRootPos - startForward, Vector3.one * attackCheckSize, startForward, startRot, attackCheckRange, enableAttackLayerMask, QueryTriggerInteraction.Collide);
        if (hits.Length > 0) //something attackable is currently in front of us, instead of shoot, we do an attack
        {
            Debug.Log("Hack-attack!");
            foreach (RaycastHit hit in hits)
            {
                var rootComp = hit.transform.parent.GetComponent<Root>();
                if(rootComp)
                    rootComp.OnAttack();
                //TODO: Player attack handling
            }
            return;
        }
        
        isShooting = true;
        
        while (currentLength < maxLength && isShooting)
        {
            currentLength++;
            
            currentRootPos += startForward;
            var hitDetect = Physics.BoxCast(currentRootPos, Vector3.one * 0.5f, startForward, out RaycastHit hit, startRot, 0.5f, layermask);
            Instantiate(rootPrefab, currentRootPos, startRot, rootParent.transform);
            if (hitDetect)
            {
                //something was hit, end the root here
                if (hit.transform.gameObject.CompareTag("Player"))
                {
                    //TODO: inform player about hit
                    Debug.Log("A Player was hit");
                }

                isShooting = false;
                break;
            }
            else
            {
                await Task.Delay(delayRootParts);
            }
        }        
    }
}
