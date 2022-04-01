using UnityEngine;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    [Serializable]
    public struct SerializableNode
    {
        public bool walkable;
        public Vector3 localPosition;
        public int[] neighbors;
        public Vector3[] vertices;
        public Quaternion worldObjectRotation;
        public Vector3 worldObjectScale;

        public SerializableNode(in Node n)
        {
            walkable = n.walkable;
            localPosition = n.localPosition;
            neighbors = n.neighbors.ToArray();
            vertices = n.vertices == null ? null : (Vector3[])n.vertices.Clone();
            worldObjectRotation = n.worldObject == null ? Quaternion.identity : n.worldObject.localRotation;
            worldObjectScale = n.worldObject == null ? Vector3.one : n.worldObject.localScale;
        }
    }
}