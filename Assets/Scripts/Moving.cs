using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moving : MonoBehaviour
{
    List<Vector3> path = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (path == null) { return; }
        if (path.Count == 0) { return; }

        float dt = Time.deltaTime;

        var currentPosition = gameObject.transform.position;
        var nextPosition = currentPosition;

        float speed = SettingSystem.Speed;

        float movingDistance = dt * speed;
        float movingDistanceRemain = movingDistance;

        List<int> removingPathIndexes = new List<int>();

        for (int n = 0; n < path.Count; n++)
        {
            float disp2Next = (path[n] - currentPosition).magnitude;

            if (movingDistanceRemain < disp2Next)
            {
                nextPosition = gameObject.transform.position + (path[n] - currentPosition).normalized * movingDistanceRemain;
                break;
            }

            else
            {
                movingDistanceRemain -= disp2Next;
                removingPathIndexes.Add(n);
                currentPosition = path[n];
                nextPosition = path[n];
            }
        }

        if (removingPathIndexes.Count > 0)
        {
            if (path.Count == 1)
            {
                path.Clear();
            }

            else
            {
                for (int n = removingPathIndexes.Count - 1; n > -1; n--)
                {
                    path.RemoveAt(removingPathIndexes[n]);
                }
            }

            removingPathIndexes.Clear();
        }

        gameObject.transform.position = nextPosition;
    }

    public void SetPath(List<Vector3> path)
    {
        this.path = path;
    }
}
