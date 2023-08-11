using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapBuilder4 : MonoBehaviour
{
    [TextAreaAttribute] public string comment = "tile frequency notes + connection frequency notes \n" +
                                                "cell collapse reduces possibilities neighbours' neighbours  \n" +
                                                "if tile has no possibilities left, restart all ";
    [SerializeField] private Tileset tileset;
    [SerializeField] private GameObject gridCell;
    public int columns;
    public int rows;
    
    GridCell[,] cells;
    int restart; 
    DateTime lastStart;
    
    enum Direction 
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    
    
    void Start()
    {
        // build up empty grid
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
            Collapse(GetLowestEntropyCell());
        }
        
        // testing stats
        var stopTime = DateTime.Now;
        string[] time = stopTime.Subtract(start).ToString().Split(':');
        string[] lastTime = stopTime.Subtract(lastStart).ToString().Split(':');
        Debug.Log("time = " + time[2]);
        Debug.Log("restarts = " + restart);
        Debug.Log("last time = " + lastTime[2]);
    }

    void Collapse(GridCell cell)
    {
        // empty cell with no possibilities = error -> restart
        if (cell.entropy == 0)
        {
            Restart();
            lastStart = DateTime.Now;
            return;
        }
        
        int x = cell.x, y = cell.y;

        // randomly select of possible tiles, include frequency notes
        float rndRange = 0;

        foreach (var poss in cell.possibleTiles)
        {
            var freq = poss.frequencyNotes;
            // multiple frequency notes with connection frequency 
            freq *= MultiplyFrequency(poss, x+1, y, Direction.LEFT);
            freq *= MultiplyFrequency(poss, x-1, y, Direction.RIGHT);
            freq *= MultiplyFrequency(poss, x, y+1, Direction.UP);
            freq *= MultiplyFrequency(poss, x, y-1, Direction.DOWN);
            
            rndRange += freq;
        }

        float rndValue = Random.Range(0, rndRange);
        Tile rndTile = cell.possibleTiles[0];

        foreach (var poss in cell.possibleTiles)
        {
            if (rndValue > poss.frequencyNotes)
            {
                rndValue -= poss.frequencyNotes;
            }
            else
            {
                rndTile = poss;
                break;
            }
        }
        
        cell.SetTile(rndTile);
        
        
        // update possibilities of directly neighbouring cells
        PropagateEntropy(x, y);

        // update possibilities of neighbours neighbouring cells
        PropagateEntropy(x+1, y);
        PropagateEntropy(x-1, y);
        PropagateEntropy(x, y+1);
        PropagateEntropy(x, y-1);
        
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
        
        var tile = cell.currentTile;

        if (tile != null)
        { 
            ReduceEntropy(tile.RightRules.Select(rule => rule.tile).ToList(), cell.x+1, cell.y);
            ReduceEntropy(tile.LeftRules.Select(rule => rule.tile).ToList(), cell.x-1, cell.y);
            ReduceEntropy(tile.DownRules.Select(rule => rule.tile).ToList(), cell.x, cell.y+1);
            ReduceEntropy(tile.UpRules.Select(rule => rule.tile).ToList(), cell.x, cell.y-1);
        }
        else
        {
            List<Tile> right = new List<Tile>();
            List<Tile> left = new List<Tile>();
            List<Tile> down = new List<Tile>();
            List<Tile> up = new List<Tile>();
            
            foreach (var poss in cell.possibleTiles)
            {
                right.AddRange(poss.RightRules.Select(rule => rule.tile).ToList());
                left.AddRange(poss.LeftRules.Select(rule => rule.tile).ToList());
                down.AddRange(poss.DownRules.Select(rule => rule.tile).ToList());
                up.AddRange(poss.UpRules.Select(rule => rule.tile).ToList());
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

    float MultiplyFrequency(Tile tile, int x, int y, Direction direction)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows) return 1;
        
        if (cells[x,y].currentTile == null) return 1;

        switch (direction)
        {
            case Direction.UP:
                foreach (var rule in  cells[x,y].currentTile.UpRules)
                {
                    if (rule.tile == tile)
                    {
                        return rule.frequencyNotes;
                    }
                }
                break; 
            case Direction.DOWN:
                foreach (var rule in  cells[x,y].currentTile.DownRules)
                {
                    if (rule.tile == tile)
                    {
                        return rule.frequencyNotes;
                    }
                }
                break;
            case Direction.LEFT:
                foreach (var rule in  cells[x,y].currentTile.LeftRules)
                {
                    if (rule.tile == tile)
                    {
                        return rule.frequencyNotes;
                    }
                }
                break; 
            case Direction.RIGHT:
                foreach (var rule in  cells[x,y].currentTile.RightRules)
                {
                    if (rule.tile == tile)
                    {
                        return rule.frequencyNotes;
                    }
                }
                break;
        }

        return 1;
    }

    void Restart()
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

