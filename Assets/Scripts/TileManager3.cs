using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TileManager3 : MonoBehaviour
{
    [TextAreaAttribute] public string comment = "cell collapse reduces possibilities neighbours' neighbours  \n" +
                                                "if tile has no possibilities left, restart all";
    [SerializeField] private Tileset tileset;
    [SerializeField] private GameObject gridCell;
    public int columns;
    public int rows;
    public bool debug;
    
    WaveCell[,] cells;
    int restart; 
    DateTime lastStart;
    
    
    void Start()
    {
        cells = new WaveCell[columns,rows];
        gameObject.GetComponent<GridLayoutGroup>().constraintCount = columns;
        
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                cells[x,y] = Instantiate(gridCell, transform).GetComponent<WaveCell>();
                cells[x,y].Init(tileset.tiles, x, y);
            }
        }

        if (debug) return;
        
        GameObject.Find("Debug Canvas").SetActive(false);
        
        
        DateTime start = DateTime.Now;
        lastStart = start;
        while (GetLowestEntropyCell().entropy >= 0)
        {
            Collapse(GetLowestEntropyCell());
        }

        var stopTime = DateTime.Now;
        string[] time = stopTime.Subtract(start).ToString().Split(':');
        string[] lastTime = stopTime.Subtract(lastStart).ToString().Split(':');
        Debug.Log("time = " + time[2]);
        Debug.Log("restarts = " + restart);
        Debug.Log("last time = " + lastTime[2]);
    }

    void Collapse(WaveCell cell)
    {
        if (cell.entropy == 0)
        {
            Restart();
            lastStart = DateTime.Now;
            return;
        }
        
        var tile = cell.possibleTiles[Random.Range(0, cell.possibleTiles.Count)];

        cell.SetTile(tile);
       
        // ReduceEntropy(tile.RightConnections, cell.x+1, cell.y);
        // ReduceEntropy(tile.LeftConnections, cell.x-1, cell.y);
        // ReduceEntropy(tile.DownConnections, cell.x, cell.y+1);
        // ReduceEntropy(tile.UpConnections, cell.x, cell.y-1);
        
        PropagateEntropy(cell.x, cell.y);
        
        PropagateEntropy(cell.x+1, cell.y);
        PropagateEntropy(cell.x-1, cell.y);
        PropagateEntropy(cell.x, cell.y+1);
        PropagateEntropy(cell.x, cell.y-1);
    }
    
    WaveCell GetLowestEntropyCell()
    {
        List<WaveCell> minCells = new List<WaveCell>();
        int minEnt = tileset.tiles.Count;
        
        foreach (var cell in cells)
        {
            switch (cell.entropy)
            {
                case -1: continue;
                case var e when e == minEnt: 
                    minCells.Add(cell);
                    break;
                case var e when e < minEnt:
                    minEnt = cell.entropy;
                    minCells.Clear();
                    minCells.Add(cell);
                    break;
            }
        }
        
        return minCells.Count == 0 ? cells[0,0] : minCells[Random.Range(0, minCells.Count)];
    }

    void PropagateEntropy(int x, int y)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows) return;
        var cell = cells[x, y];
            
        // Debug.Log(x+", "+y);
        var tile = cell.currentTile;

        if (tile != null)
        { 
            ReduceEntropy(tile.RightConnections, cell.x+1, cell.y);
            ReduceEntropy(tile.LeftConnections, cell.x-1, cell.y);
            ReduceEntropy(tile.DownConnections, cell.x, cell.y+1);
            ReduceEntropy(tile.UpConnections, cell.x, cell.y-1);
        }
        else
        {
            List<Tile> right = new List<Tile>();
            List<Tile> left = new List<Tile>();
            List<Tile> down = new List<Tile>();
            List<Tile> up = new List<Tile>();

            // Debug.Log(String.Join(", ",right));
            // // Debug.Log(left);
            // // Debug.Log(down);
            // Debug.Log(string.Join(", ",up));
        
            foreach (var poss in cell.possibleTiles)
            {
                // Debug.Log(poss);
                right.AddRange(poss.RightConnections);
                left.AddRange(poss.LeftConnections);
                down.AddRange(poss.DownConnections);
                up.AddRange(poss.UpConnections);
            }
            
            right = right.Distinct().ToList();
            left = left.Distinct().ToList();
            down = down.Distinct().ToList();
            up = up.Distinct().ToList();
            
            // Debug.Log(string.Join(", ",right));
            // Debug.Log(string.Join(", ",up));
            
            ReduceEntropy(right, cell.x+1, cell.y);
            ReduceEntropy(left, cell.x-1, cell.y);
            ReduceEntropy(down, cell.x, cell.y+1);
            ReduceEntropy(up, cell.x, cell.y-1);
           
        }
        
    }

    void ReduceEntropy(List<Tile> tileRules, int x, int y)
    {
        try
        {
            cells[x, y].SetPossibilities(tileRules);
        }
        catch { /* out of grid range, just ignore and continue */ }
    }

    private void Restart()
    {
        restart++;
        foreach (var cell in cells)
        {
            foreach (Transform child in cell.transform) {
                Destroy(child.gameObject);
            }

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    cells[x,y].Init(tileset.tiles, x, y);
                }
            }
    
        }
    }

    public void debugNext()
    {
        if (GetLowestEntropyCell().entropy >= 0)
        {
            Collapse(GetLowestEntropyCell());
        }
        else
        {
            Debug.Log("DONE");
        }
    }
}

