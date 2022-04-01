using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    public class MainSettings : ScriptableObject
    {
        public Material walkableMaterial;
        public Material unwalkableMaterial;
        public Material selectionMaterial;
        public float visualizedNodeOffset = .05f;
        public float visualizedNodeHeight = .1f;
        public bool updateVisualizerInRealtime = false;
    }
}