using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class AbsoluteGridSensor : GridSensorComponent
{
    private TrainingArena _arena;
    // Start is called before the first frame update
    void Awake()
    {
        _arena = GameObject.FindObjectsOfType<TrainingArena>()[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = _arena.transform.position;
    }
}
