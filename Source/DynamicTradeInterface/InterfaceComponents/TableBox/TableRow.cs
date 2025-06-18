using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents.TableBox
{
	public class TableRow<T> : ITableRow
	{
		private Dictionary<TableColumn, string> _rowData;
		public T RowObject { get; private set; }

#pragma warning disable CS0649 // Field 'TableRow<T>.Tag' is never assigned to, and will always have its default value null
		public object? Tag;
#pragma warning restore CS0649 // Field 'TableRow<T>.Tag' is never assigned to, and will always have its default value null

		public string SearchString;

		/// <inheritdoc />
		public string? Tooltip { get; set; }

		public string this[TableColumn key]
		{
			get
			{
				return _rowData[key];
			}
			set
			{
				_rowData[key] = value;
			}
		}

		public TableRow(T rowObject, string? searchString = null)
		{
			_rowData = new Dictionary<TableColumn, string>();
			SearchString = searchString?.ToLower() ?? string.Empty;
			RowObject = rowObject;
		}

		public bool HasColumn(TableColumn column)
		{
			return _rowData.ContainsKey(column);
		}
	}
}
