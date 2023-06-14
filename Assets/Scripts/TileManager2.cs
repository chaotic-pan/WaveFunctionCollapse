using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TileManager2 : MonoBehaviour
{
    [TextAreaAttribute] public string comment = "after cell collapse, iterate thru all cells again and compare their possibilities with direct neighbours" + 
                                                "if tile has no possibilities left, restart all";
    [SerializeField] Tileset tileset;
    [SerializeField] GameObject gridCell;
    public int columns;
    public int rows;
    
    WaveCell[,] cells;
    int restart; 
    enum Direction {UP, DOWN, LEFT, RIGHT}

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
        
        DateTime start = DateTime.Now;
        while (GetLowestEntropyCell().entropy >= 0)
        {
            Collapse(GetLowestEntropyCell());
        }

        string[] time = DateTime.Now.Subtract(start).ToString().Split(':');
        Debug.Log("time = " + time[2]);
        Debug.Log("restarts = " + restart);
    }

    void Collapse(WaveCell cell)
    {
        if (cell.entropy == 0)
        {
            Restart();
            return;
        }
        
        var tile = cell.possibleTiles[Random.Range(0, cell.possibleTiles.Count-1)];

        cell.SetTile(tile);

        PropagateEntropy();
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
        
        return minCells.Count == 0 ? cells[0,0] : minCells[Random.Range(0, minCells.Count-1)];
    }

    private void PropagateEntropy()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var tile = cells[x, y].currentTile;
                if (tile == null)
                {
                    ReduceEntropy(cells[x, y], x+1, y, Direction.RIGHT);
                    ReduceEntropy(cells[x, y], x-1, y, Direction.LEFT);
                    ReduceEntropy(cells[x, y], x, y+1, Direction.DOWN);
                    ReduceEntropy(cells[x, y], x, y-1, Direction.UP);
                }
               
            }
        }
    }
    
    void ReduceEntropy(WaveCell cell, int x, int y, Direction direction)
    {
        try
        {
            WaveCell neighbour = cells[x, y];

            if (neighbour.currentTile == null) return;

            List<Tile> tileRules = new List<Tile>();

            switch (direction)
            {
                case Direction.UP:
                    tileRules = neighbour.currentTile.DownConnections;
                    break;
                case Direction.DOWN:
                    tileRules = neighbour.currentTile.UpConnections;
                    break;
                case Direction.LEFT:
                    tileRules = neighbour.currentTile.RightConnections;
                    break;
                case Direction.RIGHT:
                    tileRules = neighbour.currentTile.LeftConnections;
                    break;
            }
            
            cell.SetPossibilities(tileRules);
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
