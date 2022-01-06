using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Moving moving;

    static public EventHandler<bool> SolverEnd { get; set; }

    static public Stopwatch Sw { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        moving = GameObject.FindWithTag("Unit").GetComponent<Moving>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);

            if (hit.collider == null) { return; }
            if (hit.collider.gameObject.tag != "Ground") { return; }

            var solver = SettingSystem.Solver;

            List<Vector3> path;

            Sw = new Stopwatch();
            Sw.Start();

            switch (solver)
            {
                case (PathSolver.raycast):
                    path = RaycastPathSearch.GetPath(moving.transform.position, hit.point, 1 << 6);
                    break;
                case (PathSolver.astar):
                    path = AStar.GetRoute(FieldManager.FieldStatus, moving.transform.position, hit.point, true, 0.0f);
                    break;
                default:
                    path = RaycastPathSearch.GetPath(moving.transform.position, hit.point, 1 << 6);
                    break;
            }

            Sw.Stop();
            SolverEnd?.Invoke(null, false);

            moving.SetPath(path);
        }
    }
}
