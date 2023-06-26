using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ImgReader : MonoBehaviour
{
    [SerializeField] Sprite inputImage;
    public int gridSize;
    public int gridOffset;
    public string saveLocation = "Assets/tilesetNEW";
    public GameObject tileTemp;
    
    private Vector2 imgSize;
    int tileNumber = 0;
    private List<Texture2D> texFiles = new List<Texture2D>();
    private Tileset tileset;
    private string[,] tileIdGrid;
    private int gridX = 0;
    private int gridY = 0;

    // Start is called before the first frame update
    void Start()
    {
        imgSize = inputImage.rect.size;
        tileIdGrid = new String[(int)Math.Ceiling(imgSize.x / gridOffset), (int)Math.Ceiling(imgSize.y / gridOffset)];
        gridY = tileIdGrid.GetLength(1)-1;
        
        AnalyseImage(); 
        SaveTiles();

        for (int i = 0; i < tileIdGrid.GetLength(1); i++)
        {
            for (int j = 0; j < tileIdGrid.GetLength(0); j++)
            {
                Tile t = (Tile)AssetDatabase.LoadAssetAtPath(
                    saveLocation + "/tile" + tileIdGrid[i, j] + ".prefab", typeof(Tile));
                
                
                AddConnectionTile(t.UpRules, 
                    i > 0 ? tileIdGrid[i - 1, j] : tileIdGrid[tileIdGrid.GetLength(1) - 1, j]);

                AddConnectionTile(t.DownRules,
                    i < tileIdGrid.GetLength(1) - 1 ? tileIdGrid[i + 1, j] : tileIdGrid[0, j]);

                AddConnectionTile(t.LeftRules,
                    j > 0 ? tileIdGrid[i, j - 1] : tileIdGrid[i, tileIdGrid.GetLength(1) - 1]);

                AddConnectionTile(t.RightRules,
                    j < tileIdGrid.GetLength(1) - 1 ? tileIdGrid[i, j + 1] : tileIdGrid[i, 0]);
            }
        }

        //  // debug log tileIdGrid
        // string a = "";
        // for (int i = 0; i < tileIdGrid.GetLength(1); i++)
        // {
        //     for (int j = 0; j < tileIdGrid.GetLength(0); j++)
        //     {
        //         a += tileIdGrid[i, j] + ", ";
        //     }
        //
        //     a += "\n";
        // }
        // Debug.Log(a);
        
    }

    private void AnalyseImage()
    {
        Color[] pixels = inputImage.texture.GetPixels();
        Color[] newPixels = new Color[gridSize*gridSize];
        int p = 0;
        
        for (int y = 0; y < imgSize.y; y+=gridOffset)
        {
            for (int x = 0; x < imgSize.x; x+=gridOffset)
            {
                for (int Ty = y; Ty < y+gridSize; Ty++)
                {
                    for (int Tx = x; Tx < x+gridSize; Tx++)
                    {
                        int Iy = (Ty < imgSize.y) ? Ty : Ty - (int)imgSize.y;
                        int Ix = (Tx < imgSize.x) ? Tx : Tx - (int)imgSize.x;

                        int pos = (int)(Ix + Iy * imgSize.x);
                        newPixels[p] = pixels[pos];
                        p++;
                    }
                }

                p = 0;
                var tex = new Texture2D(gridSize, gridSize);
                tex.SetPixels(newPixels, 0);

                var tileID = TextureEqualsAny(tex);
                
                if (tileID == null)
                {
                    byte[] bytes = tex.EncodeToPNG();
                    
                    tileIdGrid[gridY, gridX] = tileNumber < 10 ? "0" + tileNumber : tileNumber.ToString();
                    saveTile(bytes, tileNumber);
                }
                else
                {
                    tileIdGrid[gridY, gridX] = tileID;
                }
                gridX++;
                    if (gridX >= tileIdGrid.GetLength(0))
                    {
                        gridX -= tileIdGrid.GetLength(0);
                        gridY--;
                    }
            }
        }
    }

    private void saveTile(byte[] tilebytes, int fileNumber)
    {
        string fileName = (fileNumber < 10) ? "0" + fileNumber : fileNumber.ToString();
        tileNumber++;
        string path = saveLocation + "/" + fileName + ".png";
        File.WriteAllBytes(path, tilebytes);
        AssetDatabase.ImportAsset(path);
        texFiles.Add((Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)));
        
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
        importer.isReadable = true;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    private string TextureEqualsAny(Texture2D tex)
    {
        if (texFiles.Count == 0)
        {
            return null;
        }
        
        foreach (Texture2D texture in texFiles)
        {
            if (TexturesEqual(tex,texture))
            {
                return texture.name;
            }
        }

        return null;
    }
    
    private bool TexturesEqual(Texture2D tex1, Texture2D tex2)
    {
        var firstPix = tex1.GetPixels();
        var secondPix = tex2.GetPixels();

        for (int i= 0;i < firstPix.Length;i++)
        {
            if (!firstPix[i].ToString().Equals(secondPix[i].ToString()))
            {
                return false;
            }
        }

        return true;
    }

    private void SaveTiles()
    { 
        string path;
        
        tileset =  new GameObject().AddComponent<Tileset>();

        foreach (var tex in texFiles)
        {
            path = saveLocation + "/tile"+ tex.name + ".prefab";
           
            //duplicate tile template
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(tileTemp),path);
            AssetDatabase.ImportAsset(path);
        
            // add texture to tile and set correct size
            SpriteRenderer tile = (SpriteRenderer)AssetDatabase.LoadAssetAtPath(path, typeof(SpriteRenderer));
            tile.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(tex), typeof(Sprite));
            tile.size = new Vector2(0.5f, 0.5f);

            // add tile to tileset
            tileset.tiles.Add(tile.gameObject.GetComponent<Tile>());
        }
        // create another tile and immediatly deleting it bc else the texture of the last tile won't save ?(don't ask)
        path = saveLocation + "/tile.prefab";
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(tileTemp),path);
        AssetDatabase.DeleteAsset(path);
        
        // save tileset as Prefab
        PrefabUtility.SaveAsPrefabAsset(tileset.gameObject, saveLocation + "/tileset.prefab");
    }

    private void AddConnectionTile(List<ConnectionRule> connectionRules, string tileID)
    {
        connectionRules ??= new List<ConnectionRule>();
        Tile tile = (Tile)AssetDatabase.LoadAssetAtPath(saveLocation + "/tile" + tileID + ".prefab", typeof(Tile));

        // if tile already in connection rule, increase frequency note
        foreach (ConnectionRule rule in connectionRules.Where(rule => rule.tile.Equals(tile)))
        {
            rule.frequencyNotes++;
            return;
        }
        // if not, add it 
        connectionRules.Add(new ConnectionRule(tile, 1));
    }

}
