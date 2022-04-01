using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    public static class TheoryEditorUtility
    {
        private static GridMaster CreateGrid<T>(string name) where T : GridMaster
        {
            GridMaster master = new GameObject(name).AddComponent<T>();
            Selection.objects = new GameObject[] { master.gameObject };
            GridMasterEditor.visualize = true;
            master.createNodeObject = false;
            master.Init();
            return master;
        }

        [MenuItem("GameObject/3D Object/Polymorphic Grid/Square Grid")]
        public static void CreateSquareGrid() => CreateGrid<SquareGrid>("New Square Grid");

        [MenuItem("GameObject/3D Object/Polymorphic Grid/Hexagon Grid")]
        public static void CreateHexGrid() => CreateGrid<HexGrid>("New Hexagon Grid");

        [MenuItem("GameObject/3D Object/Polymorphic Grid/Triangle Grid")]
        public static void CreateTriangleGrid() => CreateGrid<TriangleGrid>("New Triangle Grid");

        /// <summary>
        /// Link any object with its' visual elements tree and add undo/redo functionality.
        /// </summary>
        /// <typeparam name="T">Fields type to control the variables</typeparam>
        /// <param name="tree">Target tree</param>
        /// <param name="obj">Object to extract variables from</param>
        /// <param name="className"></param>
        public static void LinkTreeWithFields<T>(VisualElement tree, SerializedObject obj, string className = null) where T : VisualElement, IBindable
        {
            foreach (T field in tree.Query<T>(null, className).ToList())
            {
                SerializedProperty property = obj.FindProperty(field.name);

                if (property != null)
                    field.BindProperty(obj.FindProperty(field.name));
            }
        }

        public static void DefineObjectFields<T>(VisualElement tree, T obj, string className = null) where T : Object
        {
            FieldInfo[] variables = obj.GetType().GetFields();

            foreach (ObjectField field in tree.Query<ObjectField>(null, className).ToList())
            {
                for (int i = 0; i < variables.Length; i++)
                {
                    if (variables[i].Name == field.name)
                    {
                        field.objectType = variables[i].FieldType;
                        break;
                    }
                }
            }
        }

        public static void LinkTreeWithFields(VisualElement tree, SerializedObject obj, string className = null)
        {
            LinkTreeWithFields<IntegerField>(tree, obj, className);
            LinkTreeWithFields<FloatField>(tree, obj, className);
            LinkTreeWithFields<Slider>(tree, obj, className);
            LinkTreeWithFields<ObjectField>(tree, obj, className);
            LinkTreeWithFields<TextField>(tree, obj, className);
            LinkTreeWithFields<Toggle>(tree, obj, className);
        }
    }
}
