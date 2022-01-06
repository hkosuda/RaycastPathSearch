using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceObject : MonoBehaviour
{
    static public Dictionary<GameObject, BounceObject> BounceObjectList { get; private set; } = new Dictionary<GameObject, BounceObject>();

    float halfSizeZ;
    float halfSizeX;

    float[] dzdx = new float[2] { 0.5f, 0.5f };

    public Dictionary<Direction, List<Corner>> BounceCorners { get; private set; }
    public Dictionary<Corner, Vector3> BouncePositions { get; private set; }

    public Dictionary<Corner, bool> CheckStatus { get; private set; } = new Dictionary<Corner, bool>()
        {
            { Corner.t_r, true },
            { Corner.t_l, true },
            { Corner.b_r, true },
            { Corner.b_l, true },
        };

    public Dictionary<Corner, bool> NodeStatus { get; private set; } = new Dictionary<Corner, bool>()
        {
            { Corner.t_r, true },
            { Corner.t_l, true },
            { Corner.b_r, true },
            { Corner.b_l, true },
        };

    public enum Corner
    {
        t_r,
        t_l,
        b_r,
        b_l,
    }

    public enum Direction
    {
        // +z : top,    -z : bottom
        // +x : right,  -x : left
        t,      // top
        t_r,    // top-right
        t_l,    // top-left
        b,      // bottom
        b_r,    // bottom-right
        b_l,    // bottom-left
        l,      // left
        r,      // right
        none,
    }

    // Start is called before the first frame update
    void Start()
    {
        halfSizeZ = gameObject.transform.localScale.z / 2;
        halfSizeX = gameObject.transform.localScale.x / 2;

        SetupBounce();

        BounceObjectList.Add(gameObject, this);

        RaycastPathSearch.PathSearchBegin += InitializeNodeStatus;
        RaycastPathSearch.PathSearchRoopBegin += InitializeCheckStatus;
    }

    public List<Corner> GetBounceCorners(Vector3 origin)
    {
        var dir = GetDirection(origin);
        var bounce = BounceCorners[dir];

        return bounce;

        //return new List<Corner> { Corner.t_r, Corner.t_l, Corner.b_r, Corner.b_l };
    }

    void SetupBounce()
    {
        var rotY = gameObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        var dz = halfSizeZ + dzdx[0];
        var dx = halfSizeX + dzdx[1];

        var rotPositions = GetRotPositions(dz, dx, rotY);

        var pos = gameObject.transform.position;

        BouncePositions = new Dictionary<Corner, Vector3>()
        {
            { Corner.t_r, new Vector3(pos.x + rotPositions[0, 1], 0.0f, pos.z + rotPositions[0, 0]) },
            { Corner.t_l, new Vector3(pos.x + rotPositions[1, 1], 0.0f, pos.z + rotPositions[1, 0]) },
            { Corner.b_r, new Vector3(pos.x + rotPositions[2, 1], 0.0f, pos.z + rotPositions[2, 0]) },
            { Corner.b_l, new Vector3(pos.x + rotPositions[3, 1], 0.0f, pos.z + rotPositions[3, 0]) },
        };

        BounceCorners = new Dictionary<Direction, List<Corner>>()
        {
            // top          : top-right & top-left
            { Direction.t, new List<Corner>(){ Corner.t_r,  Corner.t_l } },

            // top-right    : top-left  & bottom-right
            { Direction.t_r, new List<Corner>(){ Corner.t_l, Corner.b_r } },

            // top-left     : top-right & bottom-left
            { Direction.t_l, new List<Corner>(){ Corner.t_r, Corner.b_l } },

            // bottom       : bottom-right & bottom-left
            { Direction.b, new List<Corner>(){ Corner.b_r, Corner.b_l } },

            // bottom-right : bottom-left & top-right
            { Direction.b_r, new List<Corner>(){ Corner.b_l, Corner.t_r } },

            // bottom-left  : bottom-right & top-left
            { Direction.b_l, new List<Corner>(){ Corner.b_r, Corner.t_l } },

            // right        : top-right & bottom-right
            { Direction.r, new List<Corner>(){ Corner.t_r, Corner.b_r } },

            // left         : top-left & bottom-left
            { Direction.l, new List<Corner>(){ Corner.t_l, Corner.b_l } },
        };

        //
        // function 
        float[,] GetRotPositions(float dz, float dx, float rotY)
        {
            var radius = Mathf.Sqrt(Mathf.Pow(dz, 2.0f) + Mathf.Pow(dx, 2.0f));

            var theta = new float[4];
            theta[0] = Mathf.Atan2(dx, dz);   // top-right
            theta[1] = Mathf.Atan2(-dx, dz);   // top-left
            theta[2] = Mathf.Atan2(dx, -dz);  // bottom-right
            theta[3] = Mathf.Atan2(-dx, -dz); // bottom-left

            var rotPositions = new float[4, 2];
            for (int n = 0; n < 4; n++)
            {
                var pos = Rot(radius, theta[n], rotY);
                rotPositions[n, 0] = pos[0];
                rotPositions[n, 1] = pos[1];
            }

            return rotPositions;

            // function
            float[] Rot(float radius, float theta, float rotY)
            {
                float dzz = radius * Mathf.Cos(theta + rotY);
                float dxx = radius * Mathf.Sin(theta + rotY);

                return new float[2] { dzz, dxx };
            }
        }
    }

    Direction GetDirection(Vector3 origin)
    {
        var dx = origin.x - gameObject.transform.position.x;
        var dz = origin.z - gameObject.transform.position.z;

        var theta = gameObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        var dm = dz * Mathf.Cos(theta) + dx * Mathf.Sin(theta);
        var dl = -dz * Mathf.Sin(theta) + dx * Mathf.Cos(theta);

        // top
        if (dm > halfSizeZ)
        {
            // right
            if (dl > halfSizeX)
            {
                return Direction.t_r;
            }

            // left
            else if (dl < -halfSizeX)
            {
                return Direction.t_l;
            }

            // mid
            else
            {
                return Direction.t;
            }
        }

        // bottom
        else if (dm < -halfSizeZ)
        {
            // right
            if (dl > halfSizeX)
            {
                return Direction.b_r;
            }

            // left
            else if (dl < -halfSizeX)
            {
                return Direction.b_l;
            }

            // mid
            else
            {
                return Direction.b;
            }
        }

        // mid
        else
        {
            // right
            if (dl > halfSizeX)
            {
                return Direction.r;
            }

            // left
            else if (dl < -halfSizeX)
            {
                return Direction.l;
            }

            // mid
            else
            {
                return Direction.none;
            }
        }
    }

    void InitializeNodeStatus(object obj, bool mute)
    {
        NodeStatus[Corner.t_r] = true;
        NodeStatus[Corner.t_l] = true;
        NodeStatus[Corner.b_r] = true;
        NodeStatus[Corner.b_l] = true;
    }

    void InitializeCheckStatus(object obj, bool mute)
    {
        CheckStatus[Corner.t_r] = true;
        CheckStatus[Corner.t_l] = true;
        CheckStatus[Corner.b_r] = true;
        CheckStatus[Corner.b_l] = true;
    }

    private void OnDestroy()
    {
        BounceObjectList.Remove(gameObject);

        RaycastPathSearch.PathSearchBegin -= InitializeNodeStatus;
        RaycastPathSearch.PathSearchRoopBegin -= InitializeCheckStatus;
    }
}
