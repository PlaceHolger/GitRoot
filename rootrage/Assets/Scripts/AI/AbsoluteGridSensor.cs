using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class AbsoluteGridSensor : GridSensorComponent
{
    private bool _positionSet = false;
    private ArenaManager _arena;
    private Vector3 _arenaPosistion;

    void Awake()
    {
        // transform.parent = null;
        _arena = GameObject.FindObjectsOfType<ArenaManager>()[0];
    }

    void FixedUpdate()
    {
        if (!_positionSet)
        {

            Vector2 center = _arena.GetArenaPosition();
            _arenaPosistion = new Vector3(center.x, transform.position.y, center.y);
            _positionSet = true;
        }
        transform.position = _arenaPosistion;
    }

    void Update()
    {
        if (!_positionSet)
        {

            Vector2 center = _arena.GetArenaPosition();
            _arenaPosistion = new Vector3(center.x, transform.position.y, center.y);
            _positionSet = true;
        }
        transform.position = _arenaPosistion;
    }
}
