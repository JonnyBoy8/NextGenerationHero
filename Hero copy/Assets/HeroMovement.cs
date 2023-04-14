using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    Vector3 pos;

    //game always starts in mouse mode
    public bool KeyBoardMode = false;

    [SerializeField]
    public float kHeroSpeed;

    [SerializeField]
    public float kHeroRotateSpeed;

    //egg variables
    public Transform eggSpawnPoint;
    public GameObject eggPrefab;

    //frame rate set to 0.2 
    public float firerate = 0.2f;
    public float nextFire = 0f;

    void Start()
    {
        Debug.Log("mouse mode");
    }

    void Update()
    {
        //Quit the application
        if(Input.GetKey("q"))
        {
            Application.Quit();
        }

        //If in mouse mode go here
        if(!KeyBoardMode)
        {
            //switch from mouse mode to keyboard mode
            if(Input.GetKeyDown("m"))
            {
                KeyBoardMode = true;
                Debug.Log("Switching to Keyboard Mode");
                GlobalBehavior.sTheGlobalBehavior.UpdateToKeyboardUI();
                return;
            }

            MouseSettings();
        }
        else //else in keyboard mode, go here
        {
            //switch from keyboard mode to mouse mode
            if(Input.GetKeyDown("m"))
            {
                KeyBoardMode = false;
                Debug.Log("Switching to mouse mode");
                GlobalBehavior.sTheGlobalBehavior.UpdateToMouseUI();
                return;
            }

            KeyBoardSettings();
        }
    }

    private void MouseSettings()
    {
            //plane follows cursor
            pos = Input.mousePosition;
            pos.z = 1f;
            transform.position = Camera.main.ScreenToWorldPoint(pos);

            //AD rotate
            kHeroRotateSpeed = 45f; 
            float rotateInput = Input.GetAxis("Horizontal");
            float angle = (-1f * rotateInput) * (kHeroRotateSpeed * Time.smoothDeltaTime);

            transform.Rotate(transform.forward, angle);

            //spacebar and has fire rate
            if(Input.GetKey(KeyCode.Space) && Time.time > nextFire)
            {
                EggSpawn();
            }

        //switch from mousemode to keyboard mode
        if(Input.GetKeyDown("m"))
        {
                KeyBoardMode = true;
                Debug.Log("Switching to Keyboard Mode");
                GlobalBehavior.sTheGlobalBehavior.UpdateToKeyboardUI();
                return;
        }
    }

    private void KeyBoardSettings()
    {
        kHeroRotateSpeed = 45f; 

        //sprite continues moving up with the speed
        transform.position += transform.up * (kHeroSpeed * Time.smoothDeltaTime);

        //adjust the speed using W and S
        if(Input.GetKey(KeyCode.W))
        {
            kHeroSpeed += 1f;
        }

        if(Input.GetKey(KeyCode.S))
        {
            kHeroSpeed -= 1f;
        }

        //rotate left or right using A and D or left and right arrows
        transform.Rotate(Vector3.forward, -1f * Input.GetAxis("Horizontal") * (kHeroRotateSpeed * Time.smoothDeltaTime));

        //shoot an egg
        if((Input.GetKey(KeyCode.Space)) && Time.time > nextFire)
        {
            EggSpawn();
        }

        //swicth from keyboard mode to mouse mode
        if(Input.GetKeyDown("m"))
        {
            KeyBoardMode = false;
            Debug.Log("Switching to mouse mode");
            GlobalBehavior.sTheGlobalBehavior.UpdateToMouseUI();
            return;
        }
    }

    private void EggSpawn()
    {
        //create a bullet at a position 
        GameObject eggbullet = Instantiate(eggPrefab, eggSpawnPoint.position, eggSpawnPoint.rotation);
        Rigidbody2D eggrb = eggbullet.GetComponent<Rigidbody2D>();

        //put speed on the bullet
        eggrb.velocity = 40f * eggSpawnPoint.up;

        //adjust the firerate
        nextFire = Time.time + firerate;

        //update the UI
        GlobalBehavior.sTheGlobalBehavior.IncreaseEggCountUI();
    }

    //Pretty sure not needed
    private void OnTriggerEnter2D(Collider2D hitinfo)
    {
        if(hitinfo.name == "Plane")
        {
            hitinfo.gameObject.transform.position = Vector3.zero;
        }
    }

    private void OnTriggerStay2D(Collider2D hitinfo)
    {
        if(hitinfo.name == "Plane")
        {
            hitinfo.gameObject.transform.position = Vector3.zero;
        }
    }
}
