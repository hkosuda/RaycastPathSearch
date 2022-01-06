using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_Mosaic
{
    static int[,] boxPoints = new int[8, 2] {
        { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { 1, -1 }
    };

    static int[,] crossPoints = new int[4, 2] {
        { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 }
    };

    static int minBlockSize = 3;
    static int maxBlockSize = 7 + 1;

    static int totalBlocks = 1000;

    static int FieldX;
    static int FieldZ;

    static public void Setup(bool mute)
    {
        FieldX = FieldManager.FieldX;
        FieldZ = FieldManager.FieldZ;

        var _wall = Resources.Load<GameObject>("Wall");

        int counter = 0;

        while (true)
        {
            var randomPoint = GetRandomInitialPoint();

            if (randomPoint[0] < 0 || randomPoint[1] < 0) { return; }

            int blockSize = UnityEngine.Random.Range(minBlockSize, maxBlockSize);
            var pointList = new List<int[]>() { randomPoint };

            var blockPointList = new List<int[]>();

            for (int n = 0; n < blockSize; n++)
            {
                if (pointList.Count == 0) { UpdateStatus(blockPointList); break; }

                var baseIndex = UnityEngine.Random.Range(0, pointList.Count);
                var basePoint = pointList[baseIndex];

                pointList.RemoveAt(baseIndex);
                blockPointList.Add(basePoint);

                if (!mute)
                {
                    var position = new Vector3(basePoint[1], 0.0f, basePoint[0]);
                    var wall = Object.Instantiate(_wall, position, Quaternion.identity);

                    wall.transform.parent = Kernel.Root.transform;
                }

                counter++;
                if (counter >= totalBlocks) { UpdateStatus(blockPointList); return; }

                AddPoint(basePoint, blockPointList, pointList);
            }

            UpdateStatus(blockPointList);
        }
    }

    static int[] GetRandomInitialPoint()
    {
        var validPoints = new List<int[]>();

        for (int x = 0; x < FieldX; x++)
        {
            for (int z = 0; z < FieldZ; z++)
            {
                if (FieldManager.FieldStatus[z, x]) { continue; }

                var validation = true;

                for (int n = 0; n < 8; n++)
                {
                    int zz = z + boxPoints[n, 0];
                    int xx = x + boxPoints[n, 1];

                    if (zz < 0 || zz >= FieldZ) { continue; }
                    if (xx < 0 || xx >= FieldX) { continue; }

                    if (FieldManager.FieldStatus[zz, xx]) { validation = false; break; }
                }

                if (validation)
                {
                    validPoints.Add(new int[2] { z, x });
                }
            }
        }

        if (validPoints.Count == 0) { return new int[2] { -1, -1 }; }

        int index = UnityEngine.Random.Range(0, validPoints.Count);
        return validPoints[index];
    }

    static bool Contain(List<int[]> blockPointList, int[] point)
    {
        foreach (var p in blockPointList)
        {
            if (p[0] == point[0] && p[1] == point[1])
            {
                return true;
            }
        }

        return false;
    }

    static void UpdateStatus(List<int[]> blockPoints)
    {
        foreach (var point in blockPoints)
        {
            FieldManager.SetStatus(point);
        }
    }

    static void AddPoint(int[] basePoint, List<int[]> blockPointList, List<int[]> pointList)
    {
        for (int p = 0; p < 4; p++)
        {
            int zz = basePoint[0] + crossPoints[p, 0];
            int xx = basePoint[1] + crossPoints[p, 1];

            if (zz < 0 || zz >= FieldZ) { continue; }
            if (xx < 0 || xx >= FieldX) { continue; }

            if (FieldManager.FieldStatus[zz, xx]) { continue; }
            if (Contain(blockPointList, new int[2] { zz, xx })) { continue; }

            var validation = true;

            for (int pp = 0; pp < 8; pp++)
            {
                int zzz = zz + boxPoints[pp, 0];
                int xxx = xx + boxPoints[pp, 1];

                if (zzz < 0 || zzz >= FieldZ) { continue; }
                if (xxx < 0 || xxx >= FieldX) { continue; }

                if (FieldManager.FieldStatus[zzz, xxx]) { validation = false; break; }
            }

            if (validation)
            {
                pointList.Add(new int[2] { zz, xx });
            }
        }
    }
}

public class MG_Maze
{
    static public void Setup(bool mute)
    {
        int row = Mathf.RoundToInt((FieldManager.FieldZ + 1) / 2);
        int col = Mathf.RoundToInt((FieldManager.FieldX + 1) / 2);

        var maze = MazeGenerator.GenerateMaze(row, col);

        var _wall = Resources.Load<GameObject>("Wall");

        for (int c = 0; c < maze.GetLength(1); c++)
        {
            for (int r = 0; r < maze.GetLength(0); r++)
            {
                if (!maze[r, c]) { continue; }

                if (!mute)
                {
                    var position = new Vector3(c, 0.0f, r);
                    var wall = Object.Instantiate(_wall, position, Quaternion.identity);

                    wall.transform.parent = Kernel.Root.transform;
                }

                FieldManager.SetStatus(new int[2] { r, c });
            }
        }
    }
}

public class MG_Random
{
    static int maxBlocks = 200;

    static float sizeMin = 1.0f;
    static float sizeMax = 5.0f;

    static public void Setup(bool mute)
    {
        var _wall = Resources.Load<GameObject>("Wall");

        float fieldZ = FieldManager.FieldZ;
        float fieldX = FieldManager.FieldX;

        for (int block = 0; block < maxBlocks; block++)
        {
            float rotY = UnityEngine.Random.Range(0.0f, 360.0f);
            float sizeZ = UnityEngine.Random.Range(sizeMin, sizeMax);
            float sizeX = UnityEngine.Random.Range(sizeMin, sizeMax);

            float radius = Mathf.Max(new float[2] { sizeZ, sizeX }) + 1.0f;
            var direction = new Vector3(0.0f, -1.0f, 0.0f);

            var roopOut = true;

            for (int roopB = 0; roopB < 10000; roopB++)
            {
                float rz = UnityEngine.Random.Range(0.0f, fieldZ);
                float rx = UnityEngine.Random.Range(0.0f, fieldX);

                var origin = new Vector3(rx, 10.0f, rz);

                Physics.SphereCast(origin, radius, direction, out RaycastHit hit, 20.0f, 1 << 6);

                if (hit.collider == null)
                {
                    var position = new Vector3(rx, 0.0f, rz);
                    var wall = Object.Instantiate(_wall, position, Quaternion.Euler(0.0f, rotY, 0.0f));
                    wall.transform.localScale = new Vector3(sizeX, 0.5f, sizeZ);

                    wall.transform.parent = Kernel.Root.transform;
                    roopOut = false;
                    break;
                }
            }

            if (roopOut) { break; }
        }
        
    }
}

public class MG_Fixed
{
    static public void Setup(bool mute)
    {
        var _wall = Resources.Load<GameObject>("Wall");

        float fieldZ = FieldManager.FieldZ;
        float fieldX = FieldManager.FieldX;

        var points = new List<int[]>
        {
            new int[2] { 11, 11 },
            new int[2] { 11, 12 },
            new int[2] { 11, 13 },
            new int[2] { 11, 15 },
            new int[2] { 11, 16 },
            new int[2] { 11, 17 },

            new int[2] { 12, 11 },
            new int[2] { 12, 17 },

            new int[2] { 13, 11 },
            new int[2] { 13, 12 },
            new int[2] { 13, 13 },
            new int[2] { 13, 14 },
            new int[2] { 13, 15 },
            new int[2] { 13, 16 },
            new int[2] { 13, 17 },
        };

        foreach(var p in points)
        {
            var position = new Vector3(p[1], 0.0f, p[0]);
            var wall = Object.Instantiate(_wall, position, Quaternion.identity);

            wall.transform.parent = Kernel.Root.transform;
        }
    }
}
