using UnityEngine;
using UnityEditor;
using EditorGUITable;

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
    // --------------------------/
    
    SerializedProperty perlinXScale, perlinYScale,
                       perlinOffsetX, perlinOffsetY,
                       perlinOctaves, perlinPersistence,
                       perlinHeightScale;

    GUITableState perlinParametersTable;
    SerializedProperty perlinParameters;

    //Foldouts--------/
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlinNoise = false;
    bool showMultiplePerlins = false;
    bool showVoronoi = true;
    bool showMPD = true;

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
        //Voronoi-----------/
        VoronoiPeakCount = serializedObject.FindProperty("peakCount");
        VoronoiFallOff = serializedObject.FindProperty("fallOff");
        VoronoiDropOff = serializedObject.FindProperty("dropOff");
        VoronoiminHeight = serializedObject.FindProperty("minHeight");
        VoronoiMaxHeight = serializedObject.FindProperty("maxHeight");
        VoronoiType = serializedObject.FindProperty("voronoiType");
        

    }

    //The GUI Loop, it gets update
    public override void OnInspectorGUI()
    {
        serializedObject.Update();//F

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

        showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
        if (showMPD)
        {
            if (GUILayout.Button("MPD"))
            {
                terrain.midPointDisplacement();
            }
            
        }

        
        //resetButton
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.resetTerrain();
        }
        serializedObject.ApplyModifiedProperties();//L
    }
}
