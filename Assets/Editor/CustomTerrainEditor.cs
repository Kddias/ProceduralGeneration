using UnityEngine;
using UnityEditor;
using EditorGUITable;
using System.IO;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{

    //Properties-------/
    SerializedProperty randomHeightRange;//reference for the script and the GUI
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty resetTerrainCheckBox;

    //Voronoi Properties---------/
    SerializedProperty VoronoiPeakCount;
    SerializedProperty VoronoiFallOff;
    SerializedProperty VoronoiDropOff;
    SerializedProperty VoronoiminHeight;
    SerializedProperty VoronoiMaxHeight;
    SerializedProperty VoronoiType;

    // MPD --------------------------/
    SerializedProperty MPDminHeight;
    SerializedProperty MPDmaxHeight;
    SerializedProperty MPDRoughness;
    SerializedProperty MPDHeightDampener;

    //Smooth--------------------------------/
    SerializedProperty smoothAmount;

    //---Perlin------
    SerializedProperty perlinXScale, perlinYScale,
                       perlinOffsetX, perlinOffsetY,
                       perlinOctaves, perlinPersistence,
                       perlinHeightScale;

    GUITableState perlinParametersTable;
    SerializedProperty perlinParameters;

    //SplatMaps
    GUITableState splatMapTable;
    SerializedProperty splatHeights;

    //HeightMap
    SerializedProperty heightMapTexture;

    //Vegetation
    SerializedProperty maximumTrees;
    SerializedProperty treeSpacing;
    GUITableState vegetations;
    SerializedProperty Vegetation;

    //Details
    GUITableState detailMapTable;
    SerializedProperty detail;
    SerializedProperty maxDetails;
    SerializedProperty detailSpacing;

    //Water----------------
    SerializedProperty waterHeight;
    SerializedProperty waterGO;
    SerializedProperty shoreLineMaterial;
    
    //Eroson---------------------------------
    SerializedProperty erosionType ;
    SerializedProperty erosionStrength ;
    SerializedProperty springsPerRiver;
    SerializedProperty solubility ;
    SerializedProperty droplets ;
    SerializedProperty erosionSmoothAmount;
    SerializedProperty erosionAmount;


    //Foldouts--------/
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;
    bool showMultiplePerlins = false;
    bool showVoronoi = false;
    bool showMPD = false;
    bool showSmooth = false;
    bool showSplatMaps = false;
    bool showVegetation = false;
    bool showHeightMap = false;
    bool showDetail = false;
    bool showWater = false;
    bool showErosion = false;

    //Other
    Texture2D hmTexture;

    //Link Our Variables
    private void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistence = serializedObject.FindProperty("perlinPersistence");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        resetTerrainCheckBox = serializedObject.FindProperty("resetTerrainCheckBox");
        perlinParametersTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");
        //SplatMaps----------/
        splatMapTable = new GUITableState("splatMapTable");
        splatHeights = serializedObject.FindProperty("splatHeights");
        //Voronoi-----------/
        VoronoiPeakCount = serializedObject.FindProperty("peakCount");
        VoronoiFallOff = serializedObject.FindProperty("fallOff");
        VoronoiDropOff = serializedObject.FindProperty("dropOff");
        VoronoiminHeight = serializedObject.FindProperty("minHeight");
        VoronoiMaxHeight = serializedObject.FindProperty("maxHeight");
        VoronoiType = serializedObject.FindProperty("voronoiType");
        //MPD-------------/
        MPDminHeight = serializedObject.FindProperty("MPDminHeight");
        MPDmaxHeight = serializedObject.FindProperty("MPDmaxHeight");
        MPDRoughness = serializedObject.FindProperty("MPDRoughness");
        MPDHeightDampener = serializedObject.FindProperty("MPDHeightDampener");
        //SMooth---------/
        smoothAmount = serializedObject.FindProperty("smoothAmount");
        //HeightMap------/
        hmTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
        //Vegetation
        maximumTrees = serializedObject.FindProperty("maximumTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");
        vegetations = new GUITableState("vegetations");
        Vegetation = serializedObject.FindProperty("Vegetation");
        //Details
        detailMapTable = new GUITableState("detailMapTable");
        detail = serializedObject.FindProperty("details");
        maxDetails = serializedObject.FindProperty("maxDetails");
        detailSpacing = serializedObject.FindProperty("detailSpacing");
        //Water
        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGO = serializedObject.FindProperty("waterGO");
        shoreLineMaterial = serializedObject.FindProperty("shoreLineMaterial");
        //Erosion
        erosionType = serializedObject.FindProperty("erosionType");
        erosionStrength = serializedObject.FindProperty("erosionStrength");
        springsPerRiver = serializedObject.FindProperty("springsPerRiver");
        solubility = serializedObject.FindProperty("solubility");
        droplets = serializedObject.FindProperty("droplets");
        erosionSmoothAmount = serializedObject.FindProperty("erosionSmoothAmount");
        erosionAmount = serializedObject.FindProperty("erosionAmount");




    }

    //The GUI Loop, it gets update
    public override void OnInspectorGUI()
    {
        serializedObject.Update();//Begin

        CustomTerrain terrain = (CustomTerrain)target;//Reference for our SCRIPT

        //CheckBox For resetTerrain
        EditorGUILayout.PropertyField(resetTerrainCheckBox);

        //Foldout on GUI
        showRandom = EditorGUILayout.Foldout(showRandom, "Random Heights");
        if (showRandom)//A foldout need to be followed by a if statement
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange); //!! link GUI values to our variable
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();//Method from our customTerrain Script to update our terrain.
            }
        }

        //Foldout PerlinNoise
        showPerlinNoise = EditorGUILayout.Foldout(showPerlinNoise, "Generate Perlin Noise");
        if (showPerlinNoise)//A foldout need to be followed by a if statement
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Randomize a new Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinXScale, 0, 1, new GUIContent("X Scale"));//value,min,max
            EditorGUILayout.Slider(perlinYScale, 0, 1, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 10000, new GUIContent("X OffSet"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 10000, new GUIContent("Y OffSet"));
            EditorGUILayout.IntSlider(perlinOctaves, 0, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistence, 0.1f, 10, new GUIContent("Persistence"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));
            if (GUILayout.Button("Generate Perlin"))
            {
                terrain.perlin();
            }
        }

        //Foldout MultiplePerlinNoise
        showMultiplePerlins = EditorGUILayout.Foldout(showMultiplePerlins, "Generate Multiple Perlin");
        if (showMultiplePerlins)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParametersTable = GUITableLayout.DrawTable(perlinParametersTable,
                                                        serializedObject.FindProperty("perlinParameters"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.addNewPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.removeNewPerlin();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlin();
            }
        }

     
        //Foldout to HeightMap
        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if (showLoadHeights)//A foldout need to be followed by a if statement
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        //Voronoi
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        if (showVoronoi)
        {
            GUILayout.Label("Generate Voronoi Peaks", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(VoronoiPeakCount, 1, 10, new GUIContent("Peak count"));
            EditorGUILayout.Slider(VoronoiFallOff, 0.1f, 10, new GUIContent("Falloff"));
            EditorGUILayout.Slider(VoronoiDropOff, 0.1f, 10, new GUIContent("DropOff"));
            EditorGUILayout.Slider(VoronoiminHeight, 0.0f, 1f, new GUIContent("Min Height"));
            EditorGUILayout.Slider(VoronoiMaxHeight, 0.0f, 1f, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(VoronoiType);
            if (GUILayout.Button("Voronoi"))
            {
                terrain.voronoi();
            }
        }
        
        //MPD
        showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
        if (showMPD)
        {
            EditorGUILayout.Slider(MPDminHeight, -10, 10, new GUIContent("Min Height"));
            EditorGUILayout.Slider(MPDmaxHeight, -10f, 10f, new GUIContent("Max Height"));
            EditorGUILayout.Slider(MPDRoughness, 0f, 100, new GUIContent("Roughness"));
            EditorGUILayout.Slider(MPDHeightDampener, 0f, 10, new GUIContent("Dampener"));
            if (GUILayout.Button("MPD"))
            {
                terrain.midPointDisplacement();
            }
            
        }

           //SplatMaps
        showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
        if (showSplatMaps)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splat Maps", EditorStyles.boldLabel);
            GUILayout.Space(10);
            splatMapTable = GUITableLayout.DrawTable(splatMapTable,
                                                        serializedObject.FindProperty("splatHeightsList"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewSplatHeight();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply SplatMaps"))
            {
                terrain.SplatMaps();
            }
            GUILayout.Space(20);
        }

        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
        if (showVegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Configure Vegetation", EditorStyles.boldLabel);
            GUILayout.Space(10);
            EditorGUILayout.IntSlider(maximumTrees, 1, 10000, new GUIContent("Maximum Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 1, 25, new GUIContent("Trees Spacing"));
            GUILayout.Space(10);
            vegetations = GUITableLayout.DrawTable(vegetations,
                                                        serializedObject.FindProperty("vegetations"));
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddVegetation();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveVegetation();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Vegetation"))
            {
                terrain.applyVegetation();
            }


        }
        
        //Details
        showDetail = EditorGUILayout.Foldout(showDetail, "Details");
        if (showDetail) {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Detail", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Maximum Details"));
            EditorGUILayout.IntSlider(detailSpacing, 1, 20, new GUIContent("Detail Spacing"));
            detailMapTable = GUITableLayout.DrawTable(detailMapTable,
                serializedObject.FindProperty("details"));

            terrain.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+")) {
                terrain.AddNewDetails();
            }
            if (GUILayout.Button("-")) {
                terrain.RemoveDetails();
            }

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Details")) {
                terrain.AddDetails();
            }
        }

        showWater = EditorGUILayout.Foldout(showWater, "Water");
        if (showWater)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Water", EditorStyles.boldLabel);
            EditorGUILayout.Slider(waterHeight, 0, 1, new GUIContent("Water Height"));
            EditorGUILayout.PropertyField(waterGO);

            if (GUILayout.Button("Add Water"))
            {
                terrain.AddWater();
            }

            EditorGUILayout.PropertyField(shoreLineMaterial);
            if (GUILayout.Button("Add Shoreline"))
            {
                terrain.DrawShoreline();
            }
        }

        //Erosion
        showErosion = EditorGUILayout.Foldout(showErosion,"Erosion");
        if (showErosion)
        {
            EditorGUILayout.PropertyField(erosionType);
            EditorGUILayout.Slider(erosionStrength, 0f, 1f, new GUIContent("Erosion Strength"));
            EditorGUILayout.IntSlider(droplets, 0, 500, new GUIContent("Droplets Count"));
            EditorGUILayout.Slider(solubility, 0.001f, 1f, new GUIContent("Solubility"));
            EditorGUILayout.IntSlider(springsPerRiver, 0, 20, new GUIContent("Springs Per River"));
            EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));
            EditorGUILayout.Slider(erosionAmount,0.001f, 1f, new GUIContent("Erosion Amount"));
            if (GUILayout.Button("Erode"))
            {
                terrain.Erode();
            }
        }


        //SmoothButton
        showSmooth = EditorGUILayout.Foldout(showSmooth,"Smooth");
        if (showSmooth)
        {
            EditorGUILayout.PropertyField(smoothAmount);
            if (GUILayout.Button("Smooth"))
            {
                terrain.Smooth();
            }
        }

        //resetButton
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.resetTerrain();
        }


        //Height Map
        showHeightMap = EditorGUILayout.Foldout(showHeightMap, "Height Map");
        if (showHeightMap)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            int hmtSize = (int)(EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Label(hmTexture, GUILayout.Width(hmtSize), GUILayout.Height(hmtSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", GUILayout.Width(hmtSize)))
            {
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0, 
                                                                    terrain.terrainData.heightmapWidth, 
                                                                    terrain.terrainData.heightmapHeight);
                for (int y = 0; y < terrain.terrainData.heightmapWidth; y++)
                {
                    for (int x = 0; x < terrain.terrainData.heightmapHeight; x++)
                    {
                        hmTexture.SetPixel(x, y, new Color(heightMap[x, y], 
                                                           heightMap[x, y], 
                                                           heightMap[x, y], 1));
                    }
                }
                hmTexture.Apply();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save", GUILayout.Width(hmtSize)))
            {
                byte[] bytes = hmTexture.EncodeToPNG();
                System.IO.Directory.CreateDirectory(Application.dataPath + "/SavedTextures");
                File.WriteAllBytes(Application.dataPath + "/SavedTextures/" + "MyTexture" + ".png", bytes);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


        }

        serializedObject.ApplyModifiedProperties(); // END
    }
}
