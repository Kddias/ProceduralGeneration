﻿using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{

    #region Declarations

    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);
    public bool resetTerrainCheckBox = true;

    //Voronoi -------------------------------/
    public int peakCount = 1;
    public float fallOff = 0.2f;
    public float dropOff = 0.6f;
    public float minHeight = 0.2f;
    public float maxHeight = 0.5f;
    public enum VoronoiType { Linear = 0, Power = 1, Combined = 2, SinPower }
    public VoronoiType voronoiType = VoronoiType.Linear;


    //PerlinNoise----------------------------/
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinOctaves = 3;
    public float perlinPersistence = 8;
    public float perlinHeightScale = 0.09f;
    public int perlinOffsetX = 0;
    public int perlinOffsetY = 0;

    //MultiplePerlinNoises------------------/
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXscale = 0.01f;
        public float mPerlinYscale = 0.01f;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistence = 8;
        public float mPerlinHeightScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public bool remove = false;
    }
    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>(){
        new PerlinParameters() //istantiate one so the table dont complain//
	};
    //SplatMaps---------------------------/

    [System.Serializable]
    public class splatHeights{
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope =0f;
        public float maxSlope=1.5f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float splatOffSet = 0.01f;
        public float noiseXScale = 0.01f;
        public float noiseYScale = 0.01f;
        public float noiseScaler = 0.1f;
        public bool remove = false;
    }
    public List<splatHeights> splatHeightsList = new List<splatHeights>(){
        new splatHeights() //instantiete one
    };
    //Terrain references--------------------/
    public Terrain terrain;
    public TerrainData terrainData;

    //MPD------------------------------------/
    public float MPDminHeight = -2f;
    public float MPDmaxHeight = 2f;
    public float MPDRoughness = 2.0f;
    public float MPDHeightDampener = 2f;

    //Smooth----------------------------/
    public int smoothAmount = 2;

    #endregion
    public void AddNewSplatHeight(){
        splatHeightsList.Add(new splatHeights());
    }
    public void RemoveSplatHeight(){
        List<splatHeights> kept = new List<splatHeights>();
        for (int i = 0; i < splatHeightsList.Count; i++)
        {
            if (!splatHeightsList[i].remove)
            {
                kept.Add(splatHeightsList[i]);
            }
        }
        if (kept.Count == 0)
        {
            kept.Add(splatHeightsList[0]);
        }
        splatHeightsList = kept;
    }
    float GetSteepness(float[,] heightmap,int x,int y,int width,int height){
        float h = heightmap[x,y];
        int nx = x+1;
        int ny = y+1;
        //if on the edge
        if(nx > width - 1) nx = x-1;
        if(ny > height - 1) ny = y-1;

        float dx = heightmap[nx,y] - h;
        float dy = heightmap[x,ny] - h;
        Vector2 gradient = new Vector2(dx,dy);
        float steep = gradient.magnitude;
        return steep;
    }
    public void SplatMaps(){
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatHeightsList.Count];
        int spindex = 0;
        foreach (splatHeights sh in splatHeightsList)
        {
            newSplatPrototypes[spindex] = new TerrainLayer();
            newSplatPrototypes[spindex].diffuseTexture = sh.texture;
            newSplatPrototypes[spindex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spindex].tileSize = sh.tileSize;
            newSplatPrototypes[spindex].diffuseTexture.Apply(true);
            spindex++;
        }
        terrainData.terrainLayers = newSplatPrototypes;
        float[,] heightMap = terrainData.GetHeights(0,0,
                                                    terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);
        float[,,] splatmapData = new float[terrainData.alphamapWidth,
                                           terrainData.alphamapHeight,
                                           terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatHeightsList.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x*splatHeightsList[i].noiseXScale
                                                   ,y*splatHeightsList[i].noiseYScale)
                                                    * splatHeightsList[i].noiseScaler;
                    float offSet = splatHeightsList[i].splatOffSet + noise;
                    float thisHeightStart = splatHeightsList[i].minHeight - offSet;
                    float thisHeightStop = splatHeightsList[i].maxHeight + offSet;
                    //float steepness = GetSteepness(heightMap,x,y,
                    //                              terrainData.heightmapWidth,
                    //                               terrainData.heightmapHeight);
                    float steepness = terrainData.GetSteepness(y/(float)terrainData.alphamapHeight,
                                                               x/(float)terrainData.alphamapWidth);

                    if ((heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop)&&
                        steepness >= splatHeightsList[i].minSlope && 
                        steepness <= splatHeightsList[i].maxSlope)
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeightsList.Count; j++)
                {
                    splatmapData[x,y,j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0,0,splatmapData);
    }
    void NormalizeVector(float[] vector){
        float total = 0;
        for (int i = 0; i < vector.Length; i++)
        {
            total+=vector[i];
        }
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i]/=total;
        }
    }
    float[,] getHeightMap()
    {

        float[,] HeightMap = new float[0, 0];

        if (!resetTerrainCheckBox)
        {
            HeightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                        terrainData.heightmapHeight);
        }
        else
        {
            HeightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
        }
        return HeightMap;
    }
    public void midPointDisplacement()
    {
        float[,] heightMap = getHeightMap();
        int width = terrainData.heightmapWidth - 1;
        int squareSize = width;
        float heightMin = MPDminHeight;
        float heightMax = MPDmaxHeight;
        float heightDampener = (float)Mathf.Pow(MPDHeightDampener, -1 * MPDRoughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        /* heightMap[0,0] = UnityEngine.Random.Range(0f,0.2f);
        heightMap[0,terrainData.heightmapHeight -2] = UnityEngine.Random.Range(0f,0.2f);
        heightMap[terrainData.heightmapWidth -2,0] = UnityEngine.Random.Range(0f,0.2f);
        heightMap[terrainData.heightmapWidth -2,terrainData.heightmapHeight -2] 
                  = UnityEngine.Random.Range(0f,0.2f);*/

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)((heightMap[x, y] +
                                            heightMap[cornerX, y] +
                                            heightMap[x, cornerY] +
                                            heightMap[cornerX, cornerY]) / 4.0f +
                                            UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {

                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0
                        || pmidXR >= width - 1 || pmidYU >= width - 1) continue;

                    //Calculate the square value for the bottom side  
                    heightMap[midX, y] = (float)((heightMap[midX, midY] +
                                                  heightMap[x, y] +
                                                  heightMap[midX, pmidYD] +
                                                  heightMap[cornerX, y]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the top side   
                    heightMap[midX, cornerY] = (float)((heightMap[x, cornerY] +
                                                            heightMap[midX, midY] +
                                                            heightMap[cornerX, cornerY] +
                                                        heightMap[midX, pmidYU]) / 4.0f +
                                                       UnityEngine.Random.Range(heightMin, heightMax));

                    //Calculate the square value for the left side   
                    heightMap[x, midY] = (float)((heightMap[x, y] +
                                                            heightMap[pmidXL, midY] +
                                                            heightMap[x, cornerY] +
                                                  heightMap[midX, midY]) / 4.0f +
                                                 UnityEngine.Random.Range(heightMin, heightMax));
                    //Calculate the square value for the right side   
                    heightMap[cornerX, midY] = (float)((heightMap[midX, y] +
                                                            heightMap[midX, midY] +
                                                            heightMap[cornerX, cornerY] +
                                                            heightMap[pmidXR, midY]) / 4.0f +
                                                       UnityEngine.Random.Range(heightMin, heightMax));

                }
            }

            squareSize = (int)(squareSize / 2.0f);
            heightMin *= heightDampener;
            heightMax *= heightDampener;
        }

        terrainData.SetHeights(0, 0, heightMap);
    }
    public void voronoi()
    {

        float[,] heightMap = getHeightMap();

        for (int i = 0; i < peakCount; i++)
        {
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapWidth) //location
                                       , UnityEngine.Random.Range(minHeight, maxHeight) //height
                                       , UnityEngine.Random.Range(0, terrainData.heightmapHeight)); //location

            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
            {
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            }
            else
            {
                continue; //skip
            }


            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapWidth
                                                                             , terrainData.heightmapHeight));
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float h;

                        if (voronoiType == VoronoiType.Combined)
                        {
                            h = peak.y - distanceToPeak * fallOff - Mathf.Pow(distanceToPeak, dropOff);
                        }
                        else if (voronoiType == VoronoiType.Power)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak, fallOff) * fallOff;
                        }
                        else if (voronoiType == VoronoiType.SinPower)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak * 3, fallOff) -
                                         Mathf.Sin(distanceToPeak * 2 * Mathf.PI);
                        }
                        else
                        {
                            h = peak.y - distanceToPeak * fallOff;
                        }

                        //only set a hight if the height already stored is less then the new
                        if (heightMap[x, y] < h)
                        {
                            heightMap[x, y] = h;
                        }

                    }

                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }
    //Blurr Algorithm
    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0,0,
        terrainData.heightmapWidth,terrainData.heightmapHeight);
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                 "Progress",
                                 smoothProgress);

        for (int s = 0; s < smoothAmount; s++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                for (int x = 0; x < terrainData.heightmapWidth; x++)
                {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y),
                                                                  terrainData.heightmapWidth,
                                                                  terrainData.heightmapHeight);
                    foreach (Vector2 n in neighbours)
                    {
                        avgHeight += heightMap[(int)n.x, (int)n.y];
                    }

                    heightMap[x, y] = avgHeight / ((float)neighbours.Count + 1);
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain",
                                             "Progress",
                                             smoothProgress/smoothAmount);

        }
        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
    }
    List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1),
                                                Mathf.Clamp(pos.y + y, 0, height - 1));
                    if (!neighbours.Contains(nPos))
                        neighbours.Add(nPos);
                }
            }
        }
        return neighbours;
    }
    public void perlin()
    {
        float[,] heightMap = getHeightMap();
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                heightMap[x, y] += Utils.FractionalBM((x + perlinOffsetX) * perlinXScale,
                                                    (y + perlinOffsetY) * perlinYScale,
                                                    perlinOctaves,
                                                    perlinPersistence) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    public void MultiplePerlin()
    {
        float[,] heightMap = getHeightMap();
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utils.FractionalBM((x + p.mPerlinOffsetX) * p.mPerlinXscale,
                                                         (y + p.mPerlinOffsetY) * p.mPerlinYscale,
                                                         p.mPerlinOctaves,
                                                         p.mPerlinPersistence) * p.mPerlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    public void addNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }
    public void removeNewPerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }
        if (keptPerlinParameters.Count == 0) //dont want to keep a single one
        {
            keptPerlinParameters.Add(new PerlinParameters()); // add atleast one "because Table"
        }
        perlinParameters = keptPerlinParameters;
    }
    //att our terrain
    public void RandomTerrain()
    {

        float[,] heightMap = getHeightMap();
        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    public void LoadTexture()
    {
        float[,] heightMap = getHeightMap();

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                                (int)(y * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }
    public void resetTerrain()
    {

        float[,] heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

        for (int x = 0; x < terrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < terrainData.heightmapHeight; y++)
            {
                heightMap[x, y] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    private void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData; //becare 
    }
    private void Awake()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        Addtag(tagsProp, "Terrain");
        Addtag(tagsProp, "Cloud");
        Addtag(tagsProp, "Shore");

        tagManager.ApplyModifiedProperties();

        this.gameObject.tag = "Terrain";
    }
    void Addtag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;
        //look if tag exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag))
            {
                found = true;
                break;
            }
        }
        //if not add a new one
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }
    
}
