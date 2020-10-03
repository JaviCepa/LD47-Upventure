using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDepthFaker : MonoBehaviour
{

    public int copies = 10;
    public float separation = 0.1f;
    Tilemap tilemap;
    TilemapRenderer tilemapRenderer;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        var bounds = tilemap.cellBounds;
        var tilesTemplate = tilemap.GetTilesBlock(bounds);
        var positions = new Vector3Int[(bounds.xMax - bounds.xMin) * (bounds.yMax - bounds.yMin)];
        int currentIndex = 0;
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                var position = new Vector3Int(x, y, 0);
                positions[currentIndex] = position;
                currentIndex++;
            }
        }


        for (int i = 0; i < copies; i++)
        {
            var tilemapObject = new GameObject($"TilemapClone {i}");
            tilemapObject.transform.SetParent(transform);
            var cloneTilemap = tilemapObject.AddComponent<Tilemap>();
            var tmRenderer = tilemapObject.AddComponent<TilemapRenderer>();
            tmRenderer.sharedMaterial = tilemapRenderer.sharedMaterial;
            cloneTilemap.SetTiles(positions, tilesTemplate);
            tilemapObject.transform.position = transform.position + Vector3.forward * i * separation;
        }
    }
}
