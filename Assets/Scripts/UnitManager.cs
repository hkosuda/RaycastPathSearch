using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager
{
    static public void Setup(bool mute)
    {
        var unit = GameObject.Find("Unit");

        unit.GetComponent<Moving>().SetPath(new List<Vector3>());

        var validPointList = new List<int[]>();

        for (int x = 0; x < FieldManager.FieldX; x++)
        {
            for (int z = 0; z < FieldManager.FieldZ; z++)
            {
                if (FieldManager.FieldStatus[z, x]) { continue; }
                validPointList.Add(new int[2] { z, x });
            }
        }

        if (validPointList.Count == 0) { return; }

        var randomIndex = UnityEngine.Random.Range(0, validPointList.Count);
        var unitPoint = validPointList[randomIndex];

        unit.transform.position = new Vector3(unitPoint[1], 0.0f, unitPoint[0]);
    }
}
