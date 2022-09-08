using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardGenerator))]
public class BoardGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoardGenerator boardGeneratorScript = (BoardGenerator) target;

        if (GUI.changed)
        {
            boardGeneratorScript.generateBoard();
        }

        if (GUILayout.Button("Do The Thing"))
        {
            boardGeneratorScript.generateBoard();
        }
    }
}
