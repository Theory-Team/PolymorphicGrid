using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

namespace TheoryTeam.PolymorphicGrid
{
    [CustomEditor(typeof(TriangleGrid))]
    public class TriangleGridEditor : GenericGridEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PolymorphicGridEditorUtility.triangleGridEditorUXMLPath);
            root.Add(tree.Instantiate());

            TheoryEditorUtility.LinkTreeWithFields<Toggle>(root, new SerializedObject(target as TriangleGrid));
            root.Add(base.CreateInspectorGUI());
            return root;
        }
    }
}