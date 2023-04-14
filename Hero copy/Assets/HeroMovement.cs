using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroMovement : MonoBehaviour
{
    //posiiton variable
    Vector3 pos;

    //game always starts in mouse mode
    public bool KeyBoardMode = false;

    [SerializeField]
    public float kHeroSpeed;
    public float kHeroMaxSpeed = 50.0f;

    [SerializeField]
    public float kHeroRotateSpeed;

    //egg variables
    public Transform eggSpawnPoint;
    public GameObject eggPrefab;

    //fire rate set to 0.2 
    public float firerate = 0.2f;
    public float nextFire = 0f;

    public Slider cooldownBar;
    public  float cooldownDuration = 0.2f;
    public float cooldownCurrent = 0f;

    void Start()
    {
        Debug.Log("mouse mode");
    }

    void Update()
    {
        //Quit the application
        if (Input.GetKey("q"))
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
        Cooldown();
        CheckBounds();
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
                cooldownCurrent = 0;
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
        if(Input.GetKey(KeyCode.W) && kHeroSpeed < kHeroMaxSpeed)
        {
            kHeroSpeed += 0.2f;
        }

        if(Input.GetKey(KeyCode.S) && kHeroSpeed > -1*kHeroMaxSpeed)
        {
            kHeroSpeed -= 0.2f;
        }

        //rotate left or right using A and D or left and right arrows
        transform.Rotate(Vector3.forward, -1f * Input.GetAxis("Horizontal") * (kHeroRotateSpeed * Time.smoothDeltaTime));

        //shoot an egg
        if((Input.GetKey(KeyCode.Space)) && Time.time > nextFire)
        {
            EggSpawn();
            cooldownCurrent = 0;
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

    private void CheckBounds()
    {
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.Clamp(currentPosition.x, -150f, 150f);
        currentPosition.y = Mathf.Clamp(currentPosition.y, -98f, 98f);
        transform.position = currentPosition;
    }

    private void EggSpawn()
    {
        //create a bullet at a position 
        GameObject eggbullet = Instantiate(eggPrefab, eggSpawnPoint.position, eggSpawnPoint.rotation);
        Rigidbody2D eggrb = eggbullet.GetComponent<Rigidbody2D>();

        //put speed on the bullet
        eggrb.velocity = ((kHeroSpeed + 40f) * eggSpawnPoint.up);

        //adjust the firerate
        nextFire = Time.time + firerate;

        //update the UI
        GlobalBehavior.sTheGlobalBehavior.IncreaseEggCountUI();
    }

    void Cooldown()
    {
        cooldownCurrent += Time.deltaTime;
        cooldownCurrent = Mathf.Clamp(cooldownCurrent, 0.0f, cooldownDuration);
        cooldownBar.value = cooldownCurrent;
    }

    //Pretty sure not needed
    private void OnTriggerEnter2D(Collider2D hitinfo)
    {
        if(hitinfo.name == "Plane")
        {
            hitinfo.gameObject.transform.position = Vector3.zero;
        }
    }

    //pretty sure not needed
    private void OnTriggerStay2D(Collider2D hitinfo)
    {
        if(hitinfo.name == "Plane")
        {
            hitinfo.gameObject.transform.position = Vector3.zero;
        }
    }
}
