namespace DynamicTradeInterface.InterfaceComponents.TableBox
{
	public interface ITableRow
	{
		/// <summary>
		/// Shown when mouse is over the row.
		/// </summary>
		string? Tooltip { get; }

		bool HasColumn(TableColumn column);

		string this[TableColumn key]
		{
			get;
			set;
		}
	}
}
