using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    static readonly int[,] crossPoints = new int[4, 2] { { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 } };
    static int[,] boxPoints = new int[8, 2] { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { 1, -1 } };

    static public List<Vector3> GetRoute(bool[,] field, Vector3 startPosition, Vector3 goalPosition, bool diagMoving = false, float height = 0.0f)
    {
        var start = new int[2] { Mathf.RoundToInt(startPosition.z), Mathf.RoundToInt(startPosition.x) };
        var goal = new int[2] { Mathf.RoundToInt(goalPosition.z), Mathf.RoundToInt(goalPosition.x) };

        int row = field.GetLength(0);
        int col = field.GetLength(1);

        if (Same(start, goal)) { return new List<Vector3>() { new Vector3(start[1], height, start[0]) }; }

        var node = GetInitialNode(field, goal);
        var cost = GetInitialCost(field, goal);

        // path search direction : goal -> start
        var openNodePointList = new List<int[]> { goal };

        bool reached = false;
        for (int roop = 0; roop < int.MaxValue; roop++)
        {
            if (openNodePointList.Count == 0) { return new List<Vector3>() { new Vector3(start[1], height, start[0]) }; }

            int baseIndex = GetBaseIndex(cost, openNodePointList);

            var basePoint = openNodePointList[baseIndex];
            openNodePointList.RemoveAt(baseIndex);

            if (diagMoving)
            {
                for (int n = 0; n < 8; n++)
                {
                    int r = basePoint[0] + boxPoints[n, 0];
                    int c = basePoint[1] + boxPoints[n, 1];

                    if (!CheckRC(r, c, row, col)) { continue; }
                    if (node[r, c, 2] > 0) { continue; }

                    var targetPoint = new int[2] { r, c };
                    openNodePointList.Add(targetPoint);

                    node[r, c, 0] = basePoint[0];
                    node[r, c, 1] = basePoint[1];
                    node[r, c, 2] = 1;

                    cost[r, c, 0] = cost[basePoint[0], basePoint[1], 0] + Magnitude(start, targetPoint);
                    cost[r, c, 1] = cost[r, c, 0] + Magnitude(start, targetPoint);

                    if (start[0] == r && start[1] == c) { reached = true; break; }
                }
            }

            else
            {
                for (int n = 0; n < 4; n++)
                {
                    int r = basePoint[0] + crossPoints[n, 0];
                    int c = basePoint[1] + crossPoints[n, 1];

                    if (!CheckRC(r, c, row, col)) { continue; }
                    if (node[r, c, 2] > 0) { continue; }

                    openNodePointList.Add(new int[2] { r, c });

                    node[r, c, 0] = basePoint[0];
                    node[r, c, 1] = basePoint[1];
                    node[r, c, 2] = 1;

                    cost[r, c, 0] = cost[basePoint[0], basePoint[1], 0] + 1;
                    cost[r, c, 1] = cost[r, c, 0] + Mathf.Abs(start[0] - r) + Mathf.Abs(start[1] - c);

                    if (start[0] == r && start[1] == c) { reached = true; break; }
                }
            }

            if (reached) { break; }
        }

        return GenerateRoute(node, start, height);

        //
        // functions
        static List<Vector3> GenerateRoute(int[,,] node, int[] start, float height)
        {
            var route = new List<int[]>();
            var point = new int[2] { start[0], start[1] };

            while (true)
            {
                if (point[0] < 0 || point[1] < 0) { break; }
                route.Add(new int[2] { point[0], point[1] });

                int r = point[0];
                int c = point[1];

                point[0] = node[r, c, 0];
                point[1] = node[r, c, 1];
            }

            var vec3Route = new List<Vector3>();
            foreach (var p in route)
            {
                vec3Route.Add(new Vector3(p[1], height, p[0]));
            }

            return vec3Route;
        }
    }

    //
    // utility functions
    static bool Same(int[] point1, int[] point2)
    {
        if (point1[0] == point2[0] && point1[1] == point2[1])
        {
            return true;
        }

        return false;
    }

    static int[,,] GetInitialNode(bool[,] field, int[] goal)
    {
        int row = field.GetLength(0);
        int col = field.GetLength(1);

        var node = new int[row, col, 3];

        for (int c = 0; c < col; c++)
        {
            for (int r = 0; r < row; r++)
            {
                // false : path, true : wall
                if (field[r, c])
                {
                    node[r, c, 2] = 2;
                }
            }
        }

        // 0, 1 : parent's position / 2 : node status
        node[goal[0], goal[1], 0] = -1;
        node[goal[0], goal[1], 1] = -1;
        node[goal[0], goal[1], 2] = 1;

        return node;
    }

    static float[,,] GetInitialCost(bool[,] field, int[] goal)
    {
        int row = field.GetLength(0);
        int col = field.GetLength(1);

        var cost = new float[row, col, 2];

        cost[goal[0], goal[1], 0] = 0.0f;
        cost[goal[0], goal[1], 1] = 1.0f;

        return cost;
    }

    static int GetBaseIndex(float[,,] cost, List<int[]> openNodePointList)
    {
        float minCost = float.MaxValue;
        int minCostIndex = 0;

        for (int n = 0; n < openNodePointList.Count; n++)
        {
            var point = openNodePointList[n];
            var c_val = cost[point[0], point[1], 1];

            if (c_val > minCost) { continue; }

            minCost = c_val;
            minCostIndex = n;
        }

        return minCostIndex;
    }

    static bool CheckRC(int r, int c, int row, int col)
    {
        if (r < 0 || r >= row) { return false; }
        if (c < 0 || c >= col) { return false; }

        return true;
    }

    static float Magnitude(int[] point1, int[] point2)
    {
        return Mathf.Sqrt(Mathf.Pow((point1[0] - point2[0]), 2) + Mathf.Pow((point1[1] - point2[1]), 2));
    }
}
