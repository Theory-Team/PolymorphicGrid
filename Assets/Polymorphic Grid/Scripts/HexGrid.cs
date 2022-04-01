using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    public class HexGrid : GenericGrid
    {
        public override bool EdgeNeighborsOnly
        {
            get => true;
            set => throw new System.NotSupportedException("Hex grid must be edge neighbors only!");
        }

        public override Vector3[] GetNodeCorners()
        {
            float x = GeneratedNodeRadius * .5f;
            float y = GeneratedNodeRadius * .866025f;

            return new Vector3[6] {
                new Vector3(GeneratedNodeRadius, 0f, 0f),
                new Vector3(x, 0f, y),
                new Vector3(-x, 0f, y),
                new Vector3(-GeneratedNodeRadius, 0f, 0f),
                new Vector3(-x, 0f, -y),
                new Vector3(x, 0f, -y)
            };
        }
    }
}