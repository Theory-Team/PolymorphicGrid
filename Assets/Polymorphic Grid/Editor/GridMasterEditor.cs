using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor;

namespace TheoryTeam.PolymorphicGrid
{
    public class GridMasterEditor : Editor
    {
        public static bool visualize = false;

        protected GridVisualizer visualizer;
        protected MainSettings settings;
        protected GridMaster master;
        protected int selectedNode;

        private void InitializeVisualizer()
        {
            DestroyVisualizer();
            visualizer = new GameObject("Visualizer").AddComponent<GridVisualizer>();
            visualizer.transform.rotation = master.transform.rotation;
            visualizer.transform.parent = master.transform;
            visualizer.autoGenerate = false;
            visualizer.Grid = master;
            visualizer.materials.Add(settings.walkableMaterial);
            visualizer.materials.Add(settings.unwalkableMaterial);
            visualizer.materials.Add(settings.selectionMaterial);
            visualizer.visualizedNodeOffset = settings.visualizedNodeOffset;
            visualizer.visualizedNodeHeight = settings.visualizedNodeHeight;

            visualizer.UpdateMeshes();
            UpdateVisualizerMaterials();
            selectedNode = -1;
        }

        private void DestroyVisualizer()
        {
            if (visualizer != null)
                DestroyImmediate(visualizer.gameObject);
        }

        private void UpdateVisualizerMaterials()
        {
            for (int i = 0; i < master.MaxSize; i++)
                visualizer.SetMaterial(master.GetNode(i), master.GetNode(i).walkable ? 0 : 1);
        }

        protected virtual void HandleVisualizer()
        {
            if (!visualize)
            {
                DestroyVisualizer();
                return;
            }
            else if (visualizer == null)
                InitializeVisualizer();

            if (master.MaxSize == 0)
            {
                visualizer.ClearMeshes();
                return;
            }

            if (settings.updateVisualizerInRealtime)
                foreach (Node n in master.Nodes)
                    visualizer.SetMaterial(n, n.walkable ? 0 : 1);

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            ray = new Ray(master.transform.InverseTransformPoint(ray.origin), master.transform.InverseTransformDirection(ray.direction));
            Node current = master.GetNode(master.transform.TransformPoint(ray.GetPoint((-ray.origin.y) / ray.direction.y)));

            if (selectedNode != -1)
                visualizer.SetMaterial(master.GetNode(selectedNode), master.GetNode(selectedNode).walkable ? 0 : 1);

            if (current != null)
            {
                selectedNode = current.Index;
                visualizer.SetMaterial(current, 2);
                if (e.button == 0 && e.type == EventType.MouseUp && e.modifiers == EventModifiers.None)
                {
                    Undo.RegisterCompleteObjectUndo(master, "Modify walkable state");
                    current.walkable = !current.walkable;
                }
            }
        }

        protected virtual void OnEnable()
        {
            master = target as GridMaster;
            settings = MainSettingsEditor.GetSettings();
            SceneView.beforeSceneGui += OnSceneGUI;
        }

        protected virtual void OnDisable()
        {
            DestroyVisualizer();
            Tools.hidden = false;
            SceneView.beforeSceneGui -= OnSceneGUI;
        }

        protected virtual void OnSceneGUI(SceneView scene)
        {
            Tools.hidden = visualize;
            HandleVisualizer();
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PolymorphicGridEditorUtility.gridMasterEditorUXMLPath);
            StyleSheet styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(PolymorphicGridEditorUtility.gridMasterEditorUSSPath);
            root.Add(tree.Instantiate());
            root.styleSheets.Add(styles);

            TheoryEditorUtility.LinkTreeWithFields(root, new SerializedObject(master));
            TheoryEditorUtility.DefineObjectFields(root, master);

            ObjectField nodePrefabField = root.Q<ObjectField>("nodePrefab");
            nodePrefabField.SetEnabled(master.createNodeObject);

            root.Q<Toggle>("createNodeObject").RegisterValueChangedCallback(val => nodePrefabField.SetEnabled(val.newValue));

            root.Q<Button>("Generate").clicked += () =>
            {
                Undo.RegisterCompleteObjectUndo(master, "generate grid");
                master.Init();

                if (visualize)
                {
                    selectedNode = -1;
                    visualizer.UpdateMeshes();
                    UpdateVisualizerMaterials();
                }
            };

            root.Q<Button>("Clear").clicked += () =>
            {
                if (EditorUtility.DisplayDialog("Alert", "Are you sure you want to clear all generated grid data?\nThis action cannot be undone!", "Yes", "No"))
                {
                    master.Clear();
                    visualizer.ClearMeshes();
                    selectedNode = -1;
                }
            };

            root.Q<Button>("Load").clicked += () => LoadGridWizard.ShowWizard(master);
            root.Q<Button>("Save").clicked += () =>
            {
                string path = EditorUtility.SaveFilePanel("Save Grid Data", "Assets/", "New Grid", "asset");
                AssetDatabase.CreateAsset(master.Serialize(), path.Substring(path.IndexOf("Assets")));
                AssetDatabase.Refresh();
            };

            Toggle t = new Toggle("Visualize") { value = visualize };
            t.RegisterValueChangedCallback(value =>
            {
                if (!value.newValue && value.previousValue)
                    DestroyVisualizer();
                visualize = value.newValue;
            });
            root.Add(t);
            return root;
        }
    }
}