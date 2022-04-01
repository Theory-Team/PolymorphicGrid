using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace TheoryTeam.PolymorphicGrid
{
    public class MainSettingsEditor : SettingsProvider
    {
        public MainSettingsEditor(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }

        /// <summary>
        /// Load settings file from default path and return it.
        /// </summary>
        /// <returns></returns>
        public static MainSettings GetSettings()
        {
            if (File.Exists(PolymorphicGridEditorUtility.mainSettingsFilePath))
                return AssetDatabase.LoadAssetAtPath<MainSettings>(PolymorphicGridEditorUtility.mainSettingsFilePath);

            MainSettings settings = ScriptableObject.CreateInstance<MainSettings>();
            AssetDatabase.CreateAsset(settings, PolymorphicGridEditorUtility.mainSettingsFilePath);
            return settings;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PolymorphicGridEditorUtility.mainSettingsEditorUXMLPath);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(PolymorphicGridEditorUtility.mainSettingsEditorUSSPath);
            rootElement.Add(visualTree.Instantiate());
            rootElement.styleSheets.Add(styleSheet);

            MainSettings settings = GetSettings();
            TheoryEditorUtility.LinkTreeWithFields(rootElement, new SerializedObject(settings));
            TheoryEditorUtility.DefineObjectFields(rootElement, settings);
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider() => new MainSettingsEditor("Preferences/Polymorphic Grid", SettingsScope.User);
    }
}