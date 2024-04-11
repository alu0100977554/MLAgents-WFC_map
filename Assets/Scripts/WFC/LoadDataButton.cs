using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LoadTiledMap))]
public class LoadDataButton : Editor
{
    public override void OnInspectorGUI()
    {
        // To get the base inspector
        DrawDefaultInspector();

        LoadTiledMap loadedTiledMap = (LoadTiledMap)target;

        if (GUILayout.Button("Load Data"))
        {
            loadedTiledMap.LoadData();
        }
    }
}