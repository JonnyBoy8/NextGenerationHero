using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggBehavior : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        //continously check if the bullet hit the world bounds
        if (GlobalBehavior.sTheGlobalBehavior.ObjectCollideWorldBound(GetComponent<Renderer>().bounds) == GlobalBehavior.WorldBoundStatus.Outside)
        {
            Destroy(gameObject); 
            GlobalBehavior.sTheGlobalBehavior.DecreaseEggCountUI();
        }
    }

    //when bullet collides with plane
    private void OnTriggerEnter2D(Collider2D hitinfo)
    {
        if(hitinfo.name == "Plane(Clone)")
        {
            Destroy(gameObject);
            GlobalBehavior.sTheGlobalBehavior.DecreaseEggCountUI();
        }
        else if(hitinfo.tag == "Checkpoint")
        {
            Destroy(gameObject);
            GlobalBehavior.sTheGlobalBehavior.DecreaseEggCountUI();
        }
    }
}
