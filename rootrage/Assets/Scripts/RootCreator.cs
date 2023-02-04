using System.Threading.Tasks;
using UnityEngine;

public class RootCreator : MonoBehaviour
{
    [SerializeField] private GameObject rootPrefab;
    [SerializeField] private int delayRootParts = 100;
    [SerializeField] private int maxLength = 16;
    [SerializeField] private LayerMask layermask = 1 << 8 | 1 << 7 | 1 << 6; //by default 'player' and 'obstacle'

    private bool isShooting;
    
    //Todo: particle in front, mesh in front different from rest, sound...

    public void StopShoot()
    {
        isShooting = false;
    }
    
    public async void ShootForward()
    {
        if (isShooting) //dont allow shooting, while shooting, duh
            return;
        
        var rootParent = new GameObject("RootShoot");
        var startRot = transform.rotation;
        var startForward = transform.forward;
        var currentRootPos = transform.position + startForward;
        int currentLength = 0;
        isShooting = true;
        
        while (currentLength < maxLength && isShooting)
        {
            currentLength++;
                
            var hitDetect = Physics.BoxCast(currentRootPos, Vector3.one * 0.5f, startForward, out RaycastHit hit, startRot, 1, layermask);
            Instantiate(rootPrefab, currentRootPos, startRot, rootParent.transform);
            currentRootPos += startForward;
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
