using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGenerator : MonoBehaviour
{
    public GameObject objPrefab;
    public Transform objParent;
    public Transform playerTransform;
    public int worldSize = 5000;
    public int objRange = 250;
    public int objDensity = 3;
    public int maxObjects = 5000;
    public int objCycle = 1000;
    public int batchSize = 250;

    private HashSet<Vector2> objPositions = new HashSet<Vector2>();
    private Queue<GameObject> objPool = new Queue<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>();

    void Start()
    {
        StartCoroutine(GenerateObjects());
    }

    IEnumerator GenerateObjects()
    {
        while (true)
        {
            int objsAdded = 0;

            for (int i = 0; i < objCycle; i++)
            {
                if (objPositions.Count >= maxObjects)
                    break;

                Vector2 newPosition = GenerateRandomPosition();
                if (!objPositions.Contains(newPosition) && IsPositionValid(newPosition))
                {
                    objPositions.Add(newPosition);
                    GameObject obj = GetOrCreateObject();
                    obj.transform.position = newPosition;
                    activeObjects.Add(obj);
                    objsAdded++;

                    if (objsAdded % batchSize == 0)
                        yield return null;
                }
            }

            RemoveDistantObjects();
            yield return new WaitForSeconds(0.5f);
        }
    }

    Vector2 GenerateRandomPosition()
    {
        return (Vector2)playerTransform.position + new Vector2(
            Random.Range(-objRange, objRange),
            Random.Range(-objRange, objRange));
    }

    bool IsPositionValid(Vector2 position)
    {
        if (Mathf.Abs(position.x) >= worldSize || Mathf.Abs(position.y) >= worldSize)
            return false;

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, objDensity);
        if (nearbyColliders.Length > 0)
            return false;

        foreach (Vector2 existingPos in objPositions)
        {
            if (Vector2.Distance(position, existingPos) < objDensity)
                return false;
        }

        return true;
    }


    GameObject GetOrCreateObject()
    {
        if (objPool.Count > 0)
        {
            GameObject obj = objPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return Instantiate(objPrefab, Vector2.zero, Quaternion.identity, objParent);
        }
    }

    void RemoveDistantObjects()
    {
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeObjects[i];
            if (Vector2.Distance(playerTransform.position, obj.transform.position) > objRange)
            {
                obj.SetActive(false);
                objPool.Enqueue(obj);
                objPositions.Remove(obj.transform.position);
                activeObjects.RemoveAt(i);
            }
        }
    }
}
