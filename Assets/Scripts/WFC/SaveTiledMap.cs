using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTiledMap : MonoBehaviour
{
    public int _gridSize;
    public int _nRows;
    public int _nColumns;
    public GameObject[,] _tiledMap;

    public void SaveData()
    {
        _gridSize = this.GetComponent<OverlapWFC>().gridsize;
        _nRows = this.GetComponent<OverlapWFC>().width;
        _nColumns = this.GetComponent<OverlapWFC>().depth;
        _tiledMap = new GameObject[_nRows, _nColumns];
        ReadMap();

        // Save all the information
        SaveTiledMap newSavedData = new SaveTiledMap();
        newSavedData._gridSize = _gridSize;
        newSavedData._nRows = _nRows;
        newSavedData._nColumns = _nColumns;
        newSavedData._tiledMap = _tiledMap;

        // Convert the information to string into a JSON file
        string jsonData = JsonUtility.ToJson(newSavedData);
        PlayerPrefs.SetString("saved_tiled_map", jsonData);
        PlayerPrefs.Save();
    }

    private void ReadMap()
    {
        foreach (Transform tile in transform)
        {
            if (tile.tag == "Wall" || tile.tag == "Floor")
            {
                int [] tilePosition = [(int)tile.localPosition.x / _gridSize, (int)tile.localPosition.z / _gridSize];
                _tiledMap[tilePosition[0], tilePosition[1]] = tile.gameObject;
            }
        }
    }
}
