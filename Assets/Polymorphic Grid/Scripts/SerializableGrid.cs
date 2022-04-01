using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    public class SerializableGrid : ScriptableObject
    {
        public float nodeRadius;
        public SerializableNode[] serializedNodes;
        public bool createNodeObject;
        public string gridTypeName;
    }
}