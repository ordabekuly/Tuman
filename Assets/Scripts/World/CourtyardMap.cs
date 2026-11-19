using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tuman
{
    // Assign Gemini's imported tiles in the Inspector; no artwork is embedded here.
    public sealed class CourtyardMap : MonoBehaviour
    {
        [SerializeField] Tilemap ground;
        [SerializeField] Tilemap roads;
        [SerializeField] Tilemap obstacles;
        [SerializeField] TileBase grassTile;
        [SerializeField] TileBase roadTile;
        [SerializeField] TileBase fenceTile;
        [SerializeField, Min(8)] int width = 24;
        [SerializeField, Min(8)] int height = 16;

        [ContextMenu("Generate Courtyard")]
        public void Generate()
        {
            if (!ground || !roads || !obstacles || !grassTile || !roadTile || !fenceTile)
            {
                Debug.LogWarning("Assign all Tilemaps and three tiles before generating.", this);
                return;
            }
            ground.ClearAllTiles();
            roads.ClearAllTiles();
            obstacles.ClearAllTiles();
            for (int x = -width / 2; x < width - width / 2; x++)
            for (int y = -height / 2; y < height - height / 2; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                ground.SetTile(cell, grassTile);
                bool border = x == -width / 2 || x == width - width / 2 - 1 ||
                              y == -height / 2 || y == height - height / 2 - 1;
                if (border) obstacles.SetTile(cell, fenceTile);
                else if (Mathf.Abs(x) <= 1 || Mathf.Abs(y) <= 1) roads.SetTile(cell, roadTile);
            }
        }
    }
}
