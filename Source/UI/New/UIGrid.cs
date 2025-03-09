using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.UI.New
{
    /// <summary>
    /// A grid layout container for UI elements.
    /// </summary>
    public class UIGrid : UIContainer
    {
        private readonly int _rows;
        private readonly int _columns;
        private int _cellWidth;
        private int _cellHeight;
        private readonly int _horizontalSpacing;
        private readonly int _verticalSpacing;
        
        /// <summary>
        /// Gets the number of rows in the grid.
        /// </summary>
        public int Rows => _rows;
        
        /// <summary>
        /// Gets the number of columns in the grid.
        /// </summary>
        public int Columns => _columns;
        
        /// <summary>
        /// Gets the width of each cell in the grid.
        /// </summary>
        public int CellWidth => _cellWidth;
        
        /// <summary>
        /// Gets the height of each cell in the grid.
        /// </summary>
        public int CellHeight => _cellHeight;
        
        /// <summary>
        /// Gets the horizontal spacing between cells.
        /// </summary>
        public int HorizontalSpacing => _horizontalSpacing;
        
        /// <summary>
        /// Gets the vertical spacing between cells.
        /// </summary>
        public int VerticalSpacing => _verticalSpacing;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UIGrid"/> class.
        /// </summary>
        /// <param name="bounds">The bounds of the grid.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        /// <param name="horizontalSpacing">The horizontal spacing between cells.</param>
        /// <param name="verticalSpacing">The vertical spacing between cells.</param>
        public UIGrid(Rectangle bounds, int rows, int columns, int horizontalSpacing = 0, int verticalSpacing = 0)
        {
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be positive.");
                
            if (columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns), "Columns must be positive.");
                
            Bounds = bounds;
            _rows = rows;
            _columns = columns;
            _horizontalSpacing = Math.Max(0, horizontalSpacing);
            _verticalSpacing = Math.Max(0, verticalSpacing);
            
            // Calculate cell dimensions
            CalculateCellDimensions();
        }
        
        /// <summary>
        /// Initializes the grid and its children.
        /// </summary>
        public override void Initialize()
        {
            // Don't call abstract base.Initialize()
            
            // Initialize child elements
            foreach (var child in Children)
            {
                child.Initialize();
            }
        }
        
        /// <summary>
        /// Adds a UI element to a specific cell in the grid.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="row">The row index (0-based).</param>
        /// <param name="column">The column index (0-based).</param>
        /// <param name="rowSpan">The number of rows the element spans.</param>
        /// <param name="columnSpan">The number of columns the element spans.</param>
        public void AddElementAt(IUIElement element, int row, int column, int rowSpan = 1, int columnSpan = 1)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
                
            if (row < 0 || row >= _rows)
                throw new ArgumentOutOfRangeException(nameof(row), "Row index is out of range.");
                
            if (column < 0 || column >= _columns)
                throw new ArgumentOutOfRangeException(nameof(column), "Column index is out of range.");
                
            if (rowSpan <= 0)
                throw new ArgumentOutOfRangeException(nameof(rowSpan), "Row span must be positive.");
                
            if (columnSpan <= 0)
                throw new ArgumentOutOfRangeException(nameof(columnSpan), "Column span must be positive.");
                
            if (row + rowSpan > _rows)
                throw new ArgumentOutOfRangeException(nameof(rowSpan), "Element exceeds grid bounds.");
                
            if (column + columnSpan > _columns)
                throw new ArgumentOutOfRangeException(nameof(columnSpan), "Element exceeds grid bounds.");
                
            // Calculate position for the element
            int x = Bounds.X + column * (_cellWidth + _horizontalSpacing);
            int y = Bounds.Y + row * (_cellHeight + _verticalSpacing);
            
            // Calculate size for the element (accounting for spans)
            int width = columnSpan * _cellWidth + (columnSpan - 1) * _horizontalSpacing;
            int height = rowSpan * _cellHeight + (rowSpan - 1) * _verticalSpacing;
            
            // Set position and size of the element
            element.SetPosition(new Vector2(x, y));
            element.SetSize(new Vector2(width, height));
            
            // Add as child
            AddChild(element);
        }
        
        /// <summary>
        /// Gets the cell size.
        /// </summary>
        /// <returns>The size of each cell.</returns>
        public Vector2 GetCellSize()
        {
            return new Vector2(_cellWidth, _cellHeight);
        }
        
        /// <summary>
        /// Sets the size of the grid and recalculates cell dimensions.
        /// </summary>
        /// <param name="size">The new size.</param>
        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);
            CalculateCellDimensions();
            
            // Reposition and resize all children
            RepositionChildren();
        }
        
        /// <summary>
        /// Calculates the dimensions of each cell based on the grid size and spacing.
        /// </summary>
        private void CalculateCellDimensions()
        {
            // Calculate available space for cells
            int availableWidth = Bounds.Width - (_columns - 1) * _horizontalSpacing;
            int availableHeight = Bounds.Height - (_rows - 1) * _verticalSpacing;
            
            // Calculate cell dimensions
            _cellWidth = Math.Max(1, availableWidth / _columns);
            _cellHeight = Math.Max(1, availableHeight / _rows);
        }
        
        /// <summary>
        /// Repositions all children based on their grid positions.
        /// </summary>
        private void RepositionChildren()
        {
            // This is a simplified implementation that assumes all children
            // occupy a single cell. A more robust implementation would store
            // grid position information for each child.
            
            int childIndex = 0;
            
            for (int row = 0; row < _rows; row++)
            {
                for (int column = 0; column < _columns; column++)
                {
                    if (childIndex >= Children.Count)
                        return;
                        
                    var child = Children[childIndex++];
                    
                    // Calculate position for the element
                    int x = Bounds.X + column * (_cellWidth + _horizontalSpacing);
                    int y = Bounds.Y + row * (_cellHeight + _verticalSpacing);
                    
                    // Set position and size of the element
                    child.SetPosition(new Vector2(x, y));
                    child.SetSize(new Vector2(_cellWidth, _cellHeight));
                }
            }
        }
    }
}