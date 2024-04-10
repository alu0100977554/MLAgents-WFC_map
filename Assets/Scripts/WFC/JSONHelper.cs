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

    public class Wrapper<T>
    {
        public T[] _items;
        public Row[] _tiledMap;

        /* Constructor
         * Copies devalues of the multidimensional array tileMap
         * from SavedTiledMap class and transforms them into an
         * array of Rows, each one containig an array of strings
         */
        public Wrapper(string [,] tiledMap)
        {
            _tiledMap = new Row[tiledMap.GetLength(0)];
            for (int i = 0; i < tiledMap.GetLength(0); i++)
            {
                _tiledMap[i]._columns = new string[tiledMap.GetLength(1)];
                for (int j = 0; j < tiledMap.GetLength(1); j++)
                {
                    _tiledMap[i]._columns[j] = tiledMap[i, j];
                }
            }
        }
    }

    public static string ToJson<T>(T[] items, string[,] tiledMap)
    {
        Wrapper<T> wrapper = new Wrapper<T>(tiledMap);
        wrapper._items = items;

        return JsonUtility.ToJson(wrapper);
    }
}
