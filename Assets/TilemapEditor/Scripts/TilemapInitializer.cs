using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapInitializer : Singleton<TilemapInitializer>
{
    [SerializeField] List<BuildingCategory> categoriesToCreateTilemapFor;
    [SerializeField] Transform grid;

    private void Start()
    {
        CreateMaps();
    }

    private void CreateMaps()
    {
        foreach (BuildingCategory category in categoriesToCreateTilemapFor)
        {
            // Create new GameObject
            GameObject obj = new GameObject("Tilemap_" + category.name);

            obj.layer = LayerMask.NameToLayer("Platform");

            // Assign Tilemap Features
            Tilemap map = obj.AddComponent<Tilemap>();
            TilemapRenderer tr = obj.AddComponent<TilemapRenderer>();
            TilemapCollider2D tc = obj.AddComponent<TilemapCollider2D>();

            // Set Parent
            obj.transform.SetParent(grid);

            // Here you can add settings ...
            tr.sortingOrder = category.SortingOrder;

            category.Tilemap = map;
        }
    }
}