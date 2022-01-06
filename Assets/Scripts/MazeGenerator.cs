using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator
{
    static Vector2Int[] direction = new Vector2Int[4]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    class Node
    {
        public enum Status
        {
            open,
            close,
            none
        }

        public Status status;
        public Vector2Int position;
        public Node parentNode;
        public bool[] walls;
        public bool[] investigated;
    }

    static public bool[,] GenerateMaze(int row, int col)
    {
        Node[,] nodes = new Node[row, col];
        List<Vector2Int> noneNodesIndexList = new List<Vector2Int>();

        // Initialization
        Random.InitState(System.DateTime.Now.Millisecond);
        //Random.InitState(1000);

        for (int x = 0; x < col; x++)
        {
            for (int y = 0; y < row; y++)
            {
                Node node = new Node
                {
                    status = Node.Status.none,
                    position = new Vector2Int(x, y),
                    parentNode = null,
                    walls = new bool[4] { false, false, false, false },
                    investigated = new bool[4] { false, false, false, false }
                };

                nodes[y, x] = node;

                if (x == 0 || x == col - 1) continue;
                if (y == 0 || y == row - 1) continue;

                noneNodesIndexList.Add(new Vector2Int(x, y));
            }
        }

        for (int x = 0; x < col - 1; x++)
        {
            // walls[1] ... right
            nodes[0, x].walls[1] = true;
            nodes[0, x].status = Node.Status.close;

            nodes[row - 1, x].walls[1] = true;
            nodes[row - 1, x].status = Node.Status.close;
        }

        for (int y = 0; y < row - 1; y++)
        {
            // walls[0] ... upper
            nodes[y, 0].walls[0] = true;
            nodes[y, 0].status = Node.Status.close;

            nodes[y, col - 1].walls[0] = true;
            nodes[y, col - 1].status = Node.Status.close;
        }

        // choose start node
        for (int roopA = 0; roopA < 100000; roopA++)
        {
            int openNodesRemain = noneNodesIndexList.Count;
            if (openNodesRemain == 0) break;

            Vector2Int basePosition = noneNodesIndexList[Random.Range(0, openNodesRemain)];
            nodes[basePosition.y, basePosition.x].status = Node.Status.open;
            nodes[basePosition.y, basePosition.x].parentNode = null;

            // search
            for (int roopB = 0; roopB < 10000; roopB++)
            {
                List<int> validIndexList = new List<int>();
                // Debug.Log(string.Concat("A:", roopA.ToString(), "B:",roopB.ToString()));

                for (int m = 0; m < 4; m++)
                {
                    if (nodes[basePosition.y, basePosition.x].investigated[m]) continue;
                    validIndexList.Add(m);
                }

                if (validIndexList.Count == 0)
                {
                    basePosition = nodes[basePosition.y, basePosition.x].parentNode.position;
                    continue;
                }

                int index = validIndexList[Random.Range(0, validIndexList.Count)];
                Vector2Int next_pos = basePosition + direction[index];

                nodes[basePosition.y, basePosition.x].investigated[index] = true;

                if (nodes[next_pos.y, next_pos.x].status == Node.Status.close)
                {
                    nodes[basePosition.y, basePosition.x].walls[index] = true;

                    for (int roopC = 0; roopC < 10000; roopC++)
                    {
                        nodes[basePosition.y, basePosition.x].status = Node.Status.close;
                        noneNodesIndexList.Remove(basePosition);

                        if (nodes[basePosition.y, basePosition.x].parentNode == null) break;

                        Vector2Int diff = nodes[basePosition.y, basePosition.x].parentNode.position - nodes[basePosition.y, basePosition.x].position;

                        for (int n = 0; n < 4; n++)
                        {
                            if (diff != direction[n]) continue;
                            nodes[basePosition.y, basePosition.x].walls[n] = true;
                            break;
                        }

                        basePosition = nodes[basePosition.y, basePosition.x].parentNode.position;
                    }

                    break;
                }

                else if (nodes[next_pos.y, next_pos.x].status == Node.Status.open)
                {
                    basePosition = nodes[basePosition.y, basePosition.x].parentNode.position;
                }

                else if (nodes[next_pos.y, next_pos.x].status == Node.Status.none)
                {
                    nodes[next_pos.y, next_pos.x].investigated[(index + 2) % 4] = true;
                    nodes[next_pos.y, next_pos.x].parentNode = nodes[basePosition.y, basePosition.x];
                    nodes[next_pos.y, next_pos.x].status = Node.Status.open;
                    basePosition = next_pos;
                }
            }

            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    if (nodes[j, i].status == Node.Status.close) continue;

                    nodes[j, i].parentNode = null;
                    nodes[j, i].investigated = new bool[4] { false, false, false, false };
                    nodes[j, i].status = Node.Status.none;
                }
            }
        }

        return Unpack(nodes);
    }

    static bool[,] Unpack(Node[,] nodes)
    {
        int node_row = nodes.GetLength(0);
        int node_col = nodes.GetLength(1);

        int maze_row = 2 * node_row - 1;
        int maze_col = 2 * node_col - 1;

        bool[,] maze = new bool[maze_row, maze_col];

        for (int x = 0; x < node_col; x++)
        {
            for (int y = 0; y < node_row; y++)
            {
                Node node = nodes[y, x];
                maze[2 * y, 2 * x] = true;

                for (int n = 0; n < 4; n++)
                {
                    if (!node.walls[n]) continue;

                    Vector2Int spot = new Vector2Int(2 * x, 2 * y) + direction[n];

                    if (spot.x < 0 || spot.x > maze_col - 1) continue;
                    if (spot.y < 0 || spot.y > maze_row - 1) continue;

                    maze[spot.y, spot.x] = true;
                }
            }
        }

        return maze;
    }
}
