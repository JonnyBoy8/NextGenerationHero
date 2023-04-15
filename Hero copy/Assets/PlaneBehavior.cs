using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneBehavior : MonoBehaviour
{
    public float speed = 10.0f;
    public bool moveAroundBoundary = false;

    void Update()
    {
        HandleInput();
        Move();
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
        if (Input.GetKeyDown(KeyCode.P))
        {
            moveAroundBoundary = !moveAroundBoundary;
        }
    }

    void Move()
    {
        if (moveAroundBoundary == true)
        {
            // move the enemy up until it reaches the world bounds
            transform.Translate(Vector3.up * speed * Time.deltaTime);

            // check if the enemy is outside of the world bounds
            if (!IsInsideWorldBounds(transform.position))
            {
                // find a new direction within the world bounds
                Vector3 newDirection = GetRandomDirectionWithinWorldBounds();
                transform.up = newDirection;
            }
        }
    }

    bool IsInsideWorldBounds(Vector3 position)
    {
        return position.x >= -90f && position.x <= 90f && position.y >= -90f && position.y <= 90f;
    }

    Vector3 GetRandomDirectionWithinWorldBounds()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        Vector3 direction = new Vector3(x, y, 0).normalized;
        return direction;
    }
}
