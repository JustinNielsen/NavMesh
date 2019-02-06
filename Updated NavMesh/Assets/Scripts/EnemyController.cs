using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class EnemyController : MonoBehaviour
{

    bool active = false;
    float movementSpeed = 5.0f;
    private CinemachineVirtualCamera cam;
    public CinemachineVirtualCamera camPrefab;

    // Start is called before the first frame update
    void Start()
    {
        cam = Instantiate(camPrefab);
        cam.Priority = 5;
        cam.Follow = transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (active)
        {
            //Forward and backward movement
            transform.position += new Vector3(0, 0, 1) * Time.deltaTime * movementSpeed * Input.GetAxis("Vertical");

            //Left and right movement
            transform.position += new Vector3(1, 0, 0) * Time.deltaTime * movementSpeed * Input.GetAxis("Horizontal");

        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (cam.Priority > 10)
            {
                cam.Priority -= 10;
            }
            else
            {
                cam.Priority += 10;
            }
        }
    }

    public void ToggleEnemy(bool isOn)
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
