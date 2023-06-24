using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ImgReader : MonoBehaviour
{
    [SerializeField] Sprite inputImage;
    Vector2 imgSize;
    public int gridSize;
    public int gridOffset;
    public string saveLocation = "Assets/tilesetNEW";
    public Object tileTemp;
    
    int tileNumber = 0;
    private List<Texture2D> texFiles = new List<Texture2D>();
    private Tileset tileset;

    // Start is called before the first frame update
    void Start()
    {
       AnalyseImage();
       SaveTiles();
       

       Debug.Log("Done");
    }
    
    private void AnalyseImage()
    {
        imgSize = inputImage.rect.size;

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
                
                if (!TextureEqualsAny(tex))
                {
                    byte[] bytes = tex.EncodeToPNG();
                    saveTile(bytes, tileNumber);
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

    private bool TextureEqualsAny(Texture2D tex)
    {
        if (texFiles.Count == 0)
        {
            return false;
        }
        
        foreach (Texture2D texture in texFiles)
        {
            if (TexturesEqual(tex,texture))
            {
                return true;
            }
        }

        return false;
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

}
