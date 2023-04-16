using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalBehavior : MonoBehaviour
{
    public static GlobalBehavior sTheGlobalBehavior = null;
    public GameObject planePrefab;

    //Access and edit text boxes
    public Text mHeroModeUI = null;

    public Text mHeroCollideUI = null;
    private int mHeroCollide = 0;

    public Text mEnemyCountUI = null;
    private int mEnemyCount = 0;
    private const int maxNumPlanes = 1;

    public Text mEnemyDestroyedUI = null;
    private int mEnemyDestroyed = 0;

    public Text mEggCountUI = null;
    private int mEggCount = 0;

    //world bound code
    private Bounds mWorldBound;
    private Vector2 mWorldMin;
    private Vector2 mWorldMax;
    private Vector2 mWorldCenter;
    private Camera mMainCamera;

    //waypoint code
    public GameObject checkpoint_aPrefab;
    public GameObject checkpoint_bPrefab;
    public GameObject checkpoint_cPrefab;
    public GameObject checkpoint_dPrefab;
    public GameObject checkpoint_ePrefab;
    public GameObject checkpoint_fPrefab;

    private Vector2 checkpoint_aInitialLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_bInitialLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_cInitialLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_dInitialLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_eInitialLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_fInitialLocation = new Vector2(0f, 0f);

    public static bool moveAroundBoundary = false;

    //array to hold waypoints
    public GameObject[] waypoints;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(mEggCountUI != null);
        GlobalBehavior.sTheGlobalBehavior = this;

        mMainCamera = Camera.main;
        mWorldBound = new Bounds(Vector3.zero, Vector3.one);
        UpdateWorldWindowBound();

        //call the create plane function so spawn planes in random spaces within bounds
        for(int i = 0; i < maxNumPlanes; i++)
        {
            CreatePlane();
        }

        FirstWaypointLocation();

        //fill the array with waypoints at first start
        waypoints = new GameObject[6];
        waypoints[0] = GameObject.Find("CheckpointA");
        waypoints[1] = GameObject.Find("CheckpointB");
        waypoints[2] = GameObject.Find("CheckpointC");
        waypoints[3] = GameObject.Find("CheckpointD");
        waypoints[4] = GameObject.Find("CheckpointE");
        waypoints[5] = GameObject.Find("CheckpointF");
    }

    public enum WorldBoundStatus {
		CollideTop,
		CollideLeft,
		CollideRight,
		CollideBottom,
		Outside,
		Inside
	};

    void UpdateWorldWindowBound()
    {
        if (null != mMainCamera) {
			float maxY = mMainCamera.orthographicSize;
			float maxX = mMainCamera.orthographicSize * mMainCamera.aspect;
			float sizeX = 2 * maxX;
			float sizeY = 2 * maxY;
			float sizeZ = Mathf.Abs(mMainCamera.farClipPlane - mMainCamera.nearClipPlane);
			
			// Make sure z-component is always zero
			Vector3 c = mMainCamera.transform.position;
			c.z = 0.0f;
			mWorldBound.center = c;
			mWorldBound.size = new Vector3(sizeX, sizeY, sizeZ);

			mWorldCenter = new Vector2(c.x, c.y);
			mWorldMin = new Vector2(mWorldBound.min.x, mWorldBound.min.y);
			mWorldMax = new Vector2(mWorldBound.max.x, mWorldBound.max.y);
		}
    }

    public Vector2 WorldCenter { get { return mWorldCenter; } }
	public Vector2 WorldMin { get { return mWorldMin; }} 
	public Vector2 WorldMax { get { return mWorldMax; }}

    public WorldBoundStatus ObjectCollideWorldBound(Bounds objBound)
	{
		WorldBoundStatus status = WorldBoundStatus.Inside;

		if (mWorldBound.Intersects (objBound)) {
			if (objBound.max.x > mWorldBound.max.x)
				status = WorldBoundStatus.CollideRight;
			else if (objBound.min.x < mWorldBound.min.x)
				status = WorldBoundStatus.CollideLeft;
			else if (objBound.max.y > mWorldBound.max.y)
				status = WorldBoundStatus.CollideTop;
			else if (objBound.min.y < mWorldBound.min.y)
				status = WorldBoundStatus.CollideBottom;
			else if ((objBound.min.z < mWorldBound.min.z) || (objBound.max.z > mWorldBound.max.z))
				status = WorldBoundStatus.Outside;
		} else 
			status = WorldBoundStatus.Outside;

		return status;
	}

    public WorldBoundStatus ObjectClampToWorldBound(Transform t)
    {
        WorldBoundStatus status = WorldBoundStatus.Inside;
        Vector3 p = t.position;

        if (p.x > WorldMax.x)
        {
            status = WorldBoundStatus.CollideRight;
            p.x = WorldMax.x;
        }
        else if (t.position.x < WorldMin.x)
        {
            status = WorldBoundStatus.CollideLeft;
            p.x = WorldMin.x;
        }

        if (p.y > WorldMax.y)
        {
            status = WorldBoundStatus.CollideTop;
            p.y = WorldMax.y;
        }
        else if (p.y < WorldMin.y)
        {
            status = WorldBoundStatus.CollideBottom;
            p.y = WorldMin.y;
        }

        if ((p.z < mWorldBound.min.z) || (p.z > mWorldBound.max.z))
        {
            status = WorldBoundStatus.Outside;
        }

        t.position = p;
        return status;
    }

    //UI METHODS
    public void UpdateToMouseUI()
    {
        mHeroModeUI.text = "Mouse Mode";
    }

    public void UpdateToKeyboardUI()
    {
        mHeroModeUI.text = "Keyboard Mode";
    }

    public void UpdateHeroCollideUI()
    {
        mHeroCollide++;

        string to_text = "Enemy Collisions: " + mHeroCollide.ToString();
        mHeroCollideUI.text = to_text;
    }

    public void ReduceEnemyCountUI()
    {
        mEnemyCount--;
        string to_text = "Shown: " + mEnemyCount.ToString();

        mEnemyCountUI.text = to_text;
    }

    public void IncreaseEnemyCountUI()
    {
        mEnemyCount++;
        string to_text = "Shown: " + mEnemyCount.ToString();

        mEnemyCountUI.text = to_text;
    }

    public void UpdateEnemyDestroyUI()
    {
        mEnemyDestroyed++;

        string to_text = "Destroyed: " + mEnemyDestroyed.ToString();

        mEnemyDestroyedUI.text = to_text;
    }

    public void IncreaseEggCountUI()
    {
        mEggCount++;

        UpdateEggCountUI();
    }

    public void DecreaseEggCountUI()
    {
        mEggCount--;

        if(mEggCount < 0)
        {
            mEggCount = 0;
        }

        UpdateEggCountUI();
    }

    private void UpdateEggCountUI()
    {
        string to_text = "Egg Count: " + mEggCount.ToString();
        mEggCountUI.text = to_text;
    }

    //spawn planes randomly wihtin bounds
    public void CreatePlane()
    {
        //find the bound
        float x = Random.Range(mWorldMin.x, mWorldMax.x);
        float y = Random.Range(mWorldMin.y, mWorldMax.y);

        //create a position of the plane
        Vector2 position = new Vector2(0.9f*x, 0.9f*y);

        //create a plane at that location
        GameObject new_plane = Instantiate(planePrefab, position, Quaternion.identity);
        IncreaseEnemyCountUI();
        PlaneBehavior planeBehavior = new_plane.GetComponent<PlaneBehavior>();
        planeBehavior.Move();
    }

    //ONLY USED AT START OF GLOBAL BEHAVIOR
    //spawns in the locations of the checkpoints
    private void FirstWaypointLocation()
    {
        checkpoint_aInitialLocation = checkpoint_aPrefab.transform.position;
        checkpoint_bInitialLocation = checkpoint_bPrefab.transform.position;
        checkpoint_cInitialLocation = checkpoint_cPrefab.transform.position;
        checkpoint_dInitialLocation = checkpoint_dPrefab.transform.position;
        checkpoint_eInitialLocation = checkpoint_ePrefab.transform.position;
        checkpoint_fInitialLocation = checkpoint_fPrefab.transform.position;

    }

    //takes in the name of the destroyed sprite, then spawns a new one +- 15
    public void SpawnNewCheckpoint(string name)
    {
        if(name == "CheckpointA")
        {
            Vector2 new_position = RandomGenerator(checkpoint_aInitialLocation.x, checkpoint_aInitialLocation.y);
            checkpoint_aInitialLocation = new_position;

            checkpoint_aPrefab.transform.position = new_position;
        }
        else if(name == "CheckpointB")
        {
            Vector2 new_position = RandomGenerator(checkpoint_bInitialLocation.x, checkpoint_bInitialLocation.y);
            checkpoint_bInitialLocation = new_position;

            checkpoint_bPrefab.transform.position = new_position;
        }
        else if(name == "CheckpointC")
        {
            Vector2 new_position = RandomGenerator(checkpoint_cInitialLocation.x, checkpoint_cInitialLocation.y);
            checkpoint_cInitialLocation = new_position;

            checkpoint_cPrefab.transform.position = new_position;
        }
        else if(name == "CheckpointD")
        {
            Vector2 new_position = RandomGenerator(checkpoint_dInitialLocation.x, checkpoint_dInitialLocation.y);
            checkpoint_dInitialLocation = new_position;

            checkpoint_dPrefab.transform.position = new_position;
        }
        else if(name == "CheckpointE")
        {
            Vector2 new_position = RandomGenerator(checkpoint_eInitialLocation.x, checkpoint_eInitialLocation.y);
            checkpoint_eInitialLocation = new_position;

            checkpoint_ePrefab.transform.position = new_position;
        }
        else if(name == "CheckpointF")
        {
            Vector2 new_position = RandomGenerator(checkpoint_fInitialLocation.x, checkpoint_fInitialLocation.y);
            checkpoint_fInitialLocation = new_position;

            checkpoint_fPrefab.transform.position = new_position;
        }
    }

    public void DisableWayPoints()
    {
        //update the waypoint array before deactivation
        waypoints = GameObject.FindGameObjectsWithTag("Checkpoint");

        Debug.Log("disable");

        foreach (GameObject waypoint in waypoints)
        {
            waypoint.SetActive(false);
        }
    }

    public void EnableWayPoints()
    {

        Debug.Log("enable");

        //reactivate each waypoint present in the waypoints array
        foreach (GameObject waypoint in waypoints)
        {
            waypoint.SetActive(true);
        }
    }

    private Vector2 RandomGenerator(float prev_positionx, float prev_positiony)
    {
        int decider_num = Random.Range(0, 2);
        float random_x = 0f;
        float random_y = 0f;

        //do random x first
        if(decider_num == 0)
        {
            Debug.Log("x: -15");
            random_x = -15f;
        }
        else
        {
            Debug.Log("x: 15");
            random_x = 15f;
        }

        int decider_numy = Random.Range(0, 2);
        //now do random y
        if(decider_numy == 0)
        {
            Debug.Log("y: -15");
            random_y = -15f;
        }
        else
        {
            Debug.Log("y: 15");
            random_y = 15f;
        }

        //check if in bounds
        //if the new x coordinate is bigger than max x
        if(prev_positionx + random_x >= 0.8*mWorldMax.x)
        {
            Debug.Log("new x: " + random_x);
            random_x = -15f;
        }

        //if the new x coordinate is less than min x
        if(prev_positionx + random_x <= 0.8*mWorldMin.x)
        {
            Debug.Log("new x: " + random_x);
            random_x = 15f;
        }

        //if the new y coordinate is bigger than max y
        if(prev_positiony + random_y >= 0.8*mWorldMax.y)
        {
            Debug.Log("new y: " + random_y);
            random_y = -15f;
        }

        //if the new y coordinate is less than min y
        if(prev_positiony + random_y <= 0.8*mWorldMin.y)
        {
            Debug.Log("new y: " + random_y);
            random_y = 15f;
        }

        return new Vector2(prev_positionx + random_x, prev_positiony + random_y);
    }

    public static void ToggleMovement()
    {
        moveAroundBoundary = !moveAroundBoundary;
    }

    public GameObject[] GetWaypointArray()
    {
        return waypoints;
    }
}
