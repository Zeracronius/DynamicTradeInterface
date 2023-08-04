using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.Defs;
using DynamicTradeInterface.InterfaceComponents.TableBox;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface
{
	[HotSwappable]
	internal class Window_DynamicTrade : Window
    {
        Table<TableRow<Tradeable>> _colonyTable;
		Table<TableRow<Tradeable>> _traderTable;
		Mod.DynamicTradeInterfaceSettings _settings;
		Tradeable? _currency;
		List<Tradeable>? _tradeables;

        public Window_DynamicTrade()
		{
			_colonyTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text));
			_traderTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text));
			_settings = Mod.DynamicTradeInterfaceMod.Settings;
			resizeable = true;
			draggable = true;

		}

		public void Initialize(Tradeable currency, List<Tradeable> tradeables)
		{
			_currency = currency;
			_tradeables = tradeables;
			PopulateTable(_colonyTable, Transactor.Colony);
			PopulateTable(_traderTable, Transactor.Trader);
		}

		public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

		private void PopulateTable(Table<TableRow<Tradeable>> table, Transactor transactor)
		{
			table.Clear();
			foreach (Defs.TradeColumnDef columnDef in _settings.GetVisibleTradeColumns())
			{
				var column = table.AddColumn(columnDef.LabelCap, columnDef.defaultWidth, (ref Rect rect, TableRow<Tradeable> row) => columnDef._callback(ref rect, row.RowObject, transactor));
				if (column.Width <= 1f)
					column.IsFixedWidth = false;
			}

			foreach (Tradeable item in _tradeables.Where(x => x.CountHeldBy(transactor) > 0))
			{
				table.AddRow(new TableRow<Tradeable>(item, item.Label + " " + item.ThingDef?.label));
			}
			table.Refresh();
		}

        public override void DoWindowContents(Rect inRect)
        {
			Rect left, right;
			inRect.SplitVertically(inRect.width / 2, out left, out right);
			try
			{
				Widgets.BeginGroup(left);
				_colonyTable.Draw(left);
			}
			finally
			{
				Widgets.EndGroup();
			}

			try
			{
				Widgets.BeginGroup(right);
				_traderTable.Draw(right);
			}
			finally
			{
				Widgets.EndGroup();
			}
		}
	}
}
