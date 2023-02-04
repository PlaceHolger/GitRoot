using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

public class RootCreator : MonoBehaviour
{
    [SerializeField] private GameObject rootPrefab;
    [SerializeField] private int delayRootParts = 100;
    [SerializeField] private int maxLength = 16;
    [SerializeField] private float attackCheckSize = 1;
    [SerializeField] private float attackCheckRange = 2;
    [FormerlySerializedAs("layermask")] [SerializeField] private LayerMask obstacleLayermask = 1 << 8 | 1 << 7 | 1 << 6; //by default 'player' and 'obstacle'
    [SerializeField] private LayerMask enableAttackLayerMask = 1 << 9; //by default 'attackable'

    [SerializeField] private UnityEvent EventStartRooting;
    [SerializeField] private UnityEvent EventStopRooting;
    [SerializeField] private UnityEvent EventAttackStart;

    Collider[] collisionResults = new Collider[16];
    private bool isShooting;
    
    //Todo: particle in front, mesh in front different from rest, sound...

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
        return Physics.CheckBox(transform1.position + transform1.forward, Vector3.one * 0.5f, transform1.rotation, enableAttackLayerMask, QueryTriggerInteraction.Collide);
    }
    
    public async void ShootForward()
    {
        if (isShooting) //dont allow shooting, while shooting, duh
            return;
        
        var rootParent = new GameObject("RootShoot");
        var startRot = transform.rotation;
        var startForward = transform.forward;
        var currentRootPos = transform.position;
        float startDelay = (float)delayRootParts;
        int currentLength = 0;
        
        int hitCount = Physics.OverlapBoxNonAlloc(currentRootPos + startForward, Vector3.one * 0.5f, collisionResults, startRot, enableAttackLayerMask, QueryTriggerInteraction.Collide);
        if (hitCount > 0) //something attackable is currently in front of us, instead of shoot, we do an attack
        {
            //Debug.Log("Hack-attack!");
            for (int j = 0; j < hitCount; j++)
            {
                if (collisionResults[j].transform.parent)
                {
                    var rootComp = collisionResults[j].transform.parent.GetComponent<Root>();
                    if(rootComp)
                        rootComp.OnAttack();                    
                }
                else
                {
                    var playerComp = collisionResults[j].transform.GetComponent<Player>();
                    if(playerComp)
                        playerComp.OnHit();
                }
               
            }
            EventAttackStart.Invoke();
            return;
        }
        isShooting = true;
        EventStartRooting.Invoke();
        while (currentLength < maxLength && isShooting)
        {
            currentLength++;
            
            startRot = Quaternion.Lerp(startRot, transform.rotation, 0.05f);
            startForward = Vector3.Lerp(startForward, transform.forward, 0.04f);
            
            currentRootPos += startForward;
            hitCount = Physics.OverlapBoxNonAlloc(currentRootPos, Vector3.one * 0.45f, collisionResults, startRot, obstacleLayermask, QueryTriggerInteraction.Ignore);
            Instantiate(rootPrefab, currentRootPos, startRot, rootParent.transform);
           
            if (hitCount > 0)
            {
                int notOurselfHitCounter = 0;
                
                //something was hit, end the root here
                for (int j = 0; j < hitCount; j++)
                {
                    //hack-fest: we dont want to collide with ourself
                    if (collisionResults[j].transform.parent && collisionResults[j].transform.parent.parent == rootParent.transform)
                        continue;
                    notOurselfHitCounter++;
                    
                    if (collisionResults[j].transform.gameObject.CompareTag("Player"))
                    {
                        var playerComp = collisionResults[j].transform.GetComponent<Player>();
                        if(playerComp)
                            playerComp.OnHit();
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
