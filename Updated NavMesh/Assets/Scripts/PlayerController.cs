using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public Camera cam;

    public NavMeshAgent agent;

    public float maxDistance;

    //public GameObject movementRange;

    public GameObject waypointPrefab;

    GameObject waypoint;

    public float movementSpeed = 4.0f;

    public bool navMesh = true;

    //MeshRenderer rangeRen;

    bool active = true;

    public TurnBasedSystem turn;

    public GameObject turnObj;

    private Rigidbody rb;

    private Vector3 moveInput;
    private Vector3 moveVelocity;

    public NavMeshPath path;

    private Vector3 target;

    private void Start()
    {
        //Change the size of the movementRange based on the maxDistance
        //movementRange.transform.localScale = new Vector3(maxDistance * 2 -0.5f, 0.01f, maxDistance * 2 - 0.5f);

        //Get te renderer for the circular range
        //rangeRen = movementRange.GetComponent<MeshRenderer>();

        //Turn off the NavMesh at the beginning of the game
        ToggleNavMesh(false);

        //Get the TurnBasedSystem script from the turnObj
        turn = turnObj.GetComponent<TurnBasedSystem>();

        //Get the players RigidBody
        rb = GetComponent<Rigidbody>();

        //Initializes the path
        path = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {
        if (active && !navMesh) //if the object is active and navMesh is false calcualte moveVelocity
        {
            //Calculate direction and speed of movement based on axis input
            moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            moveVelocity = moveInput * movementSpeed;
        }
    }

    void FixedUpdate()
    {
        if (active)
        {
            if (navMesh) // if navMesh = true use navMeshMovement
            {
                rb.velocity = Vector3.zero;
                NavMeshMovement();
            }
            else //if navMesh = false use keyboard movement
            {
                //Apply forces to rigidbody
                rb.velocity = moveVelocity;
            }

            //Check if the player is done moving
            if (maxDistance < 0.5f && agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance == 0 && Vector3.Distance(transform.position, agent.destination) <= 1f)
            {
                //Sets the maxDistance back to 10 but doesn't update the scele of the range indicator yet.
                maxDistance = 10.0f;
                //Switch Turns
                turn.SwitchTurn();
            }
        }
    }

    private void NavMeshMovement()
    {
        //Sets the scale of the movement range indicator
        //movementRange.transform.localScale = new Vector3(maxDistance * 2 - 0.5f, 0.01f, maxDistance * 2 - 0.5f);

        if(target != null)
        {
            //Updates the path
            NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            //Draws updated path
            DrawPath(path, 3);
        }

        //Triggers when the player clicks with the left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            //Convertn mousePosition from a screen point to a ray
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            //Defines which layers to ignore with the raycast
            int layerMask = 1 << 10;

            ShootRayAndMove(ray, layerMask);
        }

    }

    private void ShootRayAndMove(Ray ray, int layerMask)
    {
        //Declare a hit variable 
        RaycastHit hit;

        //Check if the point the player clicked is possible to move to
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            //Get the distance of the point from the player
            Vector3 distance = hit.point - transform.position;

            //Sets the target
            target = hit.point;

            //Creates a NavMeshPath
            NavMesh.CalculatePath(transform.position, hit.point, NavMesh.AllAreas, path);

            //If the point clicked is within the max move distance then place a waypoint and move towards it
            if (CalculatePathLength(path) < maxDistance)
            {
                //Draws the path
                DrawPath(path, 1);

                //Check if a waypoint is already placed. If so Destroy it.
                if (waypoint != null)
                {
                    Destroy(waypoint);
                }

                //Place a waypoint
                waypoint = Instantiate(waypointPrefab, hit.point, Quaternion.identity);

                //Move to clicked point
                agent.SetDestination(hit.point);

                //Subtract the distance moved from the maxDistance
                maxDistance -= distance.magnitude;

                //change the size of the movement range circle to coreleate with the max distance
                //movementRange.transform.localScale = new Vector3(maxDistance * 2 - 0.5f, 0.01f, maxDistance * 2 - 0.5f);
            }
            else
            {
                Debug.Log("Out of Range");

                //Draws the path
                DrawPath(path, 2);
            }

            Debug.Log("Path Length: " + CalculatePathLength(path).ToString("n2"));
        }
    }

    //Calculates the length of the navMesh Path
    private float CalculatePathLength(NavMeshPath meshPath)
    {
        //If the path has less than 2 corners return 0
        if(path.corners.Length < 2)
        {
            return 0;
        }

        Vector3 previousCorner = meshPath.corners[0];
        float totalLength = 0.0f;

        //Calculate the length between all the corners and add them to the totalLength
        for (int i = 1; i < meshPath.corners.Length; i++)
        {
            Vector3 currentCorner = meshPath.corners[i];
            totalLength += Vector3.Distance(previousCorner, currentCorner);
            previousCorner = currentCorner;
        }

        return totalLength;
    }

    private void DrawPath(NavMeshPath meshPath, int isGood)
    {
        LineRenderer line = GetComponent<LineRenderer>();

        if (isGood == 1)
        {
            line.material.color = Color.green;
        }
        else if (isGood == 2)
        {
            line.material.color = Color.red;
        }

        line.positionCount = meshPath.corners.Length;

        for(int i = 0; i < meshPath.corners.Length; i++)
        {
            line.SetPosition(i, path.corners[i]);
        }
    }

    private void KeyboardMovement()
    {
        moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        moveVelocity = moveInput * movementSpeed;
    }

    //Triggers when entering a collider
    private void OnTriggerEnter(Collider other)
    {
        //Destroy the other object if its tag is waypoint
        if (other.gameObject.tag == "waypoint")
        {
            Destroy(other.gameObject);
        } //Turn on the navmesh if the tag is navMesh
        else if (other.gameObject.tag == "navMesh")
        {
            ToggleNavMesh(true);
        }
    }

    //Triggers when exiting a collider
    private void OnTriggerExit(Collider other)
    {
        //Turn off navMesh if exiting a collider with a tag of navMesh
        if (other.gameObject.tag == "navMesh")
        {
            ToggleNavMesh(false);
        }
    }

    //Turns on or off the navMesh according to the bool parameter
    private void ToggleNavMesh(bool isOn)
    {
        //if isOn = true then turn on the range circle indicator, the players NavMeshAgent, and sets bool navMesh to true.
        if (isOn)
        {
            //rangeRen.enabled = true;
            agent.enabled = true;
            navMesh = true;
        }
        else //Sets everything stated above to false if isOn = false
        {
            //rangeRen.enabled = false;
            agent.enabled = false;
            navMesh = false;
        }
    }

    //Turns on and off the player according to the bool parameter
    public void TogglePlayer(bool isOn)
    {
        //Sets the player active state to true
        if (isOn)
        {
            active = true;
        }
        else //Sets the player active state to false
        {
            active = false;
        }
    }
}
