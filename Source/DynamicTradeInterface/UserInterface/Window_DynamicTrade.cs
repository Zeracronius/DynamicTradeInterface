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
        Table<TableRow<Tradeable>> _table;
		Mod.DynamicTradeInterfaceSettings _settings;
		Tradeable? _currency;
		List<Tradeable>? _tradeables;

        public Window_DynamicTrade()
		{
			_table = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text));
			_settings = Mod.DynamicTradeInterfaceMod.Settings;
			resizeable = true;
			draggable = true;

		}

		public void Initialize(Tradeable currency, List<Tradeable> tradeables)
		{
			_currency = currency;
			_tradeables = tradeables;
			PopulateTable();
		}

		public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

		private void PopulateTable()
		{
			_table.Clear();
			foreach (Defs.TradeColumnDef columnDef in _settings.GetVisibleTradeColumns())
			{
				var column = _table.AddColumn(columnDef.LabelCap, columnDef.defaultWidth, (ref Rect rect, TableRow<Tradeable> row) => columnDef._callback(ref rect, row.RowObject, TradeAction.PlayerSells));
				if (column.Width <= 1f)
					column.IsFixedWidth = false;
			}

			if (_tradeables != null)
			{
				foreach (Tradeable item in _tradeables)
				{
					_table.AddRow(new TableRow<Tradeable>(item, item.Label + " " + item.ThingDef?.label));
				}
			}
			_table.Refresh();
		}

        public override void DoWindowContents(Rect inRect)
        {
			_table.Draw(inRect);
        }
	}
}
