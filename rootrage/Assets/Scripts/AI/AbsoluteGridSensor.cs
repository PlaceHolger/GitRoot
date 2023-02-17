using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class AbsoluteGridSensor : GridSensorComponent
{
    private ArenaManager _arena;
    // Start is called before the first frame update
    void Awake()
    {
        _arena = GameObject.FindObjectsOfType<ArenaManager>()[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 center = _arena.GetArenaPosition();
        transform.position = new Vector3(center.x, transform.position.y, center.y);
    }
}
