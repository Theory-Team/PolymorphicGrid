using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    public class TriangleGrid : NonSymmetricalGenericGrid
    {
        [SerializeField]
        private bool edgeNeighborsOnly;

        public override bool EdgeNeighborsOnly { get => edgeNeighborsOnly; set => edgeNeighborsOnly = value; }

        public override Vector3[] GetNodeCorners()
        {
            float y = -GeneratedNodeRadius * .5f;
            float x = GeneratedNodeRadius * .866025f;

            return new Vector3[3]
            {
                new Vector3(0f, 0f, GeneratedNodeRadius),
                new Vector3(-x, 0f, y),
                new Vector3(x, 0f, y)
            };
        }
    }
}