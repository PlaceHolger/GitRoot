using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;
using Random = UnityEngine.Random;

public class RootCreator : MonoBehaviour
{
    [SerializeField] private GameObject rootPrefab;
    [SerializeField] private int delayRootParts = 200;

    public async void ShootForward()
    {
        var rootParent = new GameObject();
        var startRot = transform.rotation;
        var startForward = transform.forward;
        
        var currentRootPos = transform.position + startForward;
        while (currentRootPos.x < 16 && currentRootPos.x > -16 && currentRootPos.z < 16 && currentRootPos.z > -16)
        {
            Instantiate(rootPrefab, currentRootPos, startRot, rootParent.transform);
            await Task.Delay(delayRootParts);
            currentRootPos += startForward;
        }        
    }
}
