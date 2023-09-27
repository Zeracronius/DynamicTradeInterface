using System;
using DynamicTradeInterface.Collections;
using UnityEngine;

namespace DynamicTradeInterface.InterfaceComponents.TableBox
{
	internal delegate void RowCallback<Rect, T>(ref Rect boundingBox, T item);

	abstract class TableColumn
	{
		/// <summary>
		/// Gets the title of the column.
		/// </summary>
		public string Caption { get; }

		/// <summary>
		/// Gets the tooltip shown on column header.
		/// </summary>
		public string Tooltip { get; }

		/// <summary>
		/// Gets the width of the column. Use decimal value between 0 and 1 as percentage for dynamic width. <see cref="TableColumn.IsFixedWidth"/>
		/// </summary>
		public float Width { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the header of this column should be shown.
		/// </summary>
		public bool ShowHeader { get; set; }

		/// <summary>
		/// Gets or sets the initial sort direction on first click.
		/// </summary>
		public SortDirection InitialSortDirection { get; set; }

		/// <summary>
		/// Gets or sets whether this column has a fixed width.
		/// Set width as a value between 0 and 1 as a percentage of how much of the remaining width after all fixed columns are added this column should use.
		/// </summary>
		/// <value>
		///   <c>true</c> if this column is fixed width; otherwise, <c>false</c>.
		/// </value>
		public bool IsFixedWidth { get; set; }


		public object? Tag { get; set; }

		public TableColumn(string caption, float width, string? tooltip)
		{
			Width = width;
			Caption = caption;
			IsFixedWidth = true;
			ShowHeader = true;
			Tooltip = tooltip ?? caption;
		}
	}



	internal class TableColumn<T> : TableColumn where T : ITableRow
	{
		public RowCallback<Rect, T>? Callback;
		public Table<T>.OrderByCallbackDelegate? OrderByCallback;

		/// <summary>
		/// Initializes a new instance of the <see cref="TableColumn{T}"/> class.
		/// </summary>
		/// <param name="caption">The column's title.</param>
		/// <param name="width">The width of the column. Use 0.xf for percentage/fractional widths.</param>
		/// <param name="orderByCallback">Callback for ordering by this column. Arguments are the collection to apply ordering to and if current ordering is ascending. Null if not sortable.</param>
		public TableColumn(string caption, float width, Table<T>.OrderByCallbackDelegate? orderByCallback = null, string? tooltip = null)
		   : base(caption, width, tooltip)
		{
			Callback = null;
			OrderByCallback = orderByCallback;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TableColumn{T}"/> class.
		/// </summary>
		/// <param name="caption">The column's title.</param>
		/// <param name="width">The width of the column. Use 0.xf for percentage/fractional widths.</param>
		/// <param name="callback">The rendering callback when a cell of this column should be rendered.</param>
		/// <param name="orderByCallback">Callback for ordering by this column. Arguments are the collection to apply ordering to and if current ordering is ascending. Null if not sortable.</param>
		public TableColumn(string caption, float width, RowCallback<Rect, T> callback, Table<T>.OrderByCallbackDelegate? orderByCallback, string? tooltip = null)
		   : base(caption, width, tooltip)
		{
			Callback = callback;
			OrderByCallback = orderByCallback;
		}
	}
}
