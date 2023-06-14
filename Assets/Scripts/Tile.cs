using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConnectionRule
{
    public Tile tile;
    public float frequencyNotes;
}

public class Tile : MonoBehaviour
{
    public float frequencyHints = 1;
    [SerializeField] public List<Tile> UpConnections = new List<Tile>();
    [SerializeField] public List<Tile> DownConnections = new List<Tile>();
    [SerializeField] public List<Tile> LeftConnections = new List<Tile>();
    [SerializeField] public List<Tile> RightConnections = new List<Tile>();

    public ConnectionRule[] UpRules;
    public ConnectionRule[] DownRules;
    public ConnectionRule[] LeftRules;
    public ConnectionRule[] RightRules;
}