using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneBehavior : MonoBehaviour
{
    //if collides with a hero
    private void OnTriggerEnter2D(Collider2D hitinfo)
    {
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
}
