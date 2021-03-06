﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class MagneticFence : MonoBehaviour
{

    //bool gg = false;
    AudioSource Electricity;

    private void Start()
    {
        Electricity = transform.Find("ElectricSound").gameObject.GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.collider.CompareTag("ResearchObject") || collision.collider.gameObject.name == "Marker")
        //{
        //    gg = true;
        Debug.Log("collided");
        if (Game_Manager.instance.LeftGrab.GetGrabbedObject() != null)
        {
            Game_Manager.instance.LeftGrab.ForceRelease();
        }
        else if (Game_Manager.instance.RightGrab.GetGrabbedObject() != null)
        {
            Game_Manager.instance.RightGrab.ForceRelease();
        }
        if (!Electricity.isPlaying)
        {
            Electricity.Play();
        }
    }
}
