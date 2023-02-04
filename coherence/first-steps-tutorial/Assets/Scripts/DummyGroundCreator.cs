using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DummyGroundCreator : MonoBehaviour
{
    [SerializeField] private GameObject GroundPrefab;
    
    [SerializeField] private int Width = 32;
    [SerializeField] private int Height = 32;
    [SerializeField] private float DistanceBetweenSquares = 1;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        CreatePlayfield();
    }

    public void CreatePlayfield()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var ground = Instantiate(GroundPrefab, transform);
                ground.transform.position = new Vector3((transform.position.x + x - (Width * 0.5f)) * DistanceBetweenSquares, 0, (transform.position.z + y - (Height * 0.5f)) * DistanceBetweenSquares);
            }
        }
    }
}

[CustomEditor(typeof(DummyGroundCreator))]   //The script which you want to button to appear in
public class DummyGroundCreatorInspectorScript : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector ();    //This goes first

        var scriptReference = (DummyGroundCreator)target;
        if (GUILayout.Button("Create"))
            scriptReference.CreatePlayfield();
    }
}
