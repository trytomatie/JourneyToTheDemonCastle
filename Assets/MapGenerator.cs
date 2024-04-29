using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Camera mapCamera;
    public Texture2D[] mapTextures;

    public static MapGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateMapTextures(int floors, int gridSize)
    {
        SystemMessageManager.DisplayProcessingMessage("Generating Map Textures");
        StartCoroutine(GenerateMapTexturesCoroutine(floors, gridSize));
    }

    private IEnumerator GenerateMapTexturesCoroutine(int floors ,int gridSize)
    {
        mapTextures = new Texture2D[floors];
        for (int i = 0; i < mapTextures.Length; i++)
        {
            mapCamera.transform.localPosition = new Vector3(0, (i * gridSize) + 1, 0);

            yield return new WaitForSeconds(1);
            Texture2D texture = new Texture2D(2048, 2048, TextureFormat.RGB24, true);
            // Generate texture from mapCamera
            RenderTexture.active = mapCamera.targetTexture;
            texture.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
            // Make every pixel that is not black pure white    
            texture.Apply();
            
            mapTextures[i] = texture;
        }
        RenderTexture.active = null;
        SystemMessageManager.ProcessingFinished();
    }
}
