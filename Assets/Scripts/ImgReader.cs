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
    int tileNumber = 0;
    private List<Texture2D> textures = new List<Texture2D>();

    // Start is called before the first frame update
    void Start()
    {
        imgSize = inputImage.rect.size;

        Color[] pixels = inputImage.texture.GetPixels();
        Texture2D tex = new Texture2D(gridSize, gridSize);
        Color[] newPixels = new Color[gridSize*gridSize];
        int p = 0;
        byte[] bytes;
        
        newPixels = new Color[gridSize*gridSize];
        p = 0;
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
                tex = new Texture2D(gridSize, gridSize);
                tex.SetPixels(newPixels, 0);
                
                if (!TextureEqualsAny(tex))
                {
                    textures.Add(tex);
                     bytes = tex.EncodeToPNG();
                    saveTile(bytes, tileNumber);
                }
            }
        }
        Debug.Log("Done");
    }

    void saveTile(byte[] tilebytes, int fileNumber)
    {
        string fileName = (fileNumber < 10) ? "0" + fileNumber : fileNumber.ToString();
        tileNumber++;
        string path = saveLocation + "/" + fileName + ".png";
        File.WriteAllBytes(path, tilebytes);
        AssetDatabase.ImportAsset(path);
    }

    bool TextureEqualsAny(Texture2D tex)
    {
        if (textures.Count == 0)
        {
            return false;
        }
        
        foreach (var texture in textures)
        {
            if (TexturesEqual(tex,texture))
            {
                return true;
            }
        }

        return false;
    }
    
    bool TexturesEqual(Texture2D tex1, Texture2D tex2)
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


}
