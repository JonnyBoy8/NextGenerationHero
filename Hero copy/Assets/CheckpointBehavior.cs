using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointBehavior : MonoBehaviour
{
    //original value
    private float checkpoint_color = 0f;
    private float checkpoint_color_adjuster = 0f;
    
    void Start()
    {
        SpriteRenderer enemy = GetComponent<SpriteRenderer>();
        checkpoint_color = enemy.color.a;

        checkpoint_color_adjuster = checkpoint_color * 0.25f;
    }

    private void OnTriggerEnter2D(Collider2D hitinfo)
    {
        //if collides with a hero
        if(hitinfo.name == "Hero")
        {
            //TODO: NEED TO REIMPLEMENT. COMMENTED OUT FOR TESTING PURPOSES
            //Destroy(GameObject);
            Debug.Log("CHECKPOINT BEHAVIOR: Hero hit checkpoint");
        }
        else if(hitinfo.name == "Egg(Clone)")//if it gets hit by anything else (an egg), adjust the color
        {
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        SpriteRenderer checkpoint = GetComponent<SpriteRenderer>();
        Color current_color = checkpoint.color;
        current_color.a -= checkpoint_color_adjuster;

        checkpoint.color = current_color;

        if(checkpoint.color.a <= 0.0f)
        {
            //TODO: SPAWN THE CORRECT CHECKPOINT BACK IN
            string checkpoint_name = checkpoint.name;
            GlobalBehavior.sTheGlobalBehavior.SpawnNewCheckpoint(checkpoint_name);

            Destroy(gameObject);
        }
    }
}
