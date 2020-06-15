using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using TMPro;
using VRTK.GrabAttachMechanics;

public class Game_Manager : MonoBehaviour
{
    //just gonna collect loads of static variables here pm
    [Header("Events")]

    //[Tooltip("an event which takes care of water movement starting")]
    //public UnityEngine.Events.UnityEvent WaterComes;

    public VRTK_ControllerEvents leftController;

    public VRTK_ControllerEvents rightController;

    [Header("Booleans")]

    [SerializeField]
    [Tooltip("checks if the touchpad is pressed for locomotion")]
    private bool locomotionOn;

    [SerializeField]
    [Tooltip("checks if the octomarker is being unsnapped from a snapzone")]
    public bool beingUnSnapped;

    [SerializeField]
    [Tooltip("checks if broom is ready to start the door cracking animation")]
    private bool playBroomAnimation;

    [SerializeField]
    [Tooltip("checks if the water has started moving or not")]
    private bool invoked;

    [SerializeField]
    [Tooltip("checks if the lantern is grabbed by the player or not")]
    private bool lanternIsGrabbedFirstTime;

    [SerializeField]
    [Tooltip("checks if the lantern light is on or not")]
    private bool lanternLightIsOn;

    [SerializeField]
    [Tooltip("Toggles between climbable rope and grabbable rope")]
    private bool ropeClimb;

    [Header("Integers")]
    [SerializeField]
    private int numberOfTheBroom;

    [SerializeField]
    [Tooltip("Toggles between climbable rope and grabbable rope")]
    private int elevatorMoving;

    private bool objectNotGrabbedYetRight;

    private bool objectNotGrabbedYetLeft;

    private bool playerPositionChanged;

    private float playerPositionChangeAmount;

    private Vector3 currentPlayerPosition;

    private Vector3 currentGrabbedObjectLocalPosition;

    AudioSource LeftWaterPush;
    AudioSource RightWaterPush;

    [SerializeField]
    private WaterMovement water;

    [Header("Gameobjects")]


    public GameObject Lantern;

    public GameObject GrabbableWater;

    public GameObject JuhaniHead;

    public GameObject JuhaniBody;

    public GameObject JuhaniHand1;

    public GameObject JuhaniHand2;

    public GameObject JuhaniLeg1;

    public GameObject JuhaniLeg2;

    public GameObject Noose;

    public GameObject RightController;

    public GameObject LeftController;

    public GameObject RightHand;

    public GameObject LeftHand;

    public GameObject previousLeftGrabbedObject;

    public GameObject previousRightGrabbedObject;

    [Header("Other")]

    public ParticleSystem WaterBubbles;

    public VRTK_InteractGrab RightGrab;

    public VRTK_InteractGrab LeftGrab;

    public static Game_Manager instance = null;

    [Header("Audio")]

    public AudioSource RopeCreak;

    public AudioSource WaterBreakingSound;

    bool notIgnoredYet;

    public Transform GrabAttachPointRight;

