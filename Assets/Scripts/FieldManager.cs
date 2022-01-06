using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager
{
    static public int FieldZ { get; } = 101;
    static public int FieldX { get; } = 101;

    static public bool[,] FieldStatus { get; private set; } = new bool[FieldZ, FieldX];

    static void InitializeStatus()
    {
        FieldStatus = new bool[FieldZ, FieldX];
    }

    static public void SetStatus(int[] point)
    {
        FieldStatus[point[0], point[1]] = true;
    }

    static public void Setup(bool mute)
    {
        InitializeStatus();

        var generator = SettingSystem.Generator;

        switch (generator)
        {
            case (MapGenerator.mosaic):
                MG_Mosaic.Setup(mute);
                break;
            case (MapGenerator.maze):
                MG_Maze.Setup(mute);
                break;
            case (MapGenerator.random):
                MG_Random.Setup(mute);
                break;
            case (MapGenerator.fixedmap):
                MG_Fixed.Setup(mute);
                break;
            default:
                break;
        }
    }
}
