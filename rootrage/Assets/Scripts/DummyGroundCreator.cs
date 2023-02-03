using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyGroundCreator : MonoBehaviour
{
    [SerializeField] private GameObject GroundPrefab;
    
    [SerializeField] private int Width = 32;
    [SerializeField] private int Height = 32;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var ground = Instantiate(GroundPrefab, transform);
                ground.transform.position = new Vector3((transform.position.x + x - (Width * 0.5f)) * 1.1f, 0, (transform.position.z + y - (Height * 0.5f)) * 1.1f);
            }            
        }
    }
}
