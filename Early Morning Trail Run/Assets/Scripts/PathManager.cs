using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public List<Vector3> points = new List<Vector3>();
    public int id;

    public Path(int id)
    {
        this.id = id;
    }
}

public class PathManager : MonoBehaviour
{   static public Dictionary<Vector2Int, List<Path>> gridDict = new Dictionary<Vector2Int, List<Path>>();
    static public PathManager Instance;
    public PathGenerator mainPath;
    public List<PathGenerator> paths = new List<PathGenerator>();

    [Header("Points Along the Path")]
    public int pathLength;

    [Header("Distance and Bend Ranges")]
    public float minStepDistance;
    public float maxStepDistance;
    public float bendAmount;

    [Header("Path Smoothing")]
    public int segmentsPerCurve;

    [Header("Fork Settings")]
    public GameObject pathPrefab;
    public int forkChance;
    public int forkCooldown;
    public float forkOffset;
    public int pathLimit;

    [Header("World Settings")]
    public float worldSize;

    [Header("Partitioning Grid Settings")]
    public float cellSize;
    public float minProximity;
    private int indexer;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        paths.Add(mainPath);
        StartNextPath();
    }

    public void StartNextPath()
    {
        if(indexer < paths.Count)
        {
            StartCoroutine(paths[indexer].GeneratePath());
            indexer++;
        }
    }
}
