using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI() {

        /* everytime the "Map" is selected, it is generated which makes it so recoure consumer
        base.OnInspectorGUI();
        MapGenerator map = target as MapGenerator;
        map.GenerateMap();
        */

        //This way Map is regenerated if only a parameter is changed
        MapGenerator map = target as MapGenerator;

        if (DrawDefaultInspector()) {
            map.GenerateMap();
        }

        //but what if the code is changed? let's generate by a button
        if (GUILayout.Button("Generate Map")) {
            map.GenerateMap();
        }
    }
}
