// // -------------------------------
// // ResponsiveGrid.cs (Runtime Only - Optimized, Fixed Resize Detection)
// // -------------------------------
// using UnityEngine;
// using UnityEngine.UI;

// /// <summary>
// /// ResponsiveGrid automatically adjusts the column count and cell size of a GridLayoutGroup
// /// based on the width of the container panel.
// ///
// /// Usage:
// /// 1. Attach this script to the same GameObject that has the GridLayoutGroup (the panel/container).
// /// 2. Configure breakpoints, column counts, spacing, padding, and cell height in the inspector.
// /// 3. Add your grid items as children of the panel; their sizes will adjust automatically.
// ///
// /// The grid adapts dynamically to width changes for slim and normal-sized layouts.
// /// Runtime-only - enter Play Mode to test responsive behavior.
// /// </summary>
// [RequireComponent(typeof(GridLayoutGroup))]
// public class ResponsiveGrid : MonoBehaviour
// {
//     [Header("Column Breakpoints")]
//     [Tooltip("Width thresholds in pixels.")]
//     public float breakpointSlim = 900f;
//     public float breakpointNormal = 1000f;

//     [Header("Columns per Breakpoint")]
//     public int columnsSlim = 4;
//     public int columnsNormal = 5;

//     [Header("Grid Settings")]
//     public float spacing = 10f;
//     public RectOffset padding;
//     public float minCellWidth = 50f;
//     public float cellHeight = -1f; // -1 = same as width (square), else independent height

//     [Header("Max Cell Size (Optional)")]
//     [Tooltip("Maximum cell width per breakpoint. Set to -1 to disable (unlimited).")]
//     public float maxCellWidthSlim = -1f;
//     public float maxCellWidthNormal = -1f;

//     private GridLayoutGroup gridLayout;
//     private RectTransform rectTransform;
//     private float lastWidth = -1f;
//     private int lastColumns = -1;

//     private void Awake()
//     {
//         gridLayout = GetComponent<GridLayoutGroup>();
//         rectTransform = GetComponent<RectTransform>();
//     }

//     private void OnEnable()
//     {
//         // Force initial calculation
//         lastWidth = -1f;
//         ApplyGridSettings();
//     }

//     private void LateUpdate()
//     {
//         // Check every frame in LateUpdate (after all layout updates)
//         float currentWidth = rectTransform.rect.width;
        
//         // Only recalculate if width actually changed (0.5px threshold to avoid float noise)
//         if (Mathf.Abs(currentWidth - lastWidth) > 0.5f)
//         {
//             lastWidth = currentWidth;
//             ApplyGridSettings();
//         }
//     }

//     private void ApplyGridSettings()
//     {
//         if (gridLayout == null || rectTransform == null)
//             return;

//         float width = rectTransform.rect.width;

//         // Determine column count based on breakpoints
//         int columns = (width >= breakpointNormal) ? columnsNormal : columnsSlim;

//         Debug.Log($"[{Time.frameCount}] ApplyGrid called: width={width:F2}, cols={columns}");

//         // Early exit if nothing changed
//         if (columns == lastColumns && Mathf.Approximately(lastWidth, width))
//             return;

//         lastColumns = columns;

//         // Calculate cell width
//         float totalSpacing = spacing * (columns - 1) + padding.left + padding.right;
//         float cellWidth = (width - totalSpacing) / columns;
        
//         // Apply min constraint
//         cellWidth = Mathf.Max(cellWidth, minCellWidth);
        
//         // Apply max constraint based on breakpoint (if set)
//         float maxCellWidth = (columns == columnsNormal) ? maxCellWidthNormal : maxCellWidthSlim;
//         if (maxCellWidth > 0)
//         {
//             cellWidth = Mathf.Min(cellWidth, maxCellWidth);
//         }

//         // Apply settings to GridLayoutGroup
//         gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//         gridLayout.constraintCount = columns;
//         gridLayout.spacing = new Vector2(spacing, spacing);
//         gridLayout.padding = padding;

//         // Determine cell height (square if -1, else custom)
//         float finalHeight = (cellHeight <= 0) ? cellWidth : cellHeight;
//         gridLayout.cellSize = new Vector2(cellWidth, finalHeight);
//     }
// }