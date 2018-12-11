using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor {

	//Properties-------/
	SerializedProperty randomHeightRange;//reference for the script and the GUI
	SerializedProperty heightMapScale;
	SerializedProperty heightMapImage;

	//Foldouts--------/
	bool showRandom = false; 
	bool showLoadHeights = false;

	private void OnEnable() {
		randomHeightRange = serializedObject.FindProperty("randomHeightRange");
		heightMapScale = serializedObject.FindProperty("heightMapScale");
		heightMapImage = serializedObject.FindProperty("heightMapImage");
	}
    
	//The GUI Loop, it gets update
	public override void OnInspectorGUI(){
		serializedObject.Update();//F

		CustomTerrain terrain = (CustomTerrain) target;//Reference for our SCRIPT

        //Foldout on GUI
		showRandom = EditorGUILayout.Foldout(showRandom,"Random Heights");
		if (showRandom)//A foldout need to be followed by a if statement
		{
			EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
			GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(randomHeightRange); //!! link GUI values to our variable
			if (GUILayout.Button("Random Heights"))
			{
				terrain.RandomTerrain();//Method from our customTerrain Script to update our terrain.
			}
		}
        
		//Foldout to HeightMap
		showLoadHeights = EditorGUILayout.Foldout(showLoadHeights,"Load Heights");
		if (showLoadHeights)//A foldout need to be followed by a if statement
		{
			EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
			GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(heightMapImage);
			EditorGUILayout.PropertyField(heightMapScale);
			if (GUILayout.Button("Load Texture"))
			{
				terrain.LoadTexture();
			}
		}


        
		//resetButton
		EditorGUILayout.LabelField("",GUI.skin.horizontalSlider);
		if (GUILayout.Button("Reset Terrain"))
		{
			terrain.resetTerrain();
		}
		



		serializedObject.ApplyModifiedProperties();//L
	}
}
