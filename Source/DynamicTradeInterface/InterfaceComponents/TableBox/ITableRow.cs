namespace DynamicTradeInterface.InterfaceComponents.TableBox
{
	internal interface ITableRow
	{
		bool HasColumn(TableColumn column);

		string this[TableColumn key]
		{
			get;
			set;
		}
	}
}
