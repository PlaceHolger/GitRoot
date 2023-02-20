using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using UnityEngine.Serialization;

public class AbsoluteGridSensor : GridSensorComponent
{
    private bool _positionSet = false;
    private ArenaManager _arena;
    private Vector3 _arenaPosition;

    void Awake()
    {
        // transform.parent = null;
        _arena = GetComponentInParent<RootAgent>().GetArena;
    }

    void FixedUpdate()
    {
        if (!_positionSet)
        {
            Vector2 center = _arena.GetArenaPosition();
            _arenaPosition = new Vector3(center.x, transform.position.y, center.y);
            _positionSet = true;
        }
        transform.position = _arenaPosition;
    }

    void Update()
    {
        if (!_positionSet)
        {

            Vector2 center = _arena.GetArenaPosition();
            _arenaPosition = new Vector3(center.x, transform.position.y, center.y);
            _positionSet = true;
        }
        transform.position = _arenaPosition;
    }
}
