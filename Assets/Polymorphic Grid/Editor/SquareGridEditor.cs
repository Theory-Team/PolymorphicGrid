using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

namespace TheoryTeam.PolymorphicGrid
{
    [CustomEditor(typeof(SquareGrid))]
    public class SquareGridEditor : GridMasterEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PolymorphicGridEditorUtility.squareGridEditorUXMLPath);
            root.Add(tree.Instantiate());

            TheoryEditorUtility.LinkTreeWithFields<IntegerField>(root, new SerializedObject(target as SquareGrid));
            TheoryEditorUtility.LinkTreeWithFields<Toggle>(root, new SerializedObject(target as SquareGrid));
            root.Add(base.CreateInspectorGUI());
            return root;
        }
    }
}