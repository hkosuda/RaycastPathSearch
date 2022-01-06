using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PathSolver
{
    raycast,
    astar,
}

public enum MapGenerator
{
    random,
    mosaic,
    maze,
    fixedmap,
}

public class SettingSystem : MonoBehaviour
{
    static public PathSolver Solver { get; private set; } = PathSolver.raycast;
    static public MapGenerator Generator { get; private set; } = MapGenerator.mosaic;

    static public float Speed { get; private set; } = 10.0f;
    static public int Seed { get; private set; } = -1;

    Text infoText;
    Dropdown solverDropdown;
    Dropdown generatorDropdown;
    InputField speedInput;
    InputField seedInput;
    Button beginButton;

    void Start()
    {
        var info = gameObject.transform.GetChild(0);
        var generator = gameObject.transform.GetChild(1);
        var solver = gameObject.transform.GetChild(2);
        var speed = gameObject.transform.GetChild(3);
        var seed = gameObject.transform.GetChild(4);
        var begin = gameObject.transform.GetChild(5);

        // info text
        infoText = info.gameObject.GetComponent<Text>();

        // generator dropdown
        generatorDropdown = generator.gameObject.GetComponent<Dropdown>();
        generatorDropdown.onValueChanged.AddListener(SetGenerator);

        // solver dropdown
        solverDropdown = solver.gameObject.GetComponent<Dropdown>();
        solverDropdown.onValueChanged.AddListener(SetSolver);

        // speed
        speedInput = speed.gameObject.GetComponent<InputField>();
        speedInput.onValueChanged.AddListener(SetSpeed);

        // seed
        seedInput = seed.gameObject.GetComponent<InputField>();
        seedInput.onValueChanged.AddListener(SetSeed);

        beginButton = begin.gameObject.GetComponent<Button>();
        beginButton.onClick.AddListener(BeginSimulation);

        UpdateText();
    }

    void SetGenerator(int option)
    {
        switch (option)
        {
            case (0):
                Generator = MapGenerator.mosaic;
                break;
            case (1):
                Generator = MapGenerator.maze;
                break;
            case (2):
                Generator = MapGenerator.random;
                break;
            case (3):
                Generator = MapGenerator.fixedmap;
                break;
            default:
                Generator = MapGenerator.random;
                break;
        }

        if (Generator == MapGenerator.random)
        {
            Solver = PathSolver.raycast;
        }

        UpdateText();
    }

    void SetSolver(int option)
    {
        switch (option)
        {
            case (0):
                Solver = PathSolver.raycast;
                break;
            case (1):
                Solver = PathSolver.astar;
                break;
            default:
                Solver = PathSolver.raycast;
                break;
        }

        if (Solver == PathSolver.astar)
        {
            if (Generator == MapGenerator.random)
            {
                Generator = MapGenerator.mosaic;
            }
        }

        UpdateText();
    }

    void SetSpeed(string str)
    {
        if (float.TryParse(str, out var num))
        {
            if (num > 0)
            {
                Speed = num;
            }
        }

        UpdateText();
    }

    void SetSeed(string str)
    {
        if (int.TryParse(str, out var num))
        {
            if (num >= 0)
            {
                Seed = num;
            }

            else
            {
                Seed = -1;
            }
        }

        UpdateText();
    }

    void BeginSimulation()
    {
        UpdateText();

        Kernel.Begin(false);
    }

    void UpdateText()
    {
        switch (Generator)
        {
            case (MapGenerator.mosaic):
                generatorDropdown.value = 0;
                break;
            case (MapGenerator.maze):
                generatorDropdown.value = 1;
                break;
            case (MapGenerator.fixedmap):
                generatorDropdown.value = 3;
                break;
            case (MapGenerator.random):
                generatorDropdown.value = 2;
                break;
        }

        switch (Solver)
        {
            case (PathSolver.raycast):
                solverDropdown.value = 0;
                break;
            case (PathSolver.astar):
                solverDropdown.value = 1;
                break;
        }

        speedInput.text = Speed.ToString("F1");
        seedInput.text = Seed.ToString();
    }
}
