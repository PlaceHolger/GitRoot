using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoopidGamePlay : MonoBehaviour
{
    public UnityEvent<GameState> GameStateChanged = new UnityEvent<GameState>();

    public enum GameState
    {
        Join,
        Start,
        Play,
        End
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}