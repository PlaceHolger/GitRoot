using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

public class RootCreator : NetworkBehaviour
{
    [SerializeField] private GameObject rootPrefab;
    [SerializeField] private GameObject rootRootPrefab;
    [SerializeField] private int delayRootParts = 100;
    [SerializeField] private int maxLength = 16;
    [SerializeField] private float attackCheckSize = 1;
    [SerializeField] private float attackCheckRange = 2;
    [FormerlySerializedAs("layermask")] [SerializeField] private LayerMask obstacleLayermask = 1 << 8 | 1 << 7 | 1 << 6; //by default 'player' and 'obstacle'
    [SerializeField] private LayerMask enableAttackLayerMask = 1 << 9; //by default 'attackable'
    [SerializeField] private Collider ownCollider; //we disable our own collider during firing, so we dont shoot ourself =)
    
    [SerializeField] private UnityEvent EventStartRooting;
    [SerializeField] private UnityEvent EventStopRooting;
    [SerializeField] private UnityEvent EventAttackStart;

    Collider[] collisionResults = new Collider[16];
    private bool isShooting;
    private bool attackAnimReady;

    public bool AttackAnimReady
    {
        get => attackAnimReady;
        set => attackAnimReady = value;
    }

    //Todo: particle in front, mesh in front different from rest...

    public void StopShoot()
    {
        isShooting = false;
    }
    
    //Draw the BoxCast as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
        var hitDetect = IsSomethingInMeleeRange();
        Gizmos.color = (hitDetect) ? Color.red : Color.green;
        //Draw a Ray forward from GameObject toward the maximum distance
        Gizmos.DrawRay(transform.position, transform.forward * attackCheckRange);
        //Draw a cube at the maximum distance
        Gizmos.DrawWireCube(transform.position + (transform.forward * attackCheckRange), Vector3.one * attackCheckSize);
    }

    public bool IsSomethingInMeleeRange()
    {
        var transform1 = transform;
        ownCollider.enabled = false;
        bool result = Physics.CheckBox(transform1.position + transform1.forward, Vector3.one * 0.5f, transform1.rotation, enableAttackLayerMask, QueryTriggerInteraction.Collide);
        ownCollider.enabled = true;
        return result;
    }

    private Transform currentRootParentTransform;
    [ServerRpc]
    private void CreateRootPieceServerRpc(Vector3 currentRootPos, Quaternion startRot, bool createNewRoot)
    {
        if (createNewRoot)
        {
            currentRootParentTransform = Instantiate(rootRootPrefab).transform;
            currentRootParentTransform.GetComponent<NetworkObject>().Spawn();
        }
            
        var rootPiece = Instantiate(rootPrefab, currentRootPos, startRot, currentRootParentTransform).transform;
        var networkObjComp = rootPiece.GetComponent<NetworkObject>();
        networkObjComp.Spawn();
        networkObjComp.TrySetParent(currentRootParentTransform, false);
    }
    
    public async void ShootForward()
    {
        if (isShooting) //dont allow shooting, while shooting, duh
            return;

        var ownTransformComp = transform;
        var startRot = ownTransformComp.rotation;
        var startForward = ownTransformComp.forward;
        var currentRootPos = ownTransformComp.position;
        float startDelay = (float)delayRootParts;
        int currentLength = 0;
        
        ownCollider.enabled = false;
        int hitCount = Physics.OverlapBoxNonAlloc(currentRootPos + startForward, Vector3.one * 0.5f, collisionResults, startRot, enableAttackLayerMask, QueryTriggerInteraction.Collide);
        ownCollider.enabled = true;

        if (hitCount > 0) //something attackable is currently in front of us, instead of shoot, we do an attack
        {
            //Debug.Log("Hack-attack!");
            for (int j = 0; j < hitCount; j++)
            {
                if (collisionResults[j].transform.parent)
                {
                    var rootComp = collisionResults[j].transform.parent.GetComponent<Root>();
                    if(rootComp)
                        rootComp.OnHitServerRpc();                    
                }
                else
                {
                    var playerComp = collisionResults[j].transform.GetComponent<Player>();
                    if(playerComp)
                        playerComp.OnHitServerRpc();
                }
               
            }
            EventAttackStart.Invoke();
            return;
        }
        
        
        isShooting = true;
        while (!attackAnimReady)
        {
            if (!isShooting)
                return;
            await Task.Delay(5);
        }
        
        EventStartRooting.Invoke();
        while (currentLength < maxLength && isShooting)
        {
            currentLength++;
            
            var prevForward = startForward;
            startForward = Vector3.Lerp(startForward, ownTransformComp.forward, 0.075f);
            startForward.Normalize();
            currentRootPos += startForward;
            startRot = Quaternion.Euler(startRot.eulerAngles + new Vector3(0, Vector3.SignedAngle(prevForward, startForward, Vector3.up), 0)); // Quaternion.Euler(Vector3.RotateTowards(prevPos, currentRootPos, 45, 45) + new Vector3(0, 90, 0));  

            ownCollider.enabled = false;
            hitCount = Physics.OverlapBoxNonAlloc(currentRootPos, Vector3.one * 0.45f, collisionResults, startRot, obstacleLayermask, QueryTriggerInteraction.Ignore);
            CreateRootPieceServerRpc(currentRootPos, startRot, currentLength == 1);
            ownCollider.enabled = true;
           
            if (hitCount > 0)
            {
                int notOurselfHitCounter = 0;
                
                //something was hit, end the root here
                for (int j = 0; j < hitCount; j++)
                {
                    //hack-fest: we dont want to collide with ourself
                    if (collisionResults[j].transform.parent && collisionResults[j].transform.parent.parent == currentRootParentTransform)
                        continue;
                    notOurselfHitCounter++;
                    
                    if (collisionResults[j].transform.gameObject.CompareTag("Player"))
                    {
                        var playerComp = collisionResults[j].transform.GetComponent<Player>();
                        if(playerComp)
                            playerComp.OnHitServerRpc();
                        Debug.Log("A Player was hit");
                    }                    
                }
                isShooting = notOurselfHitCounter == 0;
            }
            
            if(isShooting)
            {
                await Task.Delay((int)startDelay);
                //startDelay *= 0.9666f;
            }
        }
        EventStopRooting.Invoke();
    }
}
