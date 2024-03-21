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
        SaveTiledMap newSavedData = new();
        newSavedData._gridSize = _gridSize;
        newSavedData._nRows = _nRows;
        newSavedData._nColumns = _nColumns;
        newSavedData._tiledMap = _tiledMap;

        // Convert the information to string into a JSON file
        string jsonData = JsonUtility.ToJson(newSavedData);
        System.IO.File.WriteAllText(Application.persistentDataPath + "\\tiles" + GetInstanceID() + ".json", jsonData);
    }

    private void ReadMap()
    {
        Debug.Log("Entra a ReadMap()");
        Transform tiles = transform.GetChild(0).GetChild(0);
        foreach (Transform tile in tiles)
        {
            Debug.Log("Tiene hijos");
            if (tile.CompareTag("Wall") || tile.CompareTag("Floor"))
            {
                Debug.Log("Ha encontrado un muro o suelo");
                int [] tilePosition = { (int)tile.localPosition.x / _gridSize, (int)tile.localPosition.z / _gridSize };
                _tiledMap[tilePosition[0], tilePosition[1]] = tile.gameObject;
                Debug.Log(_tiledMap.ToString());
            }
        }
    }
}
