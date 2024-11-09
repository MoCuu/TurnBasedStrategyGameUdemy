using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGrid : MonoBehaviour
{
    public static MoveGrid instance;

    private void Awake()
    {
        instance = this;

        GenerateMoveGrid();

        HideMovePoints();
    }

    public MovePoint startPoint;

    public Vector2Int spawnRange; // of movement grid

    public LayerMask whatIsGround, whatIsObstacle;

    public float obstacleCheckRange;

    public List<MovePoint> allMovePoints = new List<MovePoint>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMoveGrid()
    {
        for (int x = -spawnRange.x; x <= spawnRange.x; x++)
        {
            for (int y = -spawnRange.y; y <= spawnRange.y; y++)
            {
                RaycastHit hit;

                if (Physics.Raycast(transform.position + new Vector3(x, 10f, y), Vector3.down, out hit, 20f, whatIsGround))
                {
                    if (Physics.OverlapSphere(hit.point, obstacleCheckRange, whatIsObstacle).Length == 0)
                    {
                        MovePoint newPoint = Instantiate(startPoint, hit.point, transform.rotation);
                        newPoint.transform.SetParent(transform);

                        allMovePoints.Add(newPoint);
                    }
                }
            }
        }

        startPoint.gameObject.SetActive(false);
    }

    public void HideMovePoints()
    {
        foreach (MovePoint point in allMovePoints)
        {
            point.gameObject.SetActive(false);
        }
    }

    public void ShowPointsInRange(float moveRange, Vector3 centerPoint)
    {
        HideMovePoints();

        foreach (MovePoint mp in allMovePoints)
        {
            if (Vector3.Distance(centerPoint, mp.transform.position) <= moveRange)
            {
                mp.gameObject.SetActive(true);

                foreach (CharacterController cc in GameManager.instance.allChars)
                {
                    if (Vector3.Distance(cc.transform.position, mp.transform.position) < .5f)
                    {
                        mp.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public List<MovePoint> GetMovePointsInRange(float moveRange, Vector3 centerPoint)
    {
        List<MovePoint> foundPoints = new List<MovePoint>();

        foreach (MovePoint mp in allMovePoints)
        {
            if (Vector3.Distance(centerPoint, mp.transform.position) <= moveRange)
            {
                bool shouldAdd = true;

                foreach (CharacterController cc in GameManager.instance.allChars)
                {
                    if (Vector3.Distance(cc.transform.position, mp.transform.position) < .5f)
                    {
                        shouldAdd = false;
                    }
                }

                if (shouldAdd == true)
                {
                    foundPoints.Add(mp);
                }
            }
        }

        return foundPoints;
    }
}
