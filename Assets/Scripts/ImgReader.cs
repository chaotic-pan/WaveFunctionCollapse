using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;
using Application = UnityEngine.Application;

public class ImgReader : MonoBehaviour
{
    [SerializeField] Sprite inputImage;
    public Vector2 ImgSize;
    public int GidSize = 1;
    public string saveLocation = "Assets/tilesetNEW";

    // Start is called before the first frame update
    void Start()
    {
        ImgSize = inputImage.rect.size;
        
        Texture2D tex = inputImage.texture;
        // Texture2D neTEx = new Texture2D(32,32);
        // neTEx=tex.GetRawTextureData()
        byte[] bytes = tex.EncodeToPNG();

        Debug.Log(tex.height+ "x" +tex.width);
        Debug.Log(bytes.Length);
        saveTile(bytes, "00");
        // path = saveLocation + "/Path 00";
        // FileUtil.CopyFileOrDirectory(path, saveLocation);
    }

    void saveTile(byte[] tilebytes, string fileName)
    {
        string path = saveLocation + "/" + fileName + ".png";
        File.WriteAllBytes(path, tilebytes);
        AssetDatabase.ImportAsset(path);
    }
}
