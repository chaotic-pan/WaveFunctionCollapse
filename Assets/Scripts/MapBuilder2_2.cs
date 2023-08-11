using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapBuilder2_2 : MonoBehaviour
{
    [TextAreaAttribute] public string comment = "fixed lowestEntropy double-call and earlier contradiction recognition \n" +
                                                "cell collapse reduces possibilities neighbours' neighbours  \n" +
                                                "if tile has no possibilities left, restart all";
    [SerializeField] private Tileset tileset;
    [SerializeField] private GameObject gridCell;
    public int columns;
    public int rows;
    
    GridCell[,] cells;
    int restart; 
    DateTime lastStart;
    GridCell currentCell;
    
    
    void Start()
    {
        cells = new GridCell[columns,rows];
        gameObject.GetComponent<GridLayoutGroup>().constraintCount = columns;
        
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                cells[x,y] = Instantiate(gridCell, transform).GetComponent<GridCell>();
                cells[x,y].Init(tileset.tiles, x, y);
            }
        }

        DateTime start = DateTime.Now;
        lastStart = start;
        while (GetLowestEntropyCell().entropy >= 0)
        {
            Collapse(currentCell);
        }
        
        var stopTime = DateTime.Now;
        string[] time = stopTime.Subtract(start).ToString().Split(':');
        string[] lastTime = stopTime.Subtract(lastStart).ToString().Split(':');
        Debug.Log("time = " + time[2]);
        Debug.Log("restarts = " + restart);
        Debug.Log("last time = " + lastTime[2]);
    }

    void Collapse(GridCell cell)
    {
        if (cell.entropy == 0)
        {
            Restart();
            lastStart = DateTime.Now;
            return;
        }
        
        var tile = cell.possibleTiles[Random.Range(0, cell.possibleTiles.Count)];

        cell.SetTile(tile);
       
        PropagateEntropy(cell.x, cell.y);
        
        PropagateEntropy(cell.x+1, cell.y);
        PropagateEntropy(cell.x-1, cell.y);
        PropagateEntropy(cell.x, cell.y+1);
        PropagateEntropy(cell.x, cell.y-1);
    }
    
    GridCell GetLowestEntropyCell()
    {
        List<GridCell> minCells = new List<GridCell>();
        int minEnt = tileset.tiles.Count;
        
        foreach (var cell in cells)
        {
            switch (cell.entropy)
            {
                case -1: continue;
                case 0: 
                    Restart();
                    currentCell = cells[Random.Range(0, cells.GetLength(0)), Random.Range(0, cells.GetLength(1))];
                    return currentCell;
                case var e when e < minEnt:
                    minEnt = cell.entropy;
                    minCells.Clear();
                    minCells.Add(cell);
                    break;
                case var e when e == minEnt: 
                    minCells.Add(cell);
                    break;
            }
        }
        
        currentCell = minCells.Count == 0 ? cells[0, 0] : minCells[Random.Range(0, minCells.Count)];
        
        return currentCell;
    }

    void PropagateEntropy(int x, int y)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows) return;
        var cell = cells[x, y];
        
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

            foreach (var poss in cell.possibleTiles)
            {
                right.AddRange(poss.RightConnections);
                left.AddRange(poss.LeftConnections);
                down.AddRange(poss.DownConnections);
                up.AddRange(poss.UpConnections);
            }
            
            right = right.Distinct().ToList();
            left = left.Distinct().ToList();
            down = down.Distinct().ToList();
            up = up.Distinct().ToList();

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
    
}

