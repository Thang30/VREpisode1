﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoltenTrigger : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collided");
        if (other.tag == "JanitorBroom")
        {
            Debug.Log("collidedJanitor");
            JanitorBroomTransformer.ChangeBroomColour = true;
        }
    }
}