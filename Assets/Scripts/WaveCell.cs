using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveCell : MonoBehaviour
{
      public int entropy;
      public Tile currentTile;
      public List<Tile> possibleTiles;
      public int x;
      public int y;

      public void Init(List<Tile> possibleTiles, int x, int y)
      {
            this.possibleTiles = possibleTiles;
            this.x = x;
            this.y = y;
            entropy = possibleTiles.Count;
            currentTile = null;
      }

      public void SetTile(Tile tile)
      {
            currentTile = tile;
            Instantiate(tile.gameObject, transform);
            possibleTiles = new List<Tile>(); 
            entropy = -1;
      }

      public void SetPossibilities(List<Tile> tiles)
      { 
            if (currentTile != null)
                  return;
            
            possibleTiles = new List<Tile>(possibleTiles).Intersect(tiles).ToList();
            entropy = possibleTiles.Count;
      }
}