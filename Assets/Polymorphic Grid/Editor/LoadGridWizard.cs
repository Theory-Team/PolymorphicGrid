using UnityEditor;
using System;

namespace TheoryTeam.PolymorphicGrid
{
    public class LoadGridWizard : ScriptableWizard
    {
        public SerializableGrid grid;

        private GridMaster master;

        public static void ShowWizard(GridMaster master) => DisplayWizard<LoadGridWizard>("Load Grid Wizard", "Load").master = master;

        private void OnWizardCreate()
        {
            if (grid == null)
                throw new NullReferenceException("Node grid found to load");
            master.Deserialize(grid);
        }
    }
}