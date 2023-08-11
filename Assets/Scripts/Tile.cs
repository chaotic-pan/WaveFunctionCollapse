using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConnectionRule
{
    public Tile tile;
    public float frequencyNotes;

    public ConnectionRule(Tile tile, float frequencyNotes)
    {
        this.tile = tile;
        this.frequencyNotes = frequencyNotes;
    }
}

public class Tile : MonoBehaviour
{
    public float frequencyNotes = 1;
    [SerializeField] public List<Tile> UpConnections = new List<Tile>();
    [SerializeField] public List<Tile> DownConnections = new List<Tile>();
    [SerializeField] public List<Tile> LeftConnections = new List<Tile>();
    [SerializeField] public List<Tile> RightConnections = new List<Tile>();

    public List<ConnectionRule> UpRules;
    public List<ConnectionRule> DownRules;
    public List<ConnectionRule> LeftRules;
    public List<ConnectionRule> RightRules;
}