using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveTiledMap))]
public class SaveDataButton : Editor
{
    public override void OnInspectorGUI()
    {
        // To get the base inspector
        DrawDefaultInspector();

        SaveTiledMap savedTiledMap = (SaveTiledMap)target;

        if (GUILayout.Button("Save Data"))
        {
            savedTiledMap.SaveData();
        }
    }
}
