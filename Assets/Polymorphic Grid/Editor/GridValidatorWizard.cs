using UnityEditor;
using UnityEngine;

namespace TheoryTeam.PolymorphicGrid
{
    public class GridValidatorWizard : ScriptableWizard
    {
        public GridMaster grid;

        [MenuItem("Theory Team/Polymorphic Grid/Grid Validator")]
        public static void ShowWizard() => DisplayWizard<GridValidatorWizard>("Grid Validator", "Validate");

        public static bool IsValid(GridMaster master)
        {
            Node a, b;
            for (int i = 0; i < master.MaxSize; i++)
            {
                for (int j = 0; j < master.MaxSize; j++)
                {
                    a = master.GetNode(i);
                    b = master.GetNode(j);
                    if (i == j || a.neighbors.Contains(j))
                        continue;

                    foreach (Vector3 point in master.GetNodeCorners(a))
                        if (b.ContainsPoint(point))
                            return false;
                }
            }

            return true;
        }

        private void OnWizardCreate()
        {
            EditorUtility.DisplayDialog("Validation Result", IsValid(grid) ? "Valid Grid!" : "Sorry, Not Valid Grid!", "OK");
        }
    }
}