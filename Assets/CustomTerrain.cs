using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{

    #region Declarations
    public enum TagType { Tag = 0, Layer = 1 }
    [SerializeField]
    int terrainLayer = 0;
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
    public class splatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 1.5f;
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
    //Vegetation ----------------------/
    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0f;
        public float maxSlope = 90;
        public float minScale = 0.5f;
        public float maxScale = 1.0f;
        public Color colour1 = Color.white;
        public Color colour2 = Color.white;
        public Color lightColour = Color.white;
        public float minRotation = 0;
        public float maxRotation = 360;
        public float density = 0.5f;
        public bool remove = false;
    }
    public List<Vegetation> vegetations = new List<Vegetation>(){
        new Vegetation()
    };
    public int maximumTrees = 5000;
    public int treeSpacing = 5;

    // Details --------------------------------------------------------
    [System.Serializable]
    public class Detail {
        public GameObject prototype = null;
        public Texture2D prototypeTexture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 1;
        public Color dryColour = Color.white;
        public Color healthyColour = Color.white;
        public Vector2 heightRange = new Vector2(1, 1);
        public Vector2 widthRange = new Vector2(1, 1);
        public float noiseSpread = 0.5f;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float density = 0.5f;
        public bool remove = false;
    }

    public List<Detail> details = new List<Detail>() {
        new Detail()
    };
    public int maxDetails = 5000;
    public int detailSpacing = 5;

    //Water----------------------------
    public float waterHeight = 0.5f;
    public GameObject waterGO;
    public Material shoreLineMaterial;
    
    //Erosion ----------------------------
    public enum ErosionType{Rain = 0, Thermal = 1,Tidal = 2,
                            River = 3, Wind = 4}
    public ErosionType erosionType = ErosionType.Rain;
    public float erosionStrength = 0.1f;
    public int springsPerRiver = 5;
    public float solubility = 0.01f;
    public int droplets = 10;
    public int erosionSmoothAmount = 5;
    public float erosionAmount = 0.01f;


    #endregion
    public void Erode()
    {
        if (erosionType == ErosionType.Rain)
            Rain();
        else if (erosionType == ErosionType.Tidal)
            Tidal();
        else if (erosionType == ErosionType.Thermal)
            Thermal();
        else if (erosionType == ErosionType.River)
            River();
        else if (erosionType == ErosionType.Wind)
            Wind();
            
        smoothAmount = erosionSmoothAmount;
        Smooth();
    }
    void Rain()
    {

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                          terrainData.heightmapHeight);
        for (int i = 0; i < droplets; i++)
        {
           heightMap[UnityEngine.Random.Range(0,terrainData.heightmapWidth),
                     UnityEngine.Random.Range(0,terrainData.heightmapHeight)] -= erosionStrength;
        }
        terrainData.SetHeights(0,0,heightMap);
    }
    void Thermal()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                          terrainData.heightmapHeight);

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation,
                                                              terrainData.heightmapWidth,
                                                              terrainData.heightmapHeight);

                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] > heightMap[(int)n.x, (int)n.y] + erosionStrength)
                    {
                        float currentHeight = heightMap[x, y];
                        heightMap[x, y] -= currentHeight * erosionAmount;
                        heightMap[(int)n.x, (int)n.y] += currentHeight * erosionAmount;
                    }
                }
            }

        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    void Tidal()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                           terrainData.heightmapHeight);

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation,
                                                              terrainData.heightmapWidth,
                                                              terrainData.heightmapHeight);

                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {
                        heightMap[x, y] = waterHeight;
                        heightMap[(int)n.x, (int)n.y] = waterHeight;
                    }
                }
            }
        }
        terrainData.SetHeights(0,0,heightMap);
    }
    void River()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                           terrainData.heightmapHeight);

        float[,] erosionMap = new float[terrainData.heightmapWidth,terrainData.heightmapHeight];

        for (int i = 0; i < droplets; i++)
        {
            Vector2 dropletPosition = new Vector2(UnityEngine.Random.Range(0,terrainData.heightmapWidth),
                                                  UnityEngine.Random.Range(0,terrainData.heightmapHeight));
            erosionMap[(int)dropletPosition.x,(int) dropletPosition.y] = erosionStrength;
            for (int j = 0; j < springsPerRiver; j++)
            {
                erosionMap = RunRiver(dropletPosition,heightMap,erosionMap,
                                      terrainData.heightmapWidth,terrainData.heightmapHeight);
                
            } 
            
        }

        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                if (erosionMap[x,y] > 0)
                {
                    heightMap[x,y] -= erosionMap[x,y];
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    float[,] RunRiver(Vector3 dropletPosition, float[,] heighMap, float[,] erosionMap,int width, int height)
    {
        while (erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] > 0)
        {
            List<Vector2> neighbours = GenerateNeighbours(dropletPosition, width, height);
            neighbours.Shuffle();
            bool foundLower = false;
            foreach (Vector2 n in neighbours)
            {
                if (heighMap[(int)n.x, (int)n.y] < 
                    heighMap[(int)dropletPosition.x, (int)dropletPosition.y])
                {
                    erosionMap[(int)n.x,(int)n.y] = erosionMap[(int)dropletPosition.x, 
                                                               (int)dropletPosition.y] - solubility;
                    dropletPosition = n;
                    foundLower = true;
                    break;
                }
            }
            if (!foundLower)
            {
               erosionMap[(int)dropletPosition.x,(int) dropletPosition.y] -= solubility;
            }
        }
        return erosionMap;
    }
    void Wind()
    {
        
    }
    public void AddWater()
    {
        GameObject water = GameObject.Find("water");
        if (!water)
        {
            water = Instantiate(waterGO, this.transform.position, this.transform.rotation);
            water.name = "water";
        }
        water.transform.position = this.transform.position + 
                        new Vector3(terrainData.size.x / 2, 
                                    waterHeight * terrainData.size.y, 
                                    terrainData.size.z / 2);
        water.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);
    }
    public void DrawShoreline()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                                    terrainData.heightmapHeight);

        int quadCount = 0;
        //GameObject quads = new GameObject("QUADS");
        for (int y = 0; y < terrainData.heightmapHeight; y++)
        {
            for (int x = 0; x < terrainData.heightmapWidth; x++)
            {
                //find spot on shore
                Vector2 thisLocation = new Vector2(x, y);
                List<Vector2> neighbours = GenerateNeighbours(thisLocation,
                                                              terrainData.heightmapWidth,
                                                              terrainData.heightmapHeight);
                foreach (Vector2 n in neighbours)
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {
                        //if (quadCount < 1000)
                        //{
                            quadCount++;
                            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            go.transform.localScale *= 10.0f;

                            go.transform.position = this.transform.position +
                                            new Vector3(y / (float)terrainData.heightmapHeight
                                                          * terrainData.size.z,
                                                        waterHeight * terrainData.size.y,
                                                        x / (float)terrainData.heightmapWidth
                                                          * terrainData.size.x);

                            go.transform.LookAt(new Vector3(n.y / (float)terrainData.heightmapHeight
                                                                * terrainData.size.z,
                                                            waterHeight * terrainData.size.y,
                                                            n.x / (float)terrainData.heightmapWidth
                                                                * terrainData.size.x));    

                            go.transform.Rotate(90, 0, 0);

                            go.tag = "Shore";


                            //go.transform.parent = quads.transform;
                       // }
                    }
                }
            }
        }

        GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");
        MeshFilter[] meshFilters = new MeshFilter[shoreQuads.Length];
        for (int m = 0; m < shoreQuads.Length; m++)
        {
            meshFilters[m] = shoreQuads[m].GetComponent<MeshFilter>();
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        GameObject currentShoreLine = GameObject.Find("ShoreLine");
        if (currentShoreLine)
        {
            DestroyImmediate(currentShoreLine);
        }
        GameObject shoreLine = new GameObject();
        shoreLine.name = "ShoreLine";
        shoreLine.AddComponent<WaveAnimation>();
        shoreLine.transform.position = this.transform.position;
        shoreLine.transform.rotation = this.transform.rotation;
        MeshFilter thisMF = shoreLine.AddComponent<MeshFilter>();
        thisMF.mesh = new Mesh();
        shoreLine.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);

        MeshRenderer r = shoreLine.AddComponent<MeshRenderer>();
        r.sharedMaterial = shoreLineMaterial;

        for (int sQ = 0; sQ < shoreQuads.Length; sQ++)
            DestroyImmediate(shoreQuads[sQ]);


    }
    public void AddDetails() {
        DetailPrototype[] newDetailPrototypes;
        newDetailPrototypes = new DetailPrototype[details.Count];
        int dIndex = 0;
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth,
                                            terrainData.heightmapHeight);

        foreach (Detail d in details) {
            newDetailPrototypes[dIndex] = new DetailPrototype();
            newDetailPrototypes[dIndex].prototype = d.prototype;
            newDetailPrototypes[dIndex].prototypeTexture = d.prototypeTexture;
            newDetailPrototypes[dIndex].healthyColor = d.healthyColour;
            newDetailPrototypes[dIndex].dryColor = d.dryColour;
            newDetailPrototypes[dIndex].minHeight = d.heightRange.x;
            newDetailPrototypes[dIndex].maxHeight = d.heightRange.y;
            newDetailPrototypes[dIndex].minWidth = d.widthRange.x;
            newDetailPrototypes[dIndex].maxWidth = d.widthRange.y;
            newDetailPrototypes[dIndex].noiseSpread = d.noiseSpread;

            if (newDetailPrototypes[dIndex].prototype) {
                newDetailPrototypes[dIndex].usePrototypeMesh = true;
                newDetailPrototypes[dIndex].renderMode = DetailRenderMode.VertexLit;
            } else {
                newDetailPrototypes[dIndex].usePrototypeMesh = false;
                newDetailPrototypes[dIndex].renderMode = DetailRenderMode.GrassBillboard;
            }
            dIndex++;
        }
        terrainData.detailPrototypes = newDetailPrototypes;

        for(int i = 0; i < terrainData.detailPrototypes.Length; ++i) {
            int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];
            for(int y = 0; y < terrainData.detailHeight; y += detailSpacing) {
                for(int x = 0; x < terrainData.detailWidth; x += detailSpacing) {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > details[i].density) continue;
        
                    int xHM = (int)(x / (float)terrainData.detailWidth * terrainData.heightmapWidth);
                    int yHM = (int)(y / (float)terrainData.detailHeight * terrainData.heightmapHeight);

                    float thisNoise = Utils.Map(Mathf.PerlinNoise(x * details[i].feather,
                                                y * details[i].feather), 0, 1, 0.5f, 1);
                    float thisHeightStart = details[i].minHeight * thisNoise -
                                            details[i].overlap * thisNoise;
                    float nextHeightStart = details[i].maxHeight * thisNoise +
                                            details[i].overlap* thisNoise;

                    float thisHeight = heightMap[yHM, xHM];
                    float steepness = terrainData.GetSteepness( xHM / (float)terrainData.size.x,
                                                                yHM / (float)terrainData.size.z);
                    if((thisHeight >= thisHeightStart && thisHeight <= nextHeightStart) &&
                        (steepness >= details[i].minSlope && steepness <= details[i].maxSlope)) {
                        detailMap[y, x] = 1;
                    }
                }
            }
            terrainData.SetDetailLayer(0, 0, i, detailMap);
        }
    }
    public void AddNewDetails() {
        details.Add(new Detail());
    }
    public void RemoveDetails() {
        List<Detail> keptDetails = new List<Detail>();
        for (int i = 0; i < details.Count; ++i) {
            if (!details[i].remove) {
                keptDetails.Add(details[i]);
            }
        }
        if (keptDetails.Count == 0) {    // Don't want to keep any
            keptDetails.Add(details[0]);  // Add at least one;
        }
        details = keptDetails;
    }
    public void AddVegetation()
    {
        vegetations.Add(new Vegetation());
    }
    public void RemoveVegetation()
    {
        List<Vegetation> kept = new List<Vegetation>();
        for (int i = 0; i < vegetations.Count; i++)
        {
            if (!vegetations[i].remove)
            {
                kept.Add(vegetations[i]);
            }
        }
        if (kept.Count == 0)
        {
            kept.Add(vegetations[0]);
        }
        vegetations = kept;
    }
    public void applyVegetation()
    {
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetations.Count];
        int tindex = 0;
        foreach (Vegetation t in vegetations)
        {
            newTreePrototypes[tindex] = new TreePrototype();
            newTreePrototypes[tindex].prefab = t.mesh;
            tindex++;
        }
        terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        for (int z = 0; z < terrainData.size.z; z += treeSpacing)
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing)
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetations[tp].density) break;

                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float thisHeightStart = vegetations[tp].minHeight;
                    float thisHeightEnd = vegetations[tp].maxHeight;

                    float steepness = terrainData.GetSteepness(x / (float)terrainData.size.x,
                                                               z / (float)terrainData.size.z);

                    if ((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) &&
                        (steepness >= vegetations[tp].minSlope && steepness <= vegetations[tp].maxSlope))
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.x,
                                                        terrainData.GetHeight(x, z) / terrainData.size.y,
                                                        (z + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.size.z);

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.size.x,
                            instance.position.y * terrainData.size.y,
                            instance.position.z * terrainData.size.z)
                                                         + this.transform.position;

                        RaycastHit hit;
                        int layerMask = 1 << terrainLayer;

                        if (Physics.Raycast(treeWorldPos + new Vector3(0, 10, 0), -Vector3.up, out hit, 100, layerMask) ||
                            Physics.Raycast(treeWorldPos - new Vector3(0, 10, 0), Vector3.up, out hit, 100, layerMask))
                        {
                            float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                            instance.position = new Vector3(instance.position.x,
                                                             treeHeight,
                                                             instance.position.z);

                            instance.rotation = UnityEngine.Random.Range(vegetations[tp].minRotation,
                                                                         vegetations[tp].maxRotation);
                            instance.prototypeIndex = tp;
                            instance.color = Color.Lerp(vegetations[tp].colour1,
                                                        vegetations[tp].colour2,
                                                        UnityEngine.Random.Range(0.0f, 1.0f));
                            instance.lightmapColor = vegetations[tp].lightColour;
                            float s = UnityEngine.Random.Range(vegetations[tp].minScale, vegetations[tp].maxScale);
                            instance.heightScale = s;
                            instance.widthScale = s;

                            allVegetation.Add(instance);
                            if (allVegetation.Count >= maximumTrees) goto TREESDONE;
                        }


                    }
                }
            }
        }
    TREESDONE:
        terrainData.treeInstances = allVegetation.ToArray();
    }
    public void AddNewSplatHeight()
    {
        splatHeightsList.Add(new splatHeights());
    }
    public void RemoveSplatHeight()
    {
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
    float GetSteepness(float[,] heightmap, int x, int y, int width, int height)
    {
        float h = heightmap[x, y];
        int nx = x + 1;
        int ny = y + 1;
        //if on the edge
        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightmap[nx, y] - h;
        float dy = heightmap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);
        float steep = gradient.magnitude;
        return steep;
    }
    public void SplatMaps()
    {
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
        float[,] heightMap = terrainData.GetHeights(0, 0,
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
                    float noise = Mathf.PerlinNoise(x * splatHeightsList[i].noiseXScale
                                                   , y * splatHeightsList[i].noiseYScale)
                                                    * splatHeightsList[i].noiseScaler;
                    float offSet = splatHeightsList[i].splatOffSet + noise;
                    float thisHeightStart = splatHeightsList[i].minHeight - offSet;
                    float thisHeightStop = splatHeightsList[i].maxHeight + offSet;
                    //float steepness = GetSteepness(heightMap,x,y,
                    //                              terrainData.heightmapWidth,
                    //                               terrainData.heightmapHeight);
                    float steepness = terrainData.GetSteepness(y / (float)terrainData.alphamapHeight,
                                                               x / (float)terrainData.alphamapWidth);

                    if ((heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop) &&
                        steepness >= splatHeightsList[i].minSlope &&
                        steepness <= splatHeightsList[i].maxSlope)
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeightsList.Count; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
    void NormalizeVector(float[] vector)
    {
        float total = 0;
        for (int i = 0; i < vector.Length; i++)
        {
            total += vector[i];
        }
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] /= total;
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
    public void Smooth()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0,
        terrainData.heightmapWidth, terrainData.heightmapHeight);
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
                                             smoothProgress / smoothAmount);

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

        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);

        tagManager.ApplyModifiedProperties();

        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
    }
    int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
    {
        bool found = false;
        //ensure the tag doesn't already exist
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; return i; }
        }
        //add your new tag
        if (!found && tType == TagType.Tag)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
        //add new layer
        else if (!found && tType == TagType.Layer)
        {
            for (int j = 8; j < tagsProp.arraySize; j++)
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                //add layer in next empty slot
                if (newLayer.stringValue == "")
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }
        return -1;
    }

}
