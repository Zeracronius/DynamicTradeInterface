using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.Defs;
using DynamicTradeInterface.InterfaceComponents.TableBox;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
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
		CaravanWidget _caravanWidget;

		public Window_DynamicTrade()
		{
			_colonyTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text));
			_colonyTable.LineFont = GameFont.Small;
			_traderTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text));
			_traderTable.LineFont = GameFont.Small;
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


			_caravanWidget = new CaravanWidget(tradeables, currency);
		}

		public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.75f, UI.screenHeight * 0.8f);

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
			if (_caravanWidget.InCaravan)
			{
				_caravanWidget.Draw(new Rect(12f, 0f, inRect.width - 24f, 40f));
				inRect.yMin += 52f;
			}

			Rect left, right;
			inRect.SplitVerticallyWithMargin(out left, out right, out _, GenUI.GapTiny, inRect.width / 2);
			_colonyTable.Draw(left.ContractedBy(GenUI.GapTiny));
			_traderTable.Draw(right.ContractedBy(GenUI.GapTiny));
		}
	}
}
