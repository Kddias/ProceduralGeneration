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
	public Terrain terrain;
	public TerrainData terrainData;


    //att our terrain
	public void RandomTerrain(){

		float[,] heightMap = terrainData.GetHeights(0,0,terrainData.heightmapWidth,
		                                                terrainData.heightmapHeight);
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
		float[,] heightMap = new float[terrainData.heightmapWidth,terrainData.heightmapHeight];

		for (int x = 0; x < terrainData.heightmapWidth; x++)
		{
			for (int y = 0; y < terrainData.heightmapHeight; y++)
			{
				heightMap[x,y] = heightMapImage.GetPixel((int)(x * heightMapScale.x),
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
