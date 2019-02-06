using UnityEngine;
using UnityEngine.AI;

public class Temp2 : MonoBehaviour
{

    public Camera cam;

    public NavMeshAgent agent;

    public float maxDistance;

    public GameObject movementRange;

    public GameObject waypointPrefab;

    GameObject waypoint;

    public float movementSpeed = 4.0f;

    public bool navMesh = true;

    MeshRenderer rangeRen;

    bool active = true;

    public TurnBasedSystem turn;

    public GameObject turnObj;

    private void Start()
    {
        //Change the size of the movementRange based on the maxDistance
        movementRange.transform.localScale = new Vector3(maxDistance * 2, 0.01f, maxDistance * 2);

        //Get te renderer for the circular range
        rangeRen = movementRange.GetComponent<MeshRenderer>();

        //Turn off the NavMesh at the beginning of the game
        ToggleNavMesh(false);

        //Get the TurnBasedSystem script from the turnObj
        turn = turnObj.GetComponent<TurnBasedSystem>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (active)
        {
            if (navMesh)
            {
                movementRange.transform.localScale = new Vector3(maxDistance * 2, 0.01f, maxDistance * 2);

                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    int layerMask = 1 << 10;

                    //Check if the point the player clicked is possible to move to
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                    {
                        Vector3 distance = hit.point - transform.position;

                        //Check if the point the player clicked is within the maxDistance
                        if (distance.magnitude < maxDistance)
                        {
                            //Check if a waypoint is already placed
                            if (waypoint != null)
                            {
                                Destroy(waypoint);
                            }
                            //Place a waypoint
                            waypoint = Instantiate(waypointPrefab, hit.point, Quaternion.identity);

                            //Move to clicked point
                            agent.SetDestination(hit.point);
                            maxDistance -= distance.magnitude;

                            //change the size of the movement range circle to coreleate with the max distance
                            movementRange.transform.localScale = new Vector3(maxDistance * 2, 0.01f, maxDistance * 2);


                        }
                        else
                        {
                            Debug.Log("Too Far");
                        }

                        Debug.Log("Position: " + transform.position);
                        Debug.Log("Distance: " + distance.magnitude);
                    }
                }

                if (maxDistance < 0.5f && agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance == 0 && Vector3.Distance(transform.position, agent.destination) <= 1f)
                {
                    maxDistance = 10.0f;
                    turn.SwitchTurn();
                }

            }
            else
            {
                //Forward and backward movement
                transform.position += new Vector3(0, 0, 1) * Time.deltaTime * movementSpeed * Input.GetAxis("Vertical");

                //Left and right movement
                transform.position += new Vector3(1, 0, 0) * Time.deltaTime * movementSpeed * Input.GetAxis("Horizontal");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "waypoint")
        {
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "navMesh")
        {
            ToggleNavMesh(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "navMesh")
        {
            ToggleNavMesh(false);
        }
    }

    private void ToggleNavMesh(bool isOn)
    {
        if (!isOn)
        {
            rangeRen.enabled = false;
            agent.enabled = false;
            navMesh = false;
        }
        else
        {
            rangeRen.enabled = true;
            agent.enabled = true;
            navMesh = true;
        }
    }

    public void TogglePlayer(bool isOn)
    {
        if (isOn)
        {
            active = true;
        }
        else
        {
            active = false;
        }
    }
}