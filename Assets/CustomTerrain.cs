using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour {

	public Vector2 randomHeightRange = new Vector2(0,0.1f);
	public Texture2D heightMapImage;
	public Vector3 heightMapScale = new Vector3(1,1,1);
	public bool resetTerrainCheckBox = true;

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
	public class PerlinParameters{
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

	//Terrain references--------------------/
	public Terrain terrain;
	public TerrainData terrainData;

	float[,] getHeightMap(){

		float[,] HeightMap = new float[0,0];

		if (!resetTerrainCheckBox)
		{
			HeightMap = terrainData.GetHeights(0,0,terrainData.heightmapWidth,
		                                                terrainData.heightmapHeight);
		}
		else
		{
			HeightMap = new float[terrainData.heightmapWidth,terrainData.heightmapHeight];
		}
			return HeightMap;
	}

	public void perlin(){
		float[,] heightMap = getHeightMap();
		for (int y = 0; y < terrainData.heightmapHeight; y++)
		{
			for (int x = 0; x < terrainData.heightmapWidth; x++)
			{
				heightMap[x,y] += Utils.FractionalBM((x + perlinOffsetX) * perlinXScale,
				                                    (y + perlinOffsetY) * perlinYScale,
				                                    perlinOctaves,
													perlinPersistence)* perlinHeightScale;
			}
		}
		terrainData.SetHeights(0,0,heightMap);												
	}

	public void MultiplePerlin(){
		float[,] heightMap = getHeightMap();
		for (int y = 0; y < terrainData.heightmapHeight; y++)
		{
			for (int x = 0; x < terrainData.heightmapWidth; x++)
			{
				foreach (PerlinParameters p in perlinParameters)
				{
					heightMap[x,y] += Utils.FractionalBM((x + p.mPerlinOffsetX) * p.mPerlinXscale,
					                                     (y + p.mPerlinOffsetY) * p.mPerlinYscale,
														 p.mPerlinOctaves,
														 p.mPerlinPersistence) * p.mPerlinHeightScale;
				}
			}
		}
		terrainData.SetHeights(0,0,heightMap);
	}
	
	public void addNewPerlin(){
		perlinParameters.Add(new PerlinParameters());
	}

	public void removeNewPerlin(){
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
	public void RandomTerrain(){

		float[,] heightMap = getHeightMap();
		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int y = 0; y < terrainData.heightmapHeight; y++)
			{
				heightMap[x,y] += UnityEngine.Random.Range(randomHeightRange.x,randomHeightRange.y);
			}
		}
		terrainData.SetHeights(0,0,heightMap);
	}
	
	public void LoadTexture(){
		float[,] heightMap = getHeightMap();

		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int y = 0; y < terrainData.heightmapHeight; y++)
			{
				heightMap[x,y] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
				                                (int)(y*heightMapScale.z)).grayscale * heightMapScale.y;
			}
		}
		terrainData.SetHeights(0,0,heightMap);

	}
	public void resetTerrain(){

        float[,] heightMap = new float[terrainData.heightmapWidth,terrainData.heightmapHeight];

		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int y = 0; y < terrainData.heightmapHeight; y++)
			{
				heightMap[x,y] = 0;
			}
		}
		terrainData.SetHeights(0,0,heightMap);
	}

	private void OnEnable() {
		Debug.Log("Initialising Terrain Data");
		terrain = this.GetComponent<Terrain>();
		terrainData = Terrain.activeTerrain.terrainData; //becare 
	}


	private void Awake() {
		SerializedObject tagManager = new SerializedObject(
			AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty tagsProp = tagManager.FindProperty("tags");

		Addtag(tagsProp,"Terrain");
		Addtag(tagsProp,"Cloud");
		Addtag(tagsProp,"Shore");

		tagManager.ApplyModifiedProperties();

		this.gameObject.tag ="Terrain";
	}

	void Addtag(SerializedProperty tagsProp, string newTag){
		bool found = false;
		//look if tag exists
		for (int i = 0; i < tagsProp.arraySize; i++)
		{
			SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
			if(t.stringValue.Equals(newTag)){
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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
