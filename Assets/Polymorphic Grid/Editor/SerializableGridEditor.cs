using UnityEditor;

namespace TheoryTeam.PolymorphicGrid
{
    [CustomEditor(typeof(SerializableGrid))]
    public class SerializableGridEditor : Editor
    {
        private SerializableGrid grid;

        private void OnEnable()
        {
            grid = target as SerializableGrid;
        }

        public override void OnInspectorGUI()
        {
            if (grid.serializedNodes == null || grid.serializedNodes.Length == 0)
                EditorGUILayout.HelpBox("No stored data found in this file!", MessageType.Warning);
            else
                EditorGUILayout.HelpBox($"Found grid with node radius {grid.nodeRadius}m and {grid.serializedNodes.Length} nodes", MessageType.Info);
        }
    }
}