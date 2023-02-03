using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class RootCreator : MonoBehaviour
{
    [SerializeField] private GameObject rootPrefab;
    [SerializeField] private int delayRootParts = 200;

    public async void ShootRight()
    {
        var currentRootPos = new Vector3(Random.Range(-16, 16), transform.position.y, Random.Range(-16, 16));
        while (currentRootPos.x < 16)
        {
            Instantiate(rootPrefab, currentRootPos, Quaternion.Euler(0, 0, 0), transform);
            await Task.Delay(delayRootParts);
            currentRootPos += Vector3.right;
        }
    }
    
    public async void ShootUp()
    {
        var currentRootPos = new Vector3(Random.Range(-16, 16), transform.position.y, Random.Range(-16, 16));
        while (currentRootPos.z < 16)
        {
            Instantiate(rootPrefab, currentRootPos, Quaternion.Euler(0, 90, 0), transform);
            await Task.Delay(delayRootParts);
            currentRootPos += new Vector3(0, 0, 1);
        }
    }
    
    public async void ShootLeft()
    {
        var currentRootPos = new Vector3(Random.Range(-16, 16), transform.position.y, Random.Range(-16, 16));
        while (currentRootPos.x > -16)
        {
            Instantiate(rootPrefab, currentRootPos, Quaternion.Euler(0, 0, 0), transform);
            await Task.Delay(delayRootParts);
            currentRootPos += Vector3.left;
        }
    }
    
    public async void ShootDown()
    {
        var currentRootPos = new Vector3(Random.Range(-16, 16), transform.position.y, Random.Range(-16, 16));
        while (currentRootPos.z > -16)
        {
            Instantiate(rootPrefab, currentRootPos, Quaternion.Euler(0, 90, 0), transform);
            await Task.Delay(delayRootParts);
            currentRootPos -= new Vector3(0, 0, 1);
        }
    }
}

[CustomEditor(typeof(RootCreator))]   //The script which you want to button to appear in
public class CustomInspectorScript : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector ();    //This goes first

        var scriptReference = (RootCreator)target;
        if (GUILayout.Button ("ShootRight")) 
            scriptReference.ShootRight();
        if (GUILayout.Button ("ShootUp"))    
            scriptReference.ShootUp();
        if (GUILayout.Button ("ShootLeft"))  
            scriptReference.ShootLeft();
        if (GUILayout.Button ("ShootDown"))  
            scriptReference.ShootDown();
    }
}
