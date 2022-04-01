using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TheoryTeam.PolymorphicGrid
{
    public class GenericGridEditor : GridMasterEditor
    {
        private GenericGrid grid;

        protected override void OnEnable()
        {
            base.OnEnable();
            grid = master as GenericGrid;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PolymorphicGridEditorUtility.genericGridEditorUXMLPath).Instantiate());
            TheoryEditorUtility.LinkTreeWithFields<IntegerField>(root, new SerializedObject(grid));
            root.Add(base.CreateInspectorGUI());
            return root;
        }
    }
}