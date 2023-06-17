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
    int tileNumber = 1;

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
        // for (int i = 0; i < imgSize.x; i+=gridOffset)
        // {
        //     for (int q = i; q < i+gridSize; q++)
        //     {
        //         Debug.Log(q);
        //
        //         // int Ix = (x < imgSize.x) ? x : x - (int)imgSize.x;
        //         // int Iy = (y < imgSize.y) ? y : y - (int)imgSize.y;
        //         // int pos = (int)(Ix + Iy * imgSize.x);
        //     }
        //     Debug.Log("NEW LOOP");
        //     Debug.Log(i);
        // }
        p = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = (int)(1.5*gridSize); x < (int)(2.5*gridSize); x++)
            {
                int Iy = (y < imgSize.y) ? y : y - (int)imgSize.y;
                int Ix = (x < imgSize.x) ? x : x - (int)imgSize.x;

                int pos = (int)(Ix + Iy * imgSize.x);
                newPixels[p] = pixels[pos];
                p++;
            }
        }
        tex.SetPixels(newPixels, 0);
        bytes = tex.EncodeToPNG();
        saveTile(bytes, tileNumber);
    }

    void saveTile(byte[] tilebytes, int fileNumber)
    {
        string fileName = (fileNumber < 10) ? "0" + fileNumber : fileNumber.ToString();
        tileNumber++;
        string path = saveLocation + "/" + fileName + ".png";
        File.WriteAllBytes(path, tilebytes);
        AssetDatabase.ImportAsset(path);
    }
    
    
}
