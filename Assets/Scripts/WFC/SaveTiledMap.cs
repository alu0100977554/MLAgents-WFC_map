using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTiledMap : MonoBehaviour
{
    public int _gridSize;
    public int _nRows;
    public int _nColumns;
    public string[,] _tiledMap;

    public void SaveData()
    {
        _gridSize = this.GetComponent<OverlapWFC>().gridsize;
        _nRows = this.GetComponent<OverlapWFC>().width;
        _nColumns = this.GetComponent<OverlapWFC>().depth;
        _tiledMap = new string[_nRows, _nColumns];
        int[] gridInfo = { _gridSize, _nRows, _nColumns };
        //Debug.Log(_tiledMap.GetLength(0) + ", " + _tiledMap.GetLength(1));
        ReadMap();

        // Save all the information
        /*SaveTiledMap newSavedData = new();
        newSavedData._gridSize = _gridSize;
        newSavedData._nRows = _nRows;
        newSavedData._nColumns = _nColumns;
        newSavedData._tiledMap = _tiledMap;

        */

        // Convert the information to string into a JSON file
        // string jsonData = JsonUtility.ToJson(newSavedData);
        string jsonData = JSONHelper.ToJson<int>(gridInfo, _tiledMap);
        System.IO.File.WriteAllText(Application.persistentDataPath + "\\tiles" + GetInstanceID() + ".json", jsonData);
    }

    private void ReadMap()
    {
        Debug.Log("Entra a ReadMap()");
        Transform tiles = transform.GetChild(1).GetChild(0);
        foreach (Transform tile in tiles)
        {
            if (tile.CompareTag("Wall") || tile.CompareTag("Floor"))
            {
                //Debug.Log("Tag = " + tile.tag);
                int [] tilePosition = { (int)tile.localPosition.x / _gridSize, (int)tile.localPosition.y / _gridSize };
                _tiledMap[tilePosition[0], tilePosition[1]] = tile.tag;
            }
        }
    }
}
