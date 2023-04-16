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

    void Start()
    {
        // Get the global behavior script
        globalBehavior = FindObjectOfType<GlobalBehavior>();
    }

    void Update()
    {
        HandleInput();
        Move();
        MoveToNearestCheckpoint(gameObject, globalBehavior);
        MoveToNextCheckpoint(gameObject, globalBehavior, gameObject);
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
        else //if it gets hit by anything else (an egg), adjust the color
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
            MoveToNearestCheckpoint(gameObject, globalBehavior);
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
    private void MoveToNearestCheckpoint(GameObject plane, GlobalBehavior globalBehavior)
    {
        if (moveToCheckpoint == true)
        {
            // Get the current position of the plane
            Vector3 planePos = plane.transform.position;

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

            // Move the plane towards the nearest checkpoint
            if (nearestCheckpoint != null)
            {
                Vector3 dir = (nearestCheckpoint.transform.position - planePos).normalized;
                Vector3 newPosition = planePos + dir * speed * Time.deltaTime;
                plane.transform.position = newPosition;

                if (Vector3.Distance(plane.transform.position, nearestCheckpoint.transform.position) < 0.1f)
                {
                    Debug.Log(nearestCheckpoint);
                    MoveToNextCheckpoint(plane, globalBehavior, nearestCheckpoint);
                    moveToCheckpoint = false;
                }
            }
        }
    }

    //Moves to next checkpoint in waypoints array.
    private void MoveToNextCheckpoint(GameObject plane, GlobalBehavior globalBehavior, GameObject currentCheckpoint)
    {
        Debug.Log("Move2CP");
        // Get the current position of the plane
        Vector3 planePos = plane.transform.position;
        int currentCheckpointIndex = Array.IndexOf(globalBehavior.waypoints, currentCheckpoint);

        // Move to the next checkpoint
        int nextCheckpointIndex = (currentCheckpointIndex + 1);
        
        if (nextCheckpointIndex >= globalBehavior.waypoints.Length)
        {
            nextCheckpointIndex = 0;
        }
        
        GameObject nextCheckpoint = globalBehavior.waypoints[nextCheckpointIndex];

        // Move the plane towards the next checkpoint
        Vector3 dir = (nextCheckpoint.transform.position - currentCheckpoint.transform.position).normalized;
        Debug.Log(nextCheckpoint.transform.position);
        Vector3 newPosition = planePos + dir * speed * Time.deltaTime;
        plane.transform.position = newPosition;
    }
}
