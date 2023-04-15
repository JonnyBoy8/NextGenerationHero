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

    private Vector2 checkpoint_aPrevLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_bPrevLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_cPrevLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_dPrevLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_ePrevLocation = new Vector2(0f, 0f);
    private Vector2 checkpoint_fPrevLocation = new Vector2(0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(mEggCountUI != null);
        GlobalBehavior.sTheGlobalBehavior = this;

        mMainCamera = Camera.main;
        mWorldBound = new Bounds(Vector3.zero, Vector3.one);
        UpdateWorldWindowBound();

        //call the create plane function so spawn planes in random spaces within bounds
        for(int i = 0; i < 10; i++)
        {
            CreatePlane();
        }

        SpawnFirstCheckpoints();
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
        Vector2 poistion = new Vector2(0.9f*x, 0.9f*y);

        //create a plane at that location
        GameObject new_plane = Instantiate(planePrefab, poistion, Quaternion.identity);
        IncreaseEnemyCountUI();
    }

    //ONLY USED AT START OF GLOBAL BEHAVIOR
    //spawns in the locations of the checkpoints
    private void SpawnFirstCheckpoints()
    {
        //SPAWN CHECKPOINT A
        //find the bound
        float x = Random.Range(mWorldMin.x, mWorldMax.x);
        float y = Random.Range(mWorldMin.y, mWorldMax.y);

        //create a position of the plane
        Vector2 poistion = new Vector2(0.9f*x, 0.9f*y);
        checkpoint_aPrevLocation = poistion;

        //create a plane at that location
        GameObject new_plane = Instantiate(checkpoint_aPrefab, poistion, Quaternion.identity);  

        //SPAWN CHECKPOINT B
        //find the bound
        x = Random.Range(mWorldMin.x, mWorldMax.x);
        y = Random.Range(mWorldMin.y, mWorldMax.y);

        //create a position of the plane
        poistion = new Vector2(0.9f*x, 0.9f*y);
        checkpoint_bPrevLocation = poistion;

        //create a plane at that location
        new_plane = Instantiate(checkpoint_bPrefab, poistion, Quaternion.identity); 

        //SPAWN CHECKPOINT C
        //find the bound
        x = Random.Range(mWorldMin.x, mWorldMax.x);
        y = Random.Range(mWorldMin.y, mWorldMax.y);

        //create a position of the plane
        poistion = new Vector2(0.9f*x, 0.9f*y);
        checkpoint_cPrevLocation = poistion;

        //create a plane at that location
        new_plane = Instantiate(checkpoint_cPrefab, poistion, Quaternion.identity); 

        //SPAWN CHECKPOINT D
        //find the bound
        x = Random.Range(mWorldMin.x, mWorldMax.x);
        y = Random.Range(mWorldMin.y, mWorldMax.y);

        //create a position of the plane
        poistion = new Vector2(0.9f*x, 0.9f*y);
        checkpoint_dPrevLocation = poistion;

        //create a plane at that location
        new_plane = Instantiate(checkpoint_dPrefab, poistion, Quaternion.identity); 

        //SPAWN CHECKPOINT E
        //find the bound
        x = Random.Range(mWorldMin.x, mWorldMax.x);
        y = Random.Range(mWorldMin.y, mWorldMax.y);

        //create a position of the plane
        poistion = new Vector2(0.9f*x, 0.9f*y);
        checkpoint_ePrevLocation = poistion;

        //create a plane at that location
        new_plane = Instantiate(checkpoint_ePrefab, poistion, Quaternion.identity); 

        //SPAWN CHECKPOINT F
        //find the bound
        x = Random.Range(mWorldMin.x, mWorldMax.x);
        y = Random.Range(mWorldMin.y, mWorldMax.y);

        //create a position of the plane
        poistion = new Vector2(0.9f*x, 0.9f*y);
        checkpoint_fPrevLocation = poistion;

        //create a plane at that location
        new_plane = Instantiate(checkpoint_fPrefab, poistion, Quaternion.identity); 
    }

    //takes in the name of the destroyed sprite, then spawns a new one +- 15
    public void SpawnNewCheckpoint(string name)
    {
        if(name == "CheckpointA(Clone)")
        {
            Vector2 new_position = new Vector2(checkpoint_aPrevLocation.x + RandomGenerator(), checkpoint_aPrevLocation.y + RandomGenerator());
            checkpoint_aPrevLocation = new_position;

            GameObject new_checkpoint = Instantiate(checkpoint_aPrefab, new_position, Quaternion.identity);
        }
        else if(name == "CheckpointB(Clone)")
        {
            Vector2 new_position = new Vector2(checkpoint_bPrevLocation.x + RandomGenerator(), checkpoint_bPrevLocation.y + RandomGenerator());
            checkpoint_bPrevLocation = new_position;

            GameObject new_checkpoint = Instantiate(checkpoint_bPrefab, new_position, Quaternion.identity);
        }
        else if(name == "CheckpointC(Clone)")
        {
            Vector2 new_position = new Vector2(checkpoint_cPrevLocation.x + RandomGenerator(), checkpoint_cPrevLocation.y + RandomGenerator());
            checkpoint_cPrevLocation = new_position;

            GameObject new_checkpoint = Instantiate(checkpoint_cPrefab, new_position, Quaternion.identity);
        }
        else if(name == "CheckpointD(Clone)")
        {
            Vector2 new_position = new Vector2(checkpoint_dPrevLocation.x + RandomGenerator(), checkpoint_dPrevLocation.y + RandomGenerator());
            checkpoint_dPrevLocation = new_position;

            GameObject new_checkpoint = Instantiate(checkpoint_dPrefab, new_position, Quaternion.identity);
        }
        else if(name == "CheckpointE(Clone)")
        {
            Vector2 new_position = new Vector2(checkpoint_ePrevLocation.x + RandomGenerator(), checkpoint_ePrevLocation.y + RandomGenerator());
            checkpoint_ePrevLocation = new_position;

            GameObject new_checkpoint = Instantiate(checkpoint_ePrefab, new_position, Quaternion.identity);
        }
        else if(name == "CheckpointF(Clone)")
        {
            Vector2 new_position = new Vector2(checkpoint_fPrevLocation.x + RandomGenerator(), checkpoint_fPrevLocation.y + RandomGenerator());
            checkpoint_fPrevLocation = new_position;

            GameObject new_checkpoint = Instantiate(checkpoint_fPrefab, new_position, Quaternion.identity);
        }
    }

    private float RandomGenerator()
    {
        int decider_num = Random.Range(0, 2);

        if(decider_num == 0)
        {
            Debug.Log("-15");
            return -15f;
        }
        else
        {
            Debug.Log("15");
            return 15f;
        }


    }
}
