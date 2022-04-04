using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] GameObject cellObject;
    [SerializeField] GameObject discObject;
    private int size = 8;

    CellStatus[,] cellStatuses;
    GameObject[,] cells;
    void Start()
    {
        cellStatuses = new CellStatus[size, size];
        cells = new GameObject[size, size];
        GenerateCellObjects();
        PutDisc(size / 2 - 1, size / 2 - 1);
    }

    void GenerateCellObjects()
    {
        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                Vector3 relativePos = new Vector3(x, 0, y);
                GameObject cell = Instantiate(cellObject, transform.position + relativePos, Quaternion.identity, transform);
                cell.name = "Cell(" + x + ", " + y + ")";
                cells[x, y] = cell;
            }
        }
    }

    void PutDisc(int x, int y)
    {
        GameObject cell = cells[x, y];
        GameObject disc = Instantiate(discObject, cell.transform.position + Vector3.up * 0.2f, Quaternion.identity, transform);
    }

    void Update()
    {
        
    }
}