    public Transform GrabAttachPointLeft;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this);

        //WaterComes.AddListener(WaterIsRising);

        if (GameObject.Find("Water") != null)
        {
            WaterBubbles = GameObject.Find("Water").GetComponentInChildren<ParticleSystem>();

            WaterBubbles.Pause();

            LeftWaterPush = GameObject.Find("LeftWaterPush").GetComponent<AudioSource>();

            RightWaterPush = GameObject.Find("RightWaterPush").GetComponent<AudioSource>();

            Lantern = GameObject.Find("Lantern");

            GrabbableWater = GameObject.Find("GrabbableWater");

            water = GameObject.Find("Water").GetComponent<WaterMovement>();
        }

        ropeClimb = true;

        playBroomAnimation = false;

        lanternIsGrabbedFirstTime = false;

        lanternLightIsOn = false;

        beingUnSnapped = false;

        invoked = true;

        elevatorMoving = 0;

        numberOfTheBroom = 0;

        playerPositionChanged = false;

        playerPositionChangeAmount = 0f;

        locomotionOn = false;

        objectNotGrabbedYetRight = true;

        objectNotGrabbedYetLeft = true;

        currentGrabbedObjectLocalPosition = new Vector3(0, 0, 0);

        if (GameObject.Find("JuhaniBody") != null)
        {
            JuhaniBody = GameObject.Find("JuhaniBody");

            JuhaniHead = GameObject.Find("JuhaniHead");

            JuhaniHand1 = GameObject.Find("JuhaniHand1");

            JuhaniHand2 = GameObject.Find("JuhaniHand2");

            JuhaniLeg1 = GameObject.Find("JuhaniLeg1");

            JuhaniLeg2 = GameObject.Find("JuhaniLeg2");
        }

        Noose = GameObject.Find("NOOSE");

        if (Noose != null)
        {
            RopeCreak = Noose.GetComponent<AudioSource>();
        }

        WaterBreakingSound = GetComponent<AudioSource>();

        RightController = GameObject.Find("RightController");

        LeftController = GameObject.Find("LeftController");

        RightHand = RightController.transform.GetChild(0).gameObject;

        LeftHand = LeftController.transform.GetChild(0).gameObject;

        RightGrab = RightController.GetComponent<VRTK_InteractGrab>();

        LeftGrab = LeftController.GetComponent<VRTK_InteractGrab>();

        notIgnoredYet = true;

        GrabAttachPointLeft = LeftGrab.transform.GetChild(0).GetChild(1).transform;

        GrabAttachPointRight = RightGrab.transform.GetChild(0).GetChild(1).transform;
    }


    /// <summary>
    /// /ENABLED FUNCTIONALITY OF EVENTS
    /// </summary>
    protected void OnEnable()
    {
        if (LeftController != null)
        {
            leftController.TouchpadPressed += LocomotionOn;
            leftController.TouchpadReleased += LocomotionOff;
        }

        if (RightController != null)
        {
            rightController.TouchpadPressed += LocomotionOn;
            rightController.TouchpadReleased += LocomotionOff;
        }
        if (LeftGrab != null)
        {
            LeftGrab.ControllerGrabInteractableObject += RegisterGrabbedObjectLocalPositionWhenGrabbingLeft;
            LeftGrab.ControllerUngrabInteractableObject += RegisterObjectDropLeft;
        }
        if (RightGrab != null)
        {
            RightGrab.ControllerGrabInteractableObject += RegisterGrabbedObjectLocalPositionWhenGrabbingRight;
            RightGrab.ControllerUngrabInteractableObject += RegisterObjectDropRight;
        }
    }
    //OTHER METHODS THAN GETTERS AND SETTERS OR ANIMATION STARTERS HERE!
    private void FixedUpdate()
    {
        CheckGrabbedObjects();

        if (locomotionOn)
        {
            Debug.Log("resettingLocalpos");
            CheckGrabbedObjectLocalPositionStays();
        }

        if (Time.time >= 0.75f && notIgnoredYet)
        {
            notIgnoredYet = false;
            IgnoreObjectCollisionsWithPlayer();
        }
    }
    private void IgnoreObjectCollisionsWithPlayer()
    {
        foreach (VRTK_InteractableObject inter in FindObjectsOfType<VRTK_InteractableObject>())
        {
            if (inter.GetComponent<Collider>() != null && WaterMovement.feet != null && WaterMovement.body != null && WaterMovement.head != null)
            {
                Physics.IgnoreCollision(WaterMovement.feet, inter.GetComponent<Collider>());
                Physics.IgnoreCollision(WaterMovement.body, inter.GetComponent<Collider>());
                Physics.IgnoreCollision(WaterMovement.head, inter.GetComponent<Collider>());
            }
            if (inter.GetComponentsInChildren<Collider>() != null && WaterMovement.feet != null && WaterMovement.body != null && WaterMovement.head != null)
            {
                foreach (Collider col in inter.GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(WaterMovement.feet, col);                    //testing reasons
                    Physics.IgnoreCollision(WaterMovement.body, col);
                    Physics.IgnoreCollision(WaterMovement.head, col);
                    //Debug.Log(col.gameObject.name);
                }
            }
        }
    }

    private void WaterIsRising()
    {
        water.WaterRises = true;
    }
    public void CheckGrabbedObjects()
    {
        //RIGHT HAND GRABBED OBJECTS
        if (RightGrab.GetGrabbedObject() != null)
        {
            previousRightGrabbedObject = RightGrab.GetGrabbedObject();
            if (RightGrab.GetGrabbedObject() == Lantern)
            {
                lanternIsGrabbedFirstTime = true;
                if (invoked)
                {
                    WaterBreakingSound.Play();
                    WaterBubbles.Play();
                    water.UnderwaterAmbience1.Play();
                    water.UnderwaterAmbience2.Play();
                    water.UnderwaterAmbience3.Play();
                    water.UnderwaterAmbience4.Play();
                    water.WaterRises = true;
                    invoked = false;
                    ResetOutOfFacilityObjectLocation.PlayerResetLocation = "SecondShaft";
                }
            }
            else if (RightGrab.GetGrabbedObject() == JuhaniBody && JuhaniHead.GetComponent<ConfigurableJoint>() != null)
            {
                RightGrab.ForceRelease();
                ResetOutOfFacilityObjectLocation.PlayerResetLocation = "FirstShaft";
                foreach (ConfigurableJoint juhaniJoin in JuhaniHead.GetComponents<ConfigurableJoint>())
                {
                    Destroy(juhaniJoin);
                }
                foreach (ConfigurableJoint headNoose in Noose.GetComponents<ConfigurableJoint>())
                {
                    if (headNoose.connectedBody.name == "JuhaniHead" || headNoose.connectedBody.name == "Bone_chest")
                    {
                        Destroy(headNoose);
                    }
                }
                RopeClimb = false;
                RopeCreak.Stop();
                JuhaniBody.GetComponent<Rigidbody>().mass = 1000;
                JuhaniBody.GetComponent<Rigidbody>().drag = 1.5f;
                JuhaniBody.GetComponent<Rigidbody>().angularDrag = 1.5f;
                foreach (Rigidbody Juhanirigidpart in JuhaniBody.GetComponentsInChildren<Rigidbody>())
                {
                    Juhanirigidpart.mass = 1000;
                    Juhanirigidpart.drag = 1.5f;
                    Juhanirigidpart.angularDrag = 1.5f;
                }
            }
            else if (RightGrab.GetGrabbedObject() == GrabbableWater)
            {
                if (!RightWaterPush.isPlaying)
                {
                    RightWaterPush.Play();
                }
                StartCoroutine(WaitForSecondsRealtimeRight());
            }
            else if (RightGrab.GetGrabbedObject().CompareTag("FloatingObject"))
            {
                //the floating object goes down but less if other hand was already grabbing it
                if (previousLeftGrabbedObject != RightGrab.GetGrabbedObject())
                {
                    if (RightGrab.GetGrabbedObject().GetComponent<Rigidbody>() != null)
                    {
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().AddForce(Vector3.down * 3.5f, ForceMode.Impulse);
                    }
                    else if (RightGrab.GetGrabbedObject().transform.parent.GetComponent<Rigidbody>() != null)
                    {
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        RightGrab.GetGrabbedObject().transform.parent.GetComponent<Rigidbody>().AddForce(Vector3.down * 3.5f, ForceMode.Impulse);
                    }
                    else if (RightGrab.GetGrabbedObject().transform.parent.parent.GetComponent<Rigidbody>() != null)
                    {
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        RightGrab.GetGrabbedObject().transform.parent.parent.GetComponent<Rigidbody>().AddForce(Vector3.down * 3.5f, ForceMode.Impulse);
                    }
                }
                else
                {
                    if (RightGrab.GetGrabbedObject().GetComponent<Rigidbody>() != null)
                    {
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().AddForce(Vector3.down * 1.5f, ForceMode.Impulse);
                    }
                    else if (RightGrab.GetGrabbedObject().transform.parent.GetComponent<Rigidbody>() != null)
                    {
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        RightGrab.GetGrabbedObject().transform.parent.GetComponent<Rigidbody>().AddForce(Vector3.down * 1.5f, ForceMode.Impulse);
                    }
                    else if (RightGrab.GetGrabbedObject().transform.parent.parent.GetComponent<Rigidbody>() != null)
                    {
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //RightGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        RightGrab.GetGrabbedObject().transform.parent.parent.GetComponent<Rigidbody>().AddForce(Vector3.down * 1.5f, ForceMode.Impulse);
                    }
                }
            }
        }
        //LEFT HAND GRABBED OBJECTS
        if (LeftGrab.GetGrabbedObject() != null)
        {
            previousLeftGrabbedObject = LeftGrab.GetGrabbedObject();
            if (LeftGrab.GetGrabbedObject() == Lantern)
            {
                lanternIsGrabbedFirstTime = true;
                if (invoked)
                {
                    WaterBreakingSound.Play();
                    WaterBubbles.Play();
                    water.UnderwaterAmbience1.Play();
                    water.UnderwaterAmbience2.Play();
                    water.UnderwaterAmbience3.Play();
                    water.UnderwaterAmbience4.Play();
                    water.WaterRises = true;
                    invoked = false;
                    ResetOutOfFacilityObjectLocation.PlayerResetLocation = "SecondShaft";
                }
            }
            else if (LeftGrab.GetGrabbedObject() == JuhaniBody && JuhaniHead.GetComponent<ConfigurableJoint>() != null)
            {
                LeftGrab.ForceRelease();
                ResetOutOfFacilityObjectLocation.PlayerResetLocation = "FirstShaft";
                foreach (ConfigurableJoint juhaniJoin in JuhaniHead.GetComponents<ConfigurableJoint>())
                {
                    Destroy(juhaniJoin);
                }
                foreach (ConfigurableJoint headNoose in Noose.GetComponents<ConfigurableJoint>())
                {
                    if (headNoose.connectedBody.name == "JuhaniHead" || headNoose.connectedBody.name == "Bone_chest")
                    {
                        Destroy(headNoose);
                    }
                }
                RopeClimb = false;
                RopeCreak.Stop();
                JuhaniBody.GetComponent<Rigidbody>().mass = 1000;
                JuhaniBody.GetComponent<Rigidbody>().drag = 1.5f;
                JuhaniBody.GetComponent<Rigidbody>().angularDrag = 1.5f;
                foreach (Rigidbody Juhanirigidpart in JuhaniBody.GetComponentsInChildren<Rigidbody>())
                {
                    Juhanirigidpart.mass = 1000;
                    Juhanirigidpart.drag = 1.5f;
                    Juhanirigidpart.angularDrag = 1.5f;
                }
            }
            else if (LeftGrab.GetGrabbedObject() == GrabbableWater)
            {
                if (!LeftWaterPush.isPlaying)
                {
                    LeftWaterPush.Play();
                }
                StartCoroutine(WaitForSecondsRealtimeLeft());
            }
            else if (LeftGrab.GetGrabbedObject().CompareTag("FloatingObject"))
            {
                if (previousRightGrabbedObject != LeftGrab.GetGrabbedObject())
                {
                    if (LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>() != null)
                    {
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().AddForce(Vector3.down * 3.5f, ForceMode.Impulse);
                    }
                    else if (LeftGrab.GetGrabbedObject().transform.parent.GetComponent<Rigidbody>() != null)
                    {
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        LeftGrab.GetGrabbedObject().transform.parent.GetComponent<Rigidbody>().AddForce(Vector3.down * 3.5f, ForceMode.Impulse);
                    }
                    else if (LeftGrab.GetGrabbedObject().transform.parent.parent.GetComponent<Rigidbody>() != null)
                    {
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        LeftGrab.GetGrabbedObject().transform.parent.parent.GetComponent<Rigidbody>().AddForce(Vector3.down * 3.5f, ForceMode.Impulse);
                    }
                }
                else
                {
                    if (LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>() != null)
                    {
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().AddForce(Vector3.down * 1.5f, ForceMode.Impulse);
                    }
                    else if (LeftGrab.GetGrabbedObject().transform.parent.GetComponent<Rigidbody>() != null)
                    {
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        LeftGrab.GetGrabbedObject().transform.parent.GetComponent<Rigidbody>().AddForce(Vector3.down * 1.5f, ForceMode.Impulse);
                    }
                    else if (LeftGrab.GetGrabbedObject().transform.parent.parent.GetComponent<Rigidbody>() != null)
                    {
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;
                        //LeftGrab.GetGrabbedObject().GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
                        LeftGrab.GetGrabbedObject().transform.parent.parent.GetComponent<Rigidbody>().AddForce(Vector3.down * 1.5f, ForceMode.Impulse);
                    }
                }
            }
        }         
    }
    
    IEnumerator WaitForSecondsRealtimeRight()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        StartCoroutine(ReleaseOrNotRight());
    }
    IEnumerator WaitForSecondsRealtimeLeft()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        StartCoroutine(ReleaseOrNotLeft());
    }
    IEnumerator ReleaseOrNotRight()
    {
        if (RightGrab.GetGrabbedObject() != null && RightGrab.GetGrabbedObject() == GrabbableWater)
        {
            Debug.Log("ReleasedWater");
            RightGrab.ForceRelease();          
            ToxicGasPush.PlayerBody.AddForce(Vector3.down * 7.5f, ForceMode.Impulse);
        }
        else
        {
            ToxicGasPush.PlayerBody.AddForce(Vector3.down * 7.5f, ForceMode.Impulse);
            yield return null;
        }
    }
    IEnumerator ReleaseOrNotLeft()
    {
        if (LeftGrab.GetGrabbedObject() != null && LeftGrab.GetGrabbedObject() == GrabbableWater)
        {
            ToxicGasPush.PlayerBody.AddForce(Vector3.down * 7.5f, ForceMode.Impulse);
            LeftGrab.ForceRelease();           
        }
        else
        {
            ToxicGasPush.PlayerBody.AddForce(Vector3.down * 7.5f, ForceMode.Impulse);
            yield return null;
        }      
    }

    public void CheckGrabbedObjectLocalPositionStays()
    {
        if (RightGrab.GetGrabbedObject() != null && RightGrab.GetGrabbedObject().GetComponent<VRTK_InteractableObject>().grabAttachMechanicScript == RightGrab.GetGrabbedObject().GetComponent<VRTK_ChildOfControllerGrabAttach>())
        {
            if (currentGrabbedObjectLocalPosition != GrabAttachPointRight.localPosition)
            {               
                GrabAttachPointRight.localPosition = currentGrabbedObjectLocalPosition;
            }
        }
        else if (LeftGrab.GetGrabbedObject() != null && LeftGrab.GetGrabbedObject().GetComponent<VRTK_InteractableObject>().grabAttachMechanicScript == LeftGrab.GetGrabbedObject().GetComponent<VRTK_ChildOfControllerGrabAttach>())
        {
            if (currentGrabbedObjectLocalPosition != GrabAttachPointLeft.localPosition)
            {
                GrabAttachPointLeft.localPosition = currentGrabbedObjectLocalPosition;
            }
        }
    }

    //get grabbed object position
    protected virtual void RegisterGrabbedObjectLocalPositionWhenGrabbingRight(object sender, ObjectInteractEventArgs e)
    {
        if (objectNotGrabbedYetRight)
        {
            currentGrabbedObjectLocalPosition = GrabAttachPointRight.localPosition;
            //Debug.Log(GrabAttachPointRight.localPosition);
            objectNotGrabbedYetRight = false;

            //currentGrabbedObjectLocalPosition = RightGrab.GetGrabbedObject().transform.
            //    Find("[VRTK][AUTOGEN][RightController][Original][Controller][AttachPoint]").transform.position;
        }
    }

    protected virtual void RegisterGrabbedObjectLocalPositionWhenGrabbingLeft(object sender, ObjectInteractEventArgs e)
    {
        if (objectNotGrabbedYetLeft)
        {
            currentGrabbedObjectLocalPosition = GrabAttachPointLeft.localPosition;
            //Debug.Log(GrabAttachPointLeft.localPosition);
            objectNotGrabbedYetLeft = false;

            //currentGrabbedObjectLocalPosition = LeftGrab.GetGrabbedObject().transform.
            //    Find("[VRTK][AUTOGEN][LeftController][Original][Controller][AttachPoint]").transform.position;
        }
    }

    protected virtual void RegisterObjectDropRight(object sender, ObjectInteractEventArgs e)
    {
        objectNotGrabbedYetRight = true;
    }

    protected virtual void RegisterObjectDropLeft(object sender, ObjectInteractEventArgs e)
    {
        objectNotGrabbedYetLeft = true;
    }

    public void LocomotionOn(object sender, ControllerInteractionEventArgs e)
    {
        locomotionOn = true;
    }

    protected virtual void LocomotionOff(object sender, ControllerInteractionEventArgs e)
    {
        locomotionOn = false;
    }

    //GETTERS AND SETTERS PART BELOW HERE!

    public int ElevatorMoving
    {
        get { return elevatorMoving; }

        set { elevatorMoving = value; }
    }

    public bool RopeClimb
    {
        get { return ropeClimb; }

        set { ropeClimb = value; }
    }

    public bool IsLanternGrabbed
    {
        get { return lanternIsGrabbedFirstTime; }

        set { lanternIsGrabbedFirstTime = value; }
    }

    public bool LanternLightIsOn
    {
        get { return lanternLightIsOn; }

        set { LanternLightIsOn = value; }
    }

    public bool Invoked
    {
        get { return invoked; }

        set { invoked = value; }
    }
}


