using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
public class PathGenerator : MonoBehaviour
{

    
    [Header("Path Read Values")]
    public bool generatingPath;

    private LineRenderer path, minimapPath;
    private EdgeCollider2D col;
    private Path thisPath;
    public Path parentPath;
    private List<Vector3> pathPoints = new List<Vector3>();
    private List<Vector3> smoothedPoints = new List<Vector3>();
    private List<GameObject> childPaths = new List<GameObject>();

    public IEnumerator GeneratePath()
    {
        generatingPath = true;
        thisPath = new Path(transform.GetSiblingIndex());
        
        AddPoint(transform.position);

        int lastForkIndex = 0;

        for (int i = 0; i < PathManager.Instance.pathLength - 1; i++)
        {
            Vector3 potentialPos = transform.position + transform.up * Random.Range(PathManager.Instance.minStepDistance, PathManager.Instance.maxStepDistance);
            Vector3 potentialRot = transform.eulerAngles + new Vector3(0, 0, Random.Range(-PathManager.Instance.bendAmount, PathManager.Instance.bendAmount));

            int maxAttempts = 100;
            int attempts = 0;

            float adaptiveBend = PathManager.Instance.bendAmount;
            float adaptiveStep = PathManager.Instance.maxStepDistance;

            while (IsPointTooClose(potentialPos) && attempts < maxAttempts)
            {
                adaptiveBend += 0.5f;
                adaptiveStep += 4f;

                potentialRot = transform.eulerAngles + new Vector3(0, 0, Random.Range(-adaptiveBend, adaptiveBend));
                potentialPos = transform.position + transform.up * Random.Range(adaptiveStep - PathManager.Instance.maxStepDistance + PathManager.Instance.minStepDistance, adaptiveStep);
                attempts++;
            }

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning($"{transform.name} Failed to find a suitable position after {maxAttempts} attempts.");
                break; // Exit the loop and potentially the point generation process
            }

            transform.position = potentialPos;
            transform.rotation = Quaternion.Euler(potentialRot);

            if (((Random.Range(0, PathManager.Instance.forkChance) == 0 && i - lastForkIndex > PathManager.Instance.forkCooldown) || (transform.parent.childCount < PathManager.Instance.pathLimit / 2 && i > PathManager.Instance.pathLength / 0.75f)) && transform.parent.childCount < PathManager.Instance.pathLimit)
            {
                float forkDirection = Random.Range(-PathManager.Instance.bendAmount, PathManager.Instance.bendAmount);
                GameObject branchPath = Instantiate(PathManager.Instance.pathPrefab, transform.position, Quaternion.Euler(0, 0, transform.eulerAngles.z + ((forkDirection > 0) ? forkDirection + PathManager.Instance.forkOffset : forkDirection - PathManager.Instance.forkOffset)), transform.parent);
                branchPath.transform.name = "BranchPath_" + (transform.parent.childCount - 1);
                branchPath.GetComponent<PathGenerator>().parentPath = thisPath;
                transform.parent.GetComponent<PathManager>().paths.Add(branchPath.GetComponent<PathGenerator>());
                childPaths.Add(branchPath);

                lastForkIndex = i;
            }

            if (Mathf.Abs(transform.position.x) < PathManager.Instance.worldSize && Mathf.Abs(transform.position.y) < PathManager.Instance.worldSize)
                AddPoint(transform.position);
            else
                i = PathManager.Instance.pathLength;
        }

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector3 p0 = i > 0 ? pathPoints[i - 1] : pathPoints[i];
            Vector3 p1 = pathPoints[i];
            Vector3 p2 = pathPoints[i + 1];
            Vector3 p3 = i + 2 < pathPoints.Count ? pathPoints[i + 2] : pathPoints[i + 1];

            for (int t = 0; t < PathManager.Instance.segmentsPerCurve; t++)
            {
                float u = t / (float)PathManager.Instance.segmentsPerCurve;
                smoothedPoints.Add(CatmullRom(p0, p1, p2, p3, u));
            }
        }

        path = GetComponent<LineRenderer>();
        path.positionCount = smoothedPoints.Count;
        path.SetPositions(smoothedPoints.ToArray());
        path.sortingOrder = PathManager.Instance.pathLimit - transform.parent.childCount;

        minimapPath = transform.GetChild(0).GetComponent<LineRenderer>();
        minimapPath.positionCount = smoothedPoints.Count;
        minimapPath.SetPositions(smoothedPoints.ToArray());

        col = GetComponent<EdgeCollider2D>();
        col.points = smoothedPoints.Select(p => (Vector2)transform.InverseTransformPoint(p)).ToArray();

        yield return null;
        transform.parent.GetComponent<PathManager>().StartNextPath();

    }
    private Vector2Int GetCellIndex(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / PathManager.Instance.cellSize),
            Mathf.FloorToInt(position.y / PathManager.Instance.cellSize)
        );
    }
    public void AddPoint(Vector3 point)
    {
        Vector2Int cellIndex = GetCellIndex(point);
        int pathIndex = -1;

        if (!PathManager.gridDict.ContainsKey(cellIndex))
        {
            PathManager.gridDict[cellIndex] = new List<Path> { thisPath };
            pathIndex = PathManager.gridDict[cellIndex].Count - 1;
        }
        else
        {
            bool containsCurrentPath = false;

            for (int i = 0; i < PathManager.gridDict[cellIndex].Count; i++)
            {
                if (PathManager.gridDict[cellIndex][i].id == thisPath.id)
                {
                    containsCurrentPath = true;
                    pathIndex = i;
                }
            }

            if (!containsCurrentPath)
            {
                PathManager.gridDict[cellIndex].Add(thisPath);
                pathIndex = PathManager.gridDict[cellIndex].Count - 1;
            }
        }

        PathManager.gridDict[cellIndex][pathIndex].points.Add(point);
        pathPoints.Add(point);
    }

    public bool IsPointTooClose(Vector3 point)
{
    Vector2Int cellIndex = GetCellIndex(point);

    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            Vector2Int neighborCell = new Vector2Int(cellIndex.x + x, cellIndex.y + y);
            if (!PathManager.gridDict.ContainsKey(neighborCell))
                continue;

            foreach (var path in PathManager.gridDict[neighborCell])
            {
                if (path.id == thisPath.id)
                    continue;

                if (path.points.Any(existingPoint => Vector3.Distance(existingPoint, point) < PathManager.Instance.minProximity))
                    return true;
            }
        }
    }

    return false;
}

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
}
