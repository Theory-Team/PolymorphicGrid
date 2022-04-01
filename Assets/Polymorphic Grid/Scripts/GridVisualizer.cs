using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    /// <summary>
    /// Class to visualize your grid in the scene and during gameplay.
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        /// <summary>
        /// All materials used by this visualizer.
        /// </summary>
        [Tooltip("All materials used by this visualizer.")]
        public List<Material> materials = new List<Material>();
        /// <summary>
        /// Node visualizer mesh position offset from original node position.
        /// </summary>
        [Tooltip("Node visualizer mesh position offset from original node position.")]
        public float visualizedNodeOffset = .11f;
        /// <summary>
        /// Node visualizer mesh height.
        /// </summary>
        [Tooltip("Node visualizer mesh height.")]
        public float visualizedNodeHeight = .2f;
        /// <summary>
        /// Automatically update visualizer meshes when updating targeted grid.
        /// </summary>
        [Tooltip("Automatically update visualizer meshes when updating targeted grid.")]
        public bool autoGenerate = true;

        private GridMaster grid;
        private MeshRenderer[] nodes;
        private int[] materialIndex;

        /// <summary>
        /// Target grid to visualize.
        /// </summary>
        public GridMaster Grid
        {
            get => grid;
            set
            {
                grid = value;
                if (autoGenerate)
                    UpdateMeshes();
            }
        }

        private void UpdateMaterial(int nodeIndex, int materialIndex)
        {
            int mIndex = Mathf.Clamp(materialIndex, 0, materials.Count - 1);
            if (materials[mIndex] == null)
                nodes[nodeIndex].gameObject.SetActive(false);
            else
            {
                nodes[nodeIndex].gameObject.SetActive(true);
                nodes[nodeIndex].sharedMaterial = materials[mIndex];
            }
        }

        /// <summary>
        /// Regenerate all visualizer meshes to match targeted grid.
        /// </summary>
        public void UpdateMeshes()
        {
            ClearMeshes();
            Mesh mesh = CreateNodeMesh();
            nodes = new MeshRenderer[grid.MaxSize];
            materialIndex = new int[grid.MaxSize];
            GameObject currentNode;
            int i = 0;

            foreach (Node n in grid.Nodes)
            {
                currentNode = new GameObject($"Visualized Node {i}");
                currentNode.transform.position = n.WorldPosition;
                currentNode.transform.rotation = transform.rotation;
                currentNode.transform.parent = transform;
                currentNode.AddComponent<MeshFilter>().sharedMesh = Grid is NonSymmetricalGenericGrid ? CreateNodeMesh(i) : mesh;
                nodes[i] = currentNode.AddComponent<MeshRenderer>();
                materialIndex[i] = 0;
                i++;
            }
        }

        /// <summary>
        /// Clear all generated meshes by the visualizer.
        /// </summary>
        public void ClearMeshes()
        {
            if (nodes != null)
            {
                if (Application.isPlaying)
                {
                    foreach (MeshRenderer node in nodes)
                        Destroy(node.gameObject);
                }
                else
                {
                    foreach (MeshRenderer node in nodes)
                        DestroyImmediate(node.gameObject);
                }
                nodes = null;
            }
        }

        /// <summary>
        /// Set material for the mesh associated by the given node.
        /// </summary>
        /// <param name="n">New material index</param>
        /// <param name="material"></param>
        public void SetMaterial(Node n, int material)
        {
            materialIndex[n.Index] = Mathf.Clamp(material, 0, materials.Count - 1);
            UpdateMaterial(n.Index, material);
        }

        /// <summary>
        /// Get material for the mesh associated by the given node.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public int GetMaterial(Node n) => materialIndex[n.Index];

        /// <summary>
        /// Create visualizer mesh for the given node index.
        /// </summary>
        /// <param name="index">Node index to visualize</param>
        /// <returns></returns>
        public Mesh CreateNodeMesh(int index = -1)
        {
            Vector3[] corners = index == -1 ? Grid.GetNodeCorners() : Grid.GetNodeCorners(Grid.GetNode(index)).ToArray();
            if (index != -1)
                corners = (from corner in corners select corner - Grid.GetNode(index).localPosition).ToArray();

            Vector3[] downLoop = TheoryMath.ScaleAtCenter(1 - visualizedNodeOffset, corners);
            Vector3[] upLoop = (from p in downLoop select p + Vector3.up * visualizedNodeHeight).ToArray();
            return TheoryMath.CombineMeshes(TheoryMath.BridgeEdgeLoops(downLoop, upLoop), TheoryMath.FilllAtCenter(upLoop));
        }

        /// <summary>
        /// Update all visualizer materials.
        /// </summary>
        public void UpdateAllMaterials()
        {
            int size = Grid.MaxSize;
            for (int i = 0; i < size; i++)
                UpdateMaterial(i, materialIndex[i]);
        }

        /// <summary>
        /// Get visualizer node associated with target index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MeshRenderer GetRenderer(int index) => nodes[index];
    }
}