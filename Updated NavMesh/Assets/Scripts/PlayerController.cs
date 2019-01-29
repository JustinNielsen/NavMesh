using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
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

    private void Start()
    {
        //Change the size of the movementRange based on the maxDistance
        movementRange.transform.localScale = new Vector3(maxDistance * 2, 0.01f, maxDistance * 2);
        rangeRen = movementRange.GetComponent<MeshRenderer>();
        ToggleNavMesh();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Toggle of and on the navmesh
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleNavMesh();
        }

        if (navMesh)
        {
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
                    }
                    else
                    {
                        Debug.Log("Too Far");
                    }

                    Debug.Log("Position: " + transform.position);
                    Debug.Log("Distance: " + distance.magnitude);
                }
            } 
        }
        else
        {
            //Forward and backward movement
            transform.position += new Vector3( 0, 0, 1) * Time.deltaTime * movementSpeed * Input.GetAxis("Vertical");

            //Left and right movement
            transform.position += new Vector3(1, 0, 0) * Time.deltaTime * movementSpeed * Input.GetAxis("Horizontal");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "waypoint")
        {
            Destroy(other.gameObject);
        }
        else if(other.gameObject.tag == "navMesh")
        {
            ToggleNavMesh();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "navMesh")
        {
            ToggleNavMesh();
        }
    }

    private void ToggleNavMesh()
    {
        if (navMesh)
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
}