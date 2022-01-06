using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastPathSearch
{
    static public EventHandler<bool> PathSearchBegin { get; set; }
    static public EventHandler<bool> PathSearchRoopBegin { get; set; }

    class Node
    {
        public Node parentNode;
        public Vector3 position;
        public float accumulativeCost;
        public float heulisticCost;

        public Node(Node parentNode, Vector3 position, float accumulativeCost, float heulisticCost)
        {
            this.parentNode = parentNode;
            this.position = position;
            this.accumulativeCost = accumulativeCost;
            this.heulisticCost = heulisticCost;
        }
    }

    class Target
    {
        public BounceObject bounceObject;
        public BounceObject.Corner corner;

        public Target(BounceObject bounceObject, BounceObject.Corner corner)
        {
            this.bounceObject = bounceObject;
            this.corner = corner;
        }
    }

    static public List<Vector3> GetPath(Vector3 start, Vector3 goal, int layerMask)
    {
        PathSearchBegin?.Invoke(null, false);

        start = new Vector3(start.x, 0.0f, start.z);
        goal = new Vector3(goal.x, 0.0f, goal.z);

        var openNodeList = GetInitialNodeList(start, goal);

        while (true)
        {
            PathSearchRoopBegin?.Invoke(null, false);

            if (openNodeList.Count == 0) { return new List<Vector3>() { start }; }

            var baseNodeIndex = GetBaseNodeIndex(openNodeList);
            var baseNode = openNodeList[baseNodeIndex];

            openNodeList.RemoveAt(baseNodeIndex);

            // rancast to goal
            var dxA = goal - baseNode.position;
            var rayA = new Ray
            {
                origin = baseNode.position, 
                direction = dxA.normalized,
            };

            Physics.Raycast(rayA, out RaycastHit hitA, dxA.magnitude, layerMask);
            Debug.DrawRay(rayA.origin, rayA.direction * dxA.magnitude, Color.red, 60.0f, false);

            if (hitA.collider == null) 
            {
                return UnpackNodeInfo(baseNode, goal);
            }

            var bounceObjectA = BounceObject.BounceObjectList[hitA.collider.gameObject];
            var bounceCornersA = bounceObjectA.GetBounceCorners(baseNode.position);

            var targetObjectList = new List<BounceObject>();
            var targetCornerList = new List<BounceObject.Corner>();

            AddTargetObjects(targetObjectList, targetCornerList, bounceObjectA, bounceCornersA);

            while(true)
            {
                if (targetObjectList.Count == 0) { break; }

                // raycast to target
                var targetObject = targetObjectList[0];
                var targetCorner = targetCornerList[0];

                targetObjectList.RemoveAt(0);
                targetCornerList.RemoveAt(0);

                var targetPosition = targetObject.BouncePositions[targetCorner];
                targetObject.CheckStatus[targetCorner] = false;

                // raycast
                var dxB = targetPosition - baseNode.position;
                var rayB = new Ray()
                {
                    origin = baseNode.position,
                    direction = dxB.normalized,
                };

                Physics.Raycast(rayB, out RaycastHit hitB, dxB.magnitude, layerMask);
                Debug.DrawRay(rayB.origin, rayB.direction * dxB.magnitude, Color.blue, 60.0f, false);

                // no hit
                if (hitB.collider == null)
                {
                    if (!targetObject.NodeStatus[targetCorner]) { continue; }
                    
                    targetObject.NodeStatus[targetCorner] = false;

                    float accumulativeCost = baseNode.accumulativeCost + (targetPosition - baseNode.position).magnitude;
                    float heulisticCost = accumulativeCost + (goal - targetPosition).magnitude;

                    openNodeList.Add(new Node(baseNode, targetPosition, accumulativeCost, heulisticCost));
                }

                // hit
                else
                {
                    var bounceObjectB = BounceObject.BounceObjectList[hitB.collider.gameObject];
                    var bounceCornersB = bounceObjectB.GetBounceCorners(baseNode.position);

                    AddTargetObjects(targetObjectList, targetCornerList, bounceObjectB, bounceCornersB);
                }
            }
        }

        return new List<Vector3>() { start };

        //
        // function
        int GetBaseNodeIndex(List<Node> openNodeList)
        {
            var minCost = float.MaxValue;
            var minCostIndex = 0;

            for (int n = 0; n < openNodeList.Count; n++)
            {
                var cost = openNodeList[n].heulisticCost;
                if (cost > minCost) { continue; }

                minCost = cost;
                minCostIndex = n;
            }

            return minCostIndex;
        }

        List<Node> GetInitialNodeList(Vector3 start, Vector3 goal)
        {
            var openNodeList = new List<Node>()
            { 
                new Node(null, start, 0.0f, (goal - start).magnitude)
            };

            return openNodeList;
        }

        void AddTargetObjects(List<BounceObject> targetObjectList, List<BounceObject.Corner> targetCornerList, 
            BounceObject bounceObject, List<BounceObject.Corner> corners)
        {
            foreach (var corner in corners)
            {
                if (bounceObject.CheckStatus[corner])
                {
                    targetObjectList.Add(bounceObject);
                    targetCornerList.Add(corner);
                }
            }
        }

        List<Vector3> UnpackNodeInfo(Node baseNode, Vector3 goal)
        {
            var path = new List<Vector3>();
            path.Add(goal);

            var parent = baseNode;

            while (true)
            {
                if (parent == null) { break; }

                path.Add(parent.position);

                parent = parent.parentNode;
            }

            path.Reverse();

            foreach (var p in path)
            {
                Debug.Log(p);
            }

            return path;
        }
    }
}
