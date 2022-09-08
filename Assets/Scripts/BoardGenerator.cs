using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public GameObject CubesHolder;
    public GameObject CubePrefab;
    public GameObject LinePrefab;
    public GameObject whitePrison;
    public GameObject blackPrison;

    [Range(1, 20)]
    public int SizeBoard;

    private List<GameObject> xlines = new List<GameObject>();
    private List<GameObject> zlines = new List<GameObject>();

    public void generateBoard()
    {
        List<GameObject> allChildren = new List<GameObject>();

        foreach (Transform child in CubesHolder.transform)
        {
            allChildren.Add(child.gameObject);
        }

        foreach (GameObject child in allChildren)
        {
            DestroyImmediate(child);
        }

        for (int i = 0; i < SizeBoard; i++)
        {
            GameObject horizontalCube = Instantiate(CubePrefab, CubesHolder.transform, false);
            horizontalCube.transform.localPosition = new Vector3(i, 0, 0);
            for (int j = 0; j < SizeBoard; j++)
            {
                GameObject verticalCube = Instantiate(CubePrefab, CubesHolder.transform, false);
                verticalCube.transform.localPosition = new Vector3(i, 0, j);
            }
        }

        whitePrison.transform.localPosition = new Vector3(-2.5f, 0, SizeBoard + 0.5f);
        blackPrison.transform.localPosition = new Vector3(SizeBoard + 1.5f, 0, -1.5f);

        foreach (var line in xlines)
        {
            DestroyImmediate(line.gameObject);
        }
        xlines.Clear();
        for (int i = 0; i <= SizeBoard; i++)
        {
            float xCoordinate = -0.5f + i;
            GameObject line = Instantiate(LinePrefab, CubesHolder.transform);
            xlines.Add(line);
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.SetPosition(0, new Vector3(xCoordinate, 0.5f, -0.5f));
            lr.SetPosition(1, new Vector3(xCoordinate, 0.5f, SizeBoard - 0.5f));
        }

        foreach (var line in zlines)
        {
            DestroyImmediate(line.gameObject);
        }
        zlines.Clear();
        for (int i = 0; i <= SizeBoard; i++)
        {
            float zCoordinate = -0.5f + i;
            GameObject line = Instantiate(LinePrefab, CubesHolder.transform);
            zlines.Add(line);
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.SetPosition(0, new Vector3(-0.5f, 0.5f, zCoordinate));
            lr.SetPosition(1, new Vector3(SizeBoard - 0.5f, 0.5f, zCoordinate));
        }

    }

}
