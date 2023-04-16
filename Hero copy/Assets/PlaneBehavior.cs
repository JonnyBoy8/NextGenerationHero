using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlaneBehavior : MonoBehaviour
{
    public float speed = 10.0f;
    public bool moveAroundBoundary = false;
    public bool moveToCheckpoint = true;

    private GlobalBehavior globalBehavior;

    //create new waypoints array to hold waypoints in
    private GameObject[] waypoints;

    GameObject currentCheckpoint;
    GameObject plane;

    int currentCheckpointIndex;

    void Start()
    {
        // Get the global behavior script
        globalBehavior = FindObjectOfType<GlobalBehavior>();

        //use getwaypoints to fill array with waypoints here
        waypoints = GlobalBehavior.sTheGlobalBehavior.GetWaypointArray();
        //Debug.Log("Waypoints length: " + waypoints.Length);
        for (int i = 0; i < waypoints.Length; i++)
        {
            //Debug.Log(waypoints[i]);
        }
    }

    void Update()
    {
        HandleInput();
        Move();
        MoveToNearestCheckpoint();
    }

    private void OnTriggerEnter2D(Collider2D hitinfo)
    {
        //if collides with a hero
        if(hitinfo.name == "Hero")
        {
            Destroy(gameObject);
            GlobalBehavior.sTheGlobalBehavior.UpdateEnemyDestroyUI();
            GlobalBehavior.sTheGlobalBehavior.UpdateHeroCollideUI();
            GlobalBehavior.sTheGlobalBehavior.ReduceEnemyCountUI();
            GlobalBehavior.sTheGlobalBehavior.CreatePlane();
        }
        else if(hitinfo.name == "Egg(Clone)") //if it gets hit by an egg, adjust the color
        {
            UpdateColor();
        }
    }

    //just in case the plane isnt destroyed on first hit, doesnt get called
    private void OnTriggerStay2D(Collider2D hitinfo)
    {
        if(hitinfo.name == "Hero")
        {
            Destroy(gameObject);
        }
    }

    private void UpdateColor()
    {
        //get plane color 
        SpriteRenderer enemy = GetComponent<SpriteRenderer>();

        //set plane color to the color variable to get modified
        Color current_color = enemy.color;

        //adjust the alpha by .8 (80%)
        current_color.a *= 0.8f;
        enemy.color = current_color;

        //if the color gets adjusted 4 times, destroy the plane
        if(enemy.color.a <= 0.35f)
        {
            GlobalBehavior.sTheGlobalBehavior.UpdateEnemyDestroyUI();
            GlobalBehavior.sTheGlobalBehavior.ReduceEnemyCountUI();
            Destroy(gameObject);

            //create a new plane to take its place
            GlobalBehavior.sTheGlobalBehavior.CreatePlane();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKey(KeyCode.P))
        {
            GlobalBehavior.ToggleMovement();
        }
    }

    public void Move()
    {
        if (moveAroundBoundary)
        {
            // Move around the boundary
            transform.Translate(Vector3.up * speed * Time.deltaTime);

            // Check if the plane is outside the world bounds
            if (!IsInsideWorldBounds(transform.position))
            {
                // Find a new direction within the world bounds
                Vector3 newDirection = GetRandomDirectionWithinWorldBounds();
                transform.up = newDirection;
            }
        }
        else
        {
            // Move towards the nearest checkpoint
            MoveToNearestCheckpoint();
        }
    }


    bool IsInsideWorldBounds(Vector3 position)
    {
        Camera mainCamera = Camera.main;
        float buffer = 5.0f; // add a buffer to the camera bounds

        // get the camera bounds in world space
        float cameraLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)).x + buffer;
        float cameraRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane)).x - buffer;
        float cameraBottom = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)).y + buffer;
        float cameraTop = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane)).y - buffer;

        // check if the position is within the camera bounds
        return position.x >= cameraLeft && position.x <= cameraRight && position.y >= cameraBottom && position.y <= cameraTop;
    }

    Vector3 GetRandomDirectionWithinWorldBounds()
    {
        float x = UnityEngine.Random.Range(-1f, 1f);
        float y = UnityEngine.Random.Range(-1f, 1f);
        Vector3 direction = new Vector3(x, y, 0).normalized;
        return direction;
    }

    //Moves to Nearest Checkpoint from spawn
    private void MoveToNearestCheckpoint()
    {
        if (moveToCheckpoint == true)
        {
            // Get the current position of the plane
            Vector3 planePos = transform.position;

            // Find the nearest checkpoint to the plane
            GameObject nearestCheckpoint = null;
            float minDistance = float.MaxValue;

            if (globalBehavior != null && globalBehavior.waypoints != null)
            {
                foreach (GameObject checkpoint in globalBehavior.waypoints)
                {
                    float distance = Vector3.Distance(planePos, checkpoint.transform.position);
                    if (distance < minDistance)
                    {
                        nearestCheckpoint = checkpoint;
                        minDistance = distance;
                    }
                }
            }
            //Debug.Log("1. Nearest Checkpoint: " + nearestCheckpoint);

            // Move the plane towards the nearest checkpoint
            if (nearestCheckpoint != null)
            {
                Vector3 dir = (nearestCheckpoint.transform.position - planePos).normalized;
                Vector3 newPosition = planePos + dir * speed * Time.deltaTime;
                transform.position = newPosition;

                if (Vector3.Distance(transform.position, nearestCheckpoint.transform.position) < 0.1f)
                {
                    //Debug.Log(nearestCheckpoint);
                    moveToCheckpoint = false;
                    currentCheckpoint = nearestCheckpoint;
                    //Debug.Log("2. Current Checkpoint: " + currentCheckpoint);
                }
            }
        }
        if (moveToCheckpoint == false)
        {
            MoveToNextCheckpoint();
        }
    }

    //Moves to next checkpoint in waypoints array.
    private void MoveToNextCheckpoint()
    {
        Debug.Log("1. Current Checkpoint: " + currentCheckpoint);
        // Get the current position of the plane
        Vector3 planePos = transform.position;
        currentCheckpointIndex = Array.IndexOf(waypoints, currentCheckpoint);
        Debug.Log("2. Current Checkpoint Index: " + currentCheckpointIndex);

        // Move to the next checkpoint
        int nextCheckpointIndex = (currentCheckpointIndex + 1);
        
        if (nextCheckpointIndex >= waypoints.Length)
        {
            nextCheckpointIndex = 0;
        }
        
        GameObject nextCheckpoint = waypoints[nextCheckpointIndex];

        // Move the plane towards the next checkpoint
        Vector3 dir = (nextCheckpoint.transform.position - transform.position).normalized;
        Debug.Log("3. Next Checkpoint: " + nextCheckpoint);
        Debug.Log("4. Next Checkpoint Index: " + nextCheckpointIndex);
        Vector3 newPosition = planePos + dir * speed * Time.deltaTime;
        transform.position = newPosition;

        if (Vector3.Distance(transform.position, nextCheckpoint.transform.position) < 0.1f)
        {
            // Move to the next checkpoint
            currentCheckpointIndex++;

            if (currentCheckpointIndex >= waypoints.Length)
            {
                currentCheckpointIndex = 0;
            }
            currentCheckpoint = waypoints[currentCheckpointIndex];
            Debug.Log("5. Current Checkpoint: " + currentCheckpoint);
            Debug.Log("6. Current Checkpoint Index: " + currentCheckpointIndex);
        }
    }

    //copied code from prof example
    private void PointAtPosition(Vector3 p, float r)
    {
        Vector3 v = p - transform.position;
        transform.up = Vector3.LerpUnclamped(transform.up, v, r);
    }
}
