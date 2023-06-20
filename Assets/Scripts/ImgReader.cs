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
                tex.SetPixels(newPixels, 0);
                bytes = tex.EncodeToPNG();
                saveTile(bytes, tileNumber);
            }
        }

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
