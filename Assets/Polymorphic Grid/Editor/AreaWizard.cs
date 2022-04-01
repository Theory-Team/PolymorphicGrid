using UnityEngine;
using UnityEditor;

namespace TheoryTeam.PolymorphicGrid
{
    public class AreaWizard : ScriptableWizard
    {
        /// <summary>
        /// Grid to set its' nodes.
        /// </summary>
        [Tooltip("Grid to set its' nodes.")]
        public GridMaster grid;
        /// <summary>
        /// Value to multiply with node radius and use the result as collider radius for each node.
        /// </summary>
        [Range(.1f, 2f)]
        [Tooltip("Value to multiply with node radius and use the result as collider radius for each node.")]
        public float radiusMultiplier = 1f;
        /// <summary>
        /// Layer mask of unwalkable areas.
        /// </summary>
        [Tooltip("Layer mask of unwalkable areas.")]
        public LayerMask unwalkableMask;

        [MenuItem("Theory Team/Polymorphic Grid/Area Wizard")]
        public static void ShowWizard() => DisplayWizard<AreaWizard>("Area Wizard", "Assign Areas");

        private void OnWizardCreate()
        {
            Undo.RegisterCompleteObjectUndo(grid, "Modify walkable/unwalkable areas");
            for (int i = 0; i < grid.MaxSize; i++)
            {
                Node current = grid.GetNode(i);
                current.walkable = !Physics.CheckSphere(current.WorldPosition, grid.GeneratedNodeRadius * radiusMultiplier, unwalkableMask);
            }
        }
    }
}