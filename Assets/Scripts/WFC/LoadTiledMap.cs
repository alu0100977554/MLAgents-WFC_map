using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTiledMap : MonoBehaviour
{
    [SerializeField]
    private GameObject _modelMap;
    [HideInInspector]
    public int _gridSize;
    [HideInInspector]
    public int _nRows;
    [HideInInspector]
    public int _nColumns;
    public string[,] _tiledMap;

    public void LoadData()
    {
        TextAsset jsonContent = Resources.Load<TextAsset>("tiles/" + _modelMap.GetComponent<OverlapWFC>().GetInstanceID());
        Debug.Log("Loading from: D:/Programas/Unity/Repos/MLAgents-WFC_map/Assets/Resources/tiles/" + _modelMap.GetComponent<OverlapWFC>().GetInstanceID() + ".json");

        // Convert the information of the JSON file into an object
        LoadTiledMap modelMapInfo = JSONHelper.FromJson(jsonContent.text);
        GetComponent<OverlapWFC>().gridsize = modelMapInfo._gridSize;
        GetComponent<OverlapWFC>().width = modelMapInfo._nRows;
        GetComponent<OverlapWFC>().depth = modelMapInfo._nColumns;

        CreateMap(modelMapInfo);
    }

    public void CreateMap(LoadTiledMap modelMapInfo)
    {
        string[,] tiledMap = modelMapInfo._tiledMap;
        Transform tiles = GetComponent<OverlapWFC>().transform.GetChild(1).GetChild(0);
        GameObject loadedPrefab;
        Vector3 tilePosition = Vector3.zero;

        for (int i = 0; i < tiledMap.GetLength(0); i++)
        {
            for (int j = 0; j < tiledMap.GetLength(1); j++)
            {
                loadedPrefab = (tiledMap[i, j] == "")? (GameObject)Resources.Load("prefabs/Untagged") : (GameObject)Resources.Load("prefabs/" + tiledMap[i, j]);
                //Debug.Log("(" + i + ", " + j + "): " + tiledMap[i,j]);
                tiles.localPosition = Vector3.zero;
                tilePosition = new Vector3((i * modelMapInfo._gridSize) + tiles.position.x, (j * modelMapInfo._gridSize), tiles.localPosition.z);
                //Debug.Log(tilePosition);
                Instantiate(loadedPrefab, tilePosition, Quaternion.identity, tiles);        // TODO: Fix tiles moving -16.5 units on axis X
                //tiles.position = Vector3.zero;
            }
            //tiles.position = Vector3.zero;
        }
        Debug.Log("Done");
    }
}
