﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalHitsTheFan : MonoBehaviour {
    AudioSource MetalSounds1;
    AudioSource MetalSounds2;
    AudioSource MetalSounds3;
    AudioSource MetalSounds4;
    AudioSource MetalSounds5;
    AudioSource MetalSounds6;
    int randomizer;
    bool insideTheMelter;

    private void Awake()
    {
        MetalSounds1 = GameObject.Find("MetalSounds1").GetComponent<AudioSource>();
        MetalSounds2 = GameObject.Find("MetalSounds2").GetComponent<AudioSource>();
        MetalSounds3 = GameObject.Find("MetalSounds3").GetComponent<AudioSource>();
        MetalSounds4 = GameObject.Find("MetalSounds4").GetComponent<AudioSource>();
        MetalSounds5 = GameObject.Find("MetalSounds5").GetComponent<AudioSource>();
        MetalSounds6 = GameObject.Find("MetalSounds6").GetComponent<AudioSource>();
        randomizer = 0;
        insideTheMelter = false;
    }

    public bool InsideTheMelter
    {
        get { return insideTheMelter; }
        set { insideTheMelter = value; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ConveyorPart")
        {
            if (randomizer == 1)
            {
                MetalSounds1.Play();
            }
            if (randomizer == 2)
            {
                MetalSounds2.Play();
            }
            if (randomizer == 3)
            {
                MetalSounds3.Play();
            }
            if (randomizer == 4)
            {
                MetalSounds4.Play();
            }
            if (randomizer == 5)
            {
                MetalSounds5.Play();
            }
            if (randomizer == 6)
            {
                MetalSounds6.Play();
            }
        }
    }
   
    private void Update()
    {
        var newScale = Mathf.Lerp(1, 0.5f, Time.deltaTime / 10);
        randomizer = Random.Range(1,7);      //so between 1-6

        if (insideTheMelter)
        {
            //scale this specific piece of scrap metal slowly down and at the same time another animation rises the lava
            
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }
}
