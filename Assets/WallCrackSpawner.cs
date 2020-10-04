using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallCrackSpawner : MonoBehaviour
{
    public GameObject wallCrackPrefab;
    public float crackChance = 0.99f;
    public float depth = 1f;

    float maxHeight = 150f;

    Tilemap tilemap;

    [Button]
    void SpawnWallCracks()
    {
        tilemap = GetComponent<Tilemap>();
        var bounds = tilemap.cellBounds;
        var tilemapPosition = new Vector3Int((int)tilemap.transform.position.x, (int)tilemap.transform.position.y, (int)tilemap.transform.position.z);
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var position = tilemapPosition + new Vector3Int(x, y, 0);
                if (Random.value > crackChance && tilemap.GetTile(position) == null && position.y < maxHeight)
                {
                    var newObject = Instantiate(wallCrackPrefab, position + Vector3.forward * depth, Quaternion.identity, tilemap.transform) as GameObject;
                }
            }
        }
    }

    [Button]
    void ClearWallCracks()
    {
        var cracks = GetComponentsInChildren<WallCrack>();
        for (int i = 0; i < cracks.Length; i++)
        {
            DestroyImmediate(cracks[i].gameObject);
        }
    }

}
