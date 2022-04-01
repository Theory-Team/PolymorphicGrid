using System.Collections.Generic;
using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    public class VisualizerTest : MonoBehaviour
    {
        public enum VisualizeType { Neighbors, Distance, Path, AllPathes }

        public VisualizeType visualizeType;
        public Material defaultMat;
        public Material selectionMat;
        public Material markMat;
        public Material markOtherMat;
        public GridMaster grid;
        public GameObject arrowPrefab;

        private List<GameObject> arrows = new List<GameObject>();
        private GridVisualizer visualizer;
        private Node hoveredNode;
        private int prevMat = 0;
        private Node first;
        private Node last;

        private void Start()
        {
            visualizer = gameObject.AddComponent<GridVisualizer>();
            visualizer.materials.Add(defaultMat);
            visualizer.materials.Add(selectionMat);
            visualizer.materials.Add(markMat);
            visualizer.materials.Add(markOtherMat);
            visualizer.Grid = grid;
            visualizer.UpdateAllMaterials();
            hoveredNode = null;
            first = null;
            last = null;
        }

        private void Update()
        {
            UpdateHovered();

            if (Input.GetMouseButtonUp(0))
            {
                switch (visualizeType)
                {
                    case VisualizeType.Neighbors:
                        HandleNeighborsVisualizer();
                        break;
                    case VisualizeType.Distance:
                        HandleDistanceVisualizer();
                        break;
                    case VisualizeType.Path:
                        HandlePathVisualizer();
                        break;
                    case VisualizeType.AllPathes:
                        HandleAllPathesVisualizer();
                        break;
                }
            }

            visualizer.UpdateAllMaterials();
        }

        private void UpdateHovered()
        {
            if (hoveredNode != null)
                visualizer.SetMaterial(hoveredNode, prevMat);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance = -ray.origin.y / ray.direction.y;
            hoveredNode = grid.GetNode(ray.GetPoint(distance));

            if (hoveredNode != null)
            {
                prevMat = visualizer.GetMaterial(hoveredNode);
                visualizer.SetMaterial(hoveredNode, 1);
            }
        }

        private void HandleNeighborsVisualizer()
        {
            for (int i = 0; i < grid.MaxSize; i++)
                visualizer.SetMaterial(grid.GetNode(i), 0);

            foreach (Node neighbors in grid.GetNeighbors(hoveredNode))
                visualizer.SetMaterial(neighbors, 2);

            visualizer.SetMaterial(hoveredNode, 3);
            prevMat = 3;
        }

        private void HandleDistanceVisualizer()
        {
            if (first == null)
            {
                first = hoveredNode;
                prevMat = 2;
            }
            else if (last == null)
            {
                last = hoveredNode;
                prevMat = 3;
                Debug.Log("Distance: " + grid.GetDistance(first, last) + "m");
            }
            else
            {
                visualizer.SetMaterial(first, 0);
                visualizer.SetMaterial(last, 0);
                prevMat = 0;

                first = null;
                last = null;
            }
        }

        private void HandlePathVisualizer()
        {
            if (first == null)
            {
                first = hoveredNode;
                prevMat = 2;
            }
            else if (last == null)
            {
                last = hoveredNode;
                prevMat = 2;
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                PathFinder.FindPath(first, last, grid);
                sw.Stop();
                //Debug.Log("Path found in " + sw.ElapsedTicks / 10000f + "ms");

                PathFinder.Instance.RequestPath(new PathRequest(first.WorldPosition, last.WorldPosition, grid, response =>
                {
                    Debug.Log("Path found in " + sw.ElapsedTicks / 10000f + "ms");
                    foreach (Node n in (response as PathResponse).nodesPath)
                        if (n != first && n != last)
                            visualizer.SetMaterial(n, 3);
                }));
            }
            else
            {
                for (int i = 0; i < grid.MaxSize; i++)
                    visualizer.SetMaterial(grid.GetNode(i), 0);
                prevMat = 0;

                first = null;
                last = null;
            }
        }

        private void HandleAllPathesVisualizer()
        {
            for (int i = 0; i < arrows.Count; i++)
                DestroyImmediate(arrows[i]);
            arrows.Clear();

            if (first != null)
                visualizer.SetMaterial(first, 0);

            first = hoveredNode;
            prevMat = 3;
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Traversal<Node> traversal = PathFinder.FindAllPathes(first, grid);
            sw.Stop();
            Debug.Log("All Paths found in " + sw.ElapsedTicks / 10000f + "ms");

            foreach (Node n in grid.Nodes)
            {
                if (n == first)
                    continue;

                if (!traversal.HasParent(n))
                    visualizer.SetMaterial(n, 2);
                else
                {
                    Transform current = Instantiate(arrowPrefab).transform;
                    current.position = n.WorldPosition + Vector3.up * .2f;
                    current.rotation = Quaternion.FromToRotation(current.forward, (traversal.GetParent(n).localPosition - n.localPosition).normalized) * current.rotation;
                    current.parent = transform;
                    arrows.Add(current.gameObject);
                }
            }
        }
    }
}