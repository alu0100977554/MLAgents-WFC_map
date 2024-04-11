using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONHelper : MonoBehaviour
{
    // The serializer to convert to JSON file cannot access multidimensional arrays
    [System.Serializable]
    public struct Row
    {
        [SerializeField]
        public string[] _columns;
    }

    public class Wrapper
    {
        public int[] _items;
        public Row[] _tiledMap;

        /* Constructor
         * Copies devalues of the multidimensional array tileMap
         * from SavedTiledMap class and transforms them into an
         * array of Rows, each one containig an array of strings
         */
        public Wrapper(SaveTiledMap savedData)
        {
            _items = new int[3] { savedData._gridSize, savedData._nRows, savedData._nColumns };

            _tiledMap = new Row[savedData._tiledMap.GetLength(0)];
            for (int i = 0; i < savedData._tiledMap.GetLength(0); i++)
            {
                _tiledMap[i]._columns = new string[savedData._tiledMap.GetLength(1)];
                for (int j = 0; j < savedData._tiledMap.GetLength(1); j++)
                {
                    _tiledMap[i]._columns[j] = savedData._tiledMap[i, j];
                }
            }
        }
    }

    public static string ToJson(SaveTiledMap savedData)
    {
        Wrapper wrapper = new Wrapper(savedData);
        return JsonUtility.ToJson(wrapper);
    }

    public static LoadTiledMap FromJson(string jsonContent)
    {
        Wrapper wrapper = JsonUtility.FromJson<Wrapper>(jsonContent);

        LoadTiledMap loadedData = new LoadTiledMap
        {
            _gridSize = wrapper._items[0],
            _nRows = wrapper._items[1],
            _nColumns = wrapper._items[2],
            _tiledMap = new string[wrapper._items[1], wrapper._items[2]]
        };

        for (int i = 0; i < wrapper._tiledMap.Length; i++)
        {
            for (int j = 0; j < wrapper._tiledMap[i]._columns.Length; j++)
            {
                loadedData._tiledMap[i, j] = wrapper._tiledMap[i]._columns[j];
            }
        }

        return loadedData;
    }
}
