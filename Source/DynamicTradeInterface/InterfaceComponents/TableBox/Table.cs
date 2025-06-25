using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.Collections;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents.TableBox
{
	public class Table<T> where T : ITableRow
	{
		public delegate void OrderByCallbackDelegate(ListFilter<T> rows, SortDirection sortDirection, TableColumn tableColumn, bool reset);

		public event Action<TableColumn>? ColumnResized;
		public event Action<Table<T>, TableColumn>? ColumnVisibilityChanged;

		internal readonly string SEARCH_PLACEHOLDER = "DynamicTableControlSearchPlaceholder".Translate();
		internal readonly float SEARCH_PLACEHOLDER_SIZE;

		internal const float CELL_SPACING = 5;
		internal const float ROW_SPACING = 3;

		private List<TableColumn<T>> _columns;
		private ListFilter<T> _rows;
		private string _searchText;
		private Vector2 _scrollPosition;
		private List<T> _selectedRows;
		private float _fixedColumnWidth;
		private float _dynamicColumnWidth;
		private GameFont _lineFont;
		private bool _multiSelect;
		private string? _caption;
		private bool _drawHeaders;
		private bool _drawSearchBox;
		private bool _drawScrollbar;
		private bool _drawBorder;
		private bool _allowSorting;
		private bool _canSelectRows;
		private Dictionary<TableColumn<T>, SortDirection> _columnSortCache;

		private bool _alternatingRowColors;

		public bool AlternatingRowColors
		{
			get => _alternatingRowColors;
			set => _alternatingRowColors = value;
		}


		private TableColumn<T>? _resizingColumn;

		public IList<T> RowItems => _rows.Items;

		public IEnumerable<(object, bool)> GetSortQueue() => _rows.GetSortings();

		/// <summary>
		/// Gets or sets the row filter function. Return true to include row and fall to skip.
		/// </summary>
		public Func<T, bool>? RowFilter { get; set; }

		/// <summary>
		/// Returns the current selected rows.
		/// </summary>
		public IReadOnlyList<T> SelectedRows => _selectedRows;

		/// <summary>
		/// Returns a list of columns.
		/// </summary>
		public IReadOnlyList<TableColumn<T>> Columns => _columns;

		/// <summary>
		/// Triggered when selected rows is changed.
		/// </summary>
		public event EventHandler<IReadOnlyList<T>>? SelectionChanged;

		/// <summary>
		/// Gets or sets the text filter on the underlying rows collection.
		/// </summary>
		public string? Filter
		{
			get => _rows.Filter;
			set => _rows.Filter = value;
		}

		/// <summary>
		/// Gets or sets the line font.
		/// </summary>
		/// <value>
		/// The line font.
		/// </value>
		public GameFont LineFont
		{
			get => _lineFont;
			set => _lineFont = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Table{T}"/> allows selecting multiple rows.
		/// </summary>
		/// <value>
		///   <c>true</c> if multi-select is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool MultiSelect
		{
			get => _multiSelect;
			set => _multiSelect = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether column headers should be drawn.
		/// </summary>
		/// <value>
		///   <c>true</c> if headers are visible; otherwise, <c>false</c>.
		/// </value>
		public bool DrawHeaders
		{
			get => _drawHeaders;
			set => _drawHeaders = value;
		}



		/// <summary>
		/// Gets or sets a value indicating whether the search box should be drawn.
		/// </summary>
		/// <value>
		///   <c>true</c> if search box is visible; otherwise, <c>false</c>.
		/// </value>
		public bool DrawSearchBox
		{
			get => _drawSearchBox;
			set => _drawSearchBox = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the scroll bar should always be visible.
		/// </summary>
		/// <value>
		///   <c>true</c> if the scroll bar is always visible; otherwise, <c>false</c>.
		/// </value>
		public bool DrawScrollbarAlways
		{
			get => _drawScrollbar;
			set => _drawScrollbar = value;
		}

		/// <summary>
		/// Gets or Sets a value for whether a border should be drawn around the table and filter box.
		/// </summary>
		public bool DrawBorder
		{
			get => _drawBorder;
			set => _drawBorder = value;
		}


		/// <summary>
		/// Gets or Sets a value indicating if clicking the column headers should sort the table.
		/// </summary>
		public bool AllowSorting
		{
			get => _allowSorting;
			set => _allowSorting = value;
		}


		/// <summary>
		/// Gets or Sets the caption drawn above the table.
		/// </summary>
		public string? Caption
		{
			get => _caption;
			set => _caption = value;
		}

		/// <summary>
		/// Gets or Sets the caption drawn above the table.
		/// </summary>
		public bool CanSelectRows
		{
			get => _canSelectRows;
			set => _canSelectRows = value;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Table{T}" /> class.
		/// </summary>
		/// <param name="filterCallback">Callback used to apply filter text.</param>
		public Table(ListFilter<T>.FilterCallbackDelegate filterCallback)
		{
			Text.Font = GameFont.Small;
			SEARCH_PLACEHOLDER_SIZE = Mathf.Ceil(Text.CalcSize(SEARCH_PLACEHOLDER).x) + GenUI.GapTiny;

			_rows = new ListFilter<T>(filterCallback);
			_columnSortCache = new Dictionary<TableColumn<T>, SortDirection>();

			_searchText = string.Empty;
			_selectedRows = new List<T>(20);
			_columns = new List<TableColumn<T>>();
			_drawHeaders = true;
			_drawSearchBox = true;
			_drawScrollbar = false;
			_drawBorder = false;
			_allowSorting = true;
			_canSelectRows = true;
		}


		/// <summary>Adds the column.</summary>
		/// <param name="header">Title of the column.</param>
		/// <param name="width">Column width.</param>
		/// <param name="orderByCallback">Optional callback to tell the column how to order rows.</param>
		public TableColumn<T> AddColumn(string header, float width, OrderByCallbackDelegate? orderByCallback = null, string? tooltip = null)
		{
			TableColumn<T> column = new TableColumn<T>(header, width, orderByCallback, tooltip);
			_columns.Add(column);
			return column;
		}

		/// <summary>
		/// Adds the column.
		/// </summary>
		/// <param name="header">Column caption.</param>
		/// <param name="width">Column width.</param>
		/// <param name="callback">Render callback when cell in column is drawn.</param>
		/// <param name="orderByCallback">Optional callback to tell the column how to order rows.</param>
		/// <returns></returns>
		public TableColumn<T> AddColumn(string header, float width, RowCallback<Rect, T> callback, OrderByCallbackDelegate? orderByCallback = null, string? tooltip = null)
		{
			TableColumn<T> column = new TableColumn<T>(header, width, callback, orderByCallback, tooltip);
			_columns.Add(column);
			return column;
		}

		/// <summary>
		/// Add a new row to the table.
		/// </summary>
		/// <param name="item">Row to add.</param>
		public void AddRow(T item)
		{
			if (RowFilter != null && RowFilter(item) == false)
				return;

			// Ensure new rows have every column in data cache to avoid needing to check for every single cell during rendering.
			foreach (TableColumn column in _columns)
			{
				if (item.HasColumn(column) == false)
					item[column] = string.Empty;
			}

			_rows.Items.Add(item);
		}

		/// <summary>
		/// Remove specific row from table.
		/// </summary>
		/// <param name="item">Row to remove.</param>
		public void DeleteRow(T item)
		{
			_rows.Items.Remove(item);
		}

		/// <summary>
		/// Invalidate rows and recalculates columns.
		/// </summary>
		public void Refresh()
		{
			_rows.Invalidate();
			InvalidateColumnWidths();
		}

		private void InvalidateColumnWidths()
		{
			_fixedColumnWidth = 0;
			_dynamicColumnWidth = 0;
			for (int i = 0; i < _columns.Count; i++)
			{
				TableColumn column = _columns[i];
				if (column.Visible == false) 
					continue;

				if (column.IsFixedWidth)
					_fixedColumnWidth += column.Width;
				else
					_dynamicColumnWidth += column.Width;
			}
		}

		/// <summary>
		/// Clears all rows and columns from table.
		/// </summary>
		public void Clear()
		{
			_rows.Items.Clear();
			_selectedRows.Clear();
			_columns.Clear();
		}

		public void SetColumnWidth(object tag, float width)
		{
			TableColumn tableColumn = Columns.SingleOrDefault(x => x.Tag == tag);
			if (tableColumn != null)
				tableColumn.Width = width;

			InvalidateColumnWidths();
		}

		/// <summary>
		/// Clears currently selected rows.
		/// </summary>
		public void ClearSelection()
		{
			_selectedRows.Clear();
		}

		/// <summary>
		/// Marks provided rows as selected.
		/// </summary>
		/// <param name="rows">The rows to be selected.</param>
		public void SelectRows(IEnumerable<T> rows)
		{
			_selectedRows.Clear();
			foreach (T row in rows)
				_selectedRows.Add(row);

			if (SelectionChanged != null)
				SelectionChanged.Invoke(this, _selectedRows);
		}

		public void Sort(TableColumn<T> column, SortDirection? direction = null, bool? reset = null)
		{
			if (column.CanSort == false)
				return;

			SortDirection targetDirection;

			if (reset == null)
				reset = Event.current.modifiers != EventModifiers.Shift;

			if (direction == null)
			{
				SortDirection currentDirection = _rows.GetSortingDirection(column);

				switch (currentDirection)
				{
					case SortDirection.Ascending:
						targetDirection = SortDirection.Descending;
						break;

					case SortDirection.Descending:
						targetDirection = SortDirection.Ascending;
						break;

					case SortDirection.None:
					default:
						targetDirection = column.InitialSortDirection;
						break;
				}

				// If column is already sorted and new sort would set it back to initial, then clear sort instead.
				if (currentDirection != SortDirection.None && targetDirection == column.InitialSortDirection)
					targetDirection = SortDirection.None;
			}
			else
				targetDirection = direction.Value;

			// Apply sorting
			switch (targetDirection)
			{
				case SortDirection.None:
					_rows.ClearSorting(column);
					break;

				case SortDirection.Ascending:
					if (column.OrderByCallback == null)
						_rows.OrderBy(x => x[column], reset.Value, column);
					else
						column.OrderByCallback(_rows, SortDirection.Ascending, column, reset.Value);
					break;

				case SortDirection.Descending:
					if (column.OrderByCallback == null)
						_rows.OrderByDescending(x => x[column], reset.Value, column);
					else
						column.OrderByCallback(_rows, SortDirection.Descending, column, reset.Value);
					break;
			}

			_columnSortCache.Clear();
			for (int i = _columns.Count - 1; i >= 0; i--)
			{
				TableColumn<T> columnEntry = _columns[i];
				direction = _rows.GetSortingDirection(columnEntry);
				if (direction != SortDirection.None)
					_columnSortCache[columnEntry] = direction.Value;
			}
		}

		/// <summary>
		/// Draw table to bounding box.
		/// </summary>
		/// <param name="boundingBox">Decides the size and position of the table.</param>
		public void Draw(Rect boundingBox)
		{
			Text.Font = GameFont.Small;
			if (string.IsNullOrEmpty(_caption) == false)
			{
				float height = Text.LineHeight;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(boundingBox, _caption);
				Text.Anchor = TextAnchor.UpperLeft;
				boundingBox.y += height + GenUI.GapTiny;
				boundingBox.height -= height + GenUI.GapTiny;
			}

			if (_drawBorder)
			{
				Widgets.DrawBoxSolidWithOutline(boundingBox, Widgets.WindowBGFillColor, Color.gray);
				boundingBox = boundingBox.ContractedBy(2);
			}

			bool selectionHasChanged = false;

			if (_drawSearchBox)
			{
				float clearButtonSize = Text.LineHeight;
				Rect searchBox = new Rect(boundingBox.x, boundingBox.y, boundingBox.width - clearButtonSize - CELL_SPACING, clearButtonSize);
				_searchText = Widgets.TextField(searchBox, _searchText);
				if (Widgets.ButtonText(new Rect(boundingBox.xMax - clearButtonSize, boundingBox.y, clearButtonSize, clearButtonSize), "X"))
					_searchText = "";


				if (_searchText == string.Empty)
					Widgets.NoneLabelCenteredVertically(new Rect(searchBox.x + CELL_SPACING, searchBox.y, SEARCH_PLACEHOLDER_SIZE, Text.LineHeight), SEARCH_PLACEHOLDER);

				_rows.Filter = _searchText;

				boundingBox.y += clearButtonSize + 7;
				boundingBox.height -= clearButtonSize + 7;
			}


			float tableWidth = boundingBox.width - GenUI.ScrollBarWidth - CELL_SPACING;
			float leftoverWidth = tableWidth - _fixedColumnWidth - (CELL_SPACING * _columns.Count);

			if (_drawHeaders)
			{
				Rect columnHeader = new Rect(boundingBox);
				columnHeader.height = Text.LineHeight;

				int columnCount = _columns.Count;
				for (int i = 0; i < columnCount; i++)
				{
					TableColumn<T> column = _columns[i];
					if (column.Visible == false)
						continue;

					bool fixedWidth = column.IsFixedWidth;
					if (fixedWidth)
					{
						columnHeader.width = column.Width;
					}
					else
						columnHeader.width = column.Width / _dynamicColumnWidth * leftoverWidth;

					bool canOrder = _allowSorting && column.CanSort;
					if (canOrder)
						Widgets.DrawHighlightIfMouseover(columnHeader);

					if (String.IsNullOrWhiteSpace(column.Caption) == false)
					{
						if (column.ShowHeader)
							Widgets.Label(columnHeader, column.Caption);

						if (Mouse.IsOver(columnHeader))
							TooltipHandler.TipRegion(columnHeader, column.Tooltip);
					}

					if (_columnSortCache.Count > 0)
					{
						if (_columnSortCache.TryGetValue(column, out SortDirection sortDirection))
						{
							string sortIndicator = "↑";
							if (sortDirection == SortDirection.Descending)
								sortIndicator = "↓";

							Text.Anchor = TextAnchor.UpperRight;
							Widgets.Label(columnHeader.ContractedBy(GenUI.GapTiny, 0), sortIndicator);
							Text.Anchor = TextAnchor.UpperLeft;
						}
					}

					if (Widgets.ButtonInvisible(columnHeader, true))
					{
						// If right-click then show context menu.
						if (Event.current.button == 1)
							ShowColumnContextMenu(column, canOrder);
						else if (canOrder)
							Sort(column);
					}

					

					// Add column resize widget.
					if (_resizingColumn == null)
					{
						// Only allow resizing columns with fixed width.
						if (fixedWidth)
						{
							Rect dragRect = new Rect(columnHeader.xMax, columnHeader.y, CELL_SPACING, columnHeader.height);
							bool isOver = Mouse.IsOver(dragRect);
							if (isOver)
							{
								Widgets.DrawHighlight(dragRect);
								if (Event.current.type == EventType.MouseDown)
								{
									_resizingColumn = column;
									Event.current.Use();
								}
							}
						}
					}
					else if (_resizingColumn == column)
					{
						float x = Event.current.mousePosition.x;
						if (x - columnHeader.x > 5)
						{
							columnHeader.xMax = x;
							_resizingColumn.Width = columnHeader.width;
						}

						// End dragging event.
						if (Event.current.rawType == EventType.MouseUp)
						{
							InvalidateColumnWidths();
							ColumnResized?.Invoke(_resizingColumn);
							_resizingColumn = null;
						}
					}

					columnHeader.x = columnHeader.xMax + CELL_SPACING;
				}

				boundingBox.y += Text.LineHeight + ROW_SPACING;
				boundingBox.height -= Text.LineHeight + ROW_SPACING;
			}

			Text.Font = _lineFont;


			Rect rowRect = new Rect(0, 0, tableWidth, Text.LineHeight + ROW_SPACING);
			Rect listbox = new Rect(0, 0, tableWidth, (_rows.Filtered.Count + 1) * rowRect.height);

			try
			{
				Widgets.BeginScrollView(boundingBox, ref _scrollPosition, listbox, _drawScrollbar);

				rowRect.y = _scrollPosition.y;

				T currentRow;
				// Get index of first row visible in scrollbox
				int currentIndex = Mathf.FloorToInt(_scrollPosition.y / rowRect.height);
				int rowCount = _rows.Filtered.Count;
				for (; currentIndex < rowCount; currentIndex++)
				{
					currentRow = _rows.Filtered[currentIndex];
					rowRect.x = 0;
					for (int i = 0; i < _columns.Count; i++)
					{
						Text.Font = _lineFont;
						TableColumn<T> column = _columns[i];
						if (column.Visible == false)
							continue;

						if (column.IsFixedWidth)
						{
							rowRect.width = column.Width;
						}
						else
							rowRect.width = column.Width / _dynamicColumnWidth * leftoverWidth;

						RowCallback<Rect, T>? callback = column.Callback;
						if (callback != null)
							callback(ref rowRect, currentRow);
						else
							Widgets.Label(rowRect, currentRow[column]);


						rowRect.x = rowRect.xMax + CELL_SPACING;
					}

					rowRect.x = 0;
					rowRect.width = tableWidth;

					if (_alternatingRowColors && currentIndex % 2 == 1)
						Widgets.DrawLightHighlight(rowRect);

					// Hightlight entire row if selected.
					if (_canSelectRows && _selectedRows.Contains(currentRow))
						Widgets.DrawHighlightSelected(rowRect);

					// Hightlight row if moused over.
					if (Mouse.IsOver(rowRect))
					{
						Widgets.DrawHighlight(rowRect);
						string? tooltip = currentRow.Tooltip;
						if (string.IsNullOrEmpty(tooltip) == false)
							TooltipHandler.TipRegion(rowRect, tooltip);
					}
					Widgets.DrawHighlightIfMouseover(rowRect);

					if (_canSelectRows)
					{
						if (Widgets.ButtonInvisible(rowRect, false))
						{
							if (_multiSelect == false || Event.current.control == false)
								_selectedRows.Clear();

							_selectedRows.Add(currentRow);
							selectionHasChanged = true;
						}
					}

					rowRect.y += rowRect.height;

					// Break if next row starts outside bottom of scrollbox + 1 row to ensure smooth scrolling - though this should possibly not be needed for IMGUI.
					if (rowRect.y > boundingBox.height + _scrollPosition.y + rowRect.height)
						break;
				}

			}
			finally
			{
				Widgets.EndScrollView();
			}

			// Handle any potential event handlers when selection is modified.
			if (selectionHasChanged && SelectionChanged != null)
				SelectionChanged.Invoke(this, _selectedRows);
		}

		private void ShowColumnContextMenu(TableColumn<T> currentColumn, bool canOrder)
		{
			_columnSortCache.TryGetValue(currentColumn, out SortDirection currentSortDirection);
			
			List<FloatMenuOption> options = new List<FloatMenuOption>();
			if (canOrder)
			{
				options.Add(new FloatMenuOption("DynamicTradeWindowColumnContextSortByAscending".TranslateSimple(), () => Sort(currentColumn, SortDirection.Ascending))
				{
					orderInPriority = 4,
					Priority = MenuOptionPriority.DisabledOption,
					Disabled = currentSortDirection == SortDirection.Ascending,
				});

				options.Add(new FloatMenuOption("DynamicTradeWindowColumnContextSortByDescending".TranslateSimple(), () => Sort(currentColumn, SortDirection.Descending))
				{
					orderInPriority = 3,
					Priority = MenuOptionPriority.DisabledOption,
					Disabled = currentSortDirection == SortDirection.Descending,
				});
			}

			if (currentSortDirection != SortDirection.None)
			{
				options.Add(new FloatMenuOption("DynamicTradeWindowColumnContextClearSorting".TranslateSimple(), () => Sort(currentColumn, SortDirection.None))
				{
					orderInPriority = 2,
					Priority = MenuOptionPriority.DisabledOption,
				});
			}

			options.Add(new FloatMenuOption("DynamicTradeWindowColumnContextHideColumn".TranslateSimple(), () => ToggleColumn(currentColumn, false))
			{
				orderInPriority = 1,
				Priority = MenuOptionPriority.DisabledOption,
			});

			var hiddenColumns = _columns.Where(x => x.Visible == false).ToList();
			if (hiddenColumns.Count > 0)
			{
				options.Add(new FloatMenuOption("DynamicTradeWindowColumnContextShowHiddenColumns".TranslateSimple(), () => { }) 
				{ 
					Disabled = true,
					orderInPriority = 0,
				});

				int priority = -1;
				foreach (var tableColumn in hiddenColumns)
					options.Add(new FloatMenuOption(tableColumn.Caption, () => ToggleColumn(tableColumn, true))
					{
						orderInPriority = priority--,
						Priority = MenuOptionPriority.DisabledOption,
					});
			}
			Find.WindowStack.Add(new FloatMenu(options));
		}

		public void ToggleColumn(TableColumn column, bool setVisible)
		{
			column.Visible = setVisible;
			InvalidateColumnWidths();
			ColumnVisibilityChanged?.Invoke(this, column);
		}
	}
}
