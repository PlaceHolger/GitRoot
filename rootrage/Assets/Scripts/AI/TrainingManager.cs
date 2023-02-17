using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : GameManager
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        Initialize();
        base.Update();
    }
}
