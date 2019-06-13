using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnInspectorGUI(){
        MapGenerator map = target as MapGenerator;
        if (DrawDefaultInspector()){
            map.GenerateMap();
        }
        if(GUILayout.Button("Generate Map")){
            map.GenerateMap();
        }
        
    }
}
