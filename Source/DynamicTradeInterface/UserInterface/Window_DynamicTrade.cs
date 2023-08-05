using DynamicTradeInterface.Attributes;
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
		CaravanWidget _caravanWidget;
		bool _refresh;



		float _headerHeight;
		string _colonyHeader;
		string _colonyHeaderDescription;
		string _traderHeader;
		string _traderHeaderDescription;

		public Window_DynamicTrade()
		{
			_colonyTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text))
			{
				DrawScrollbarAlways = true,
			};
			_colonyTable.LineFont = GameFont.Small;
			_traderTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text))
			{
				DrawScrollbarAlways = true,
			};
			_traderTable.LineFont = GameFont.Small;
			_settings = Mod.DynamicTradeInterfaceMod.Settings;
			resizeable = true;
			draggable = true;
			_refresh = false;

			_colonyTable.OnSorting += Table_OnSorting;
			_traderTable.OnSorting += Table_OnSorting;
		}

		private void Table_OnSorting(IEnumerable<TableRow<Tradeable>> originalCollection, ref IOrderedEnumerable<TableRow<Tradeable>>? ordering)
		{
			ordering = originalCollection.OrderByDescending(x => x.RowObject.CountToTransfer != 0);
		}

		public void Initialize(Tradeable currency, List<Tradeable> tradeables)
		{
			_currency = currency;
			_tradeables = tradeables;
			PopulateTable(_colonyTable, Transactor.Colony);
			PopulateTable(_traderTable, Transactor.Trader);


			_colonyHeader = Faction.OfPlayer.Name;
			string negotiatorName = TradeSession.playerNegotiator.Name.ToStringFull;
			string negotiatorValue = TradeSession.playerNegotiator.GetStatValue(StatDefOf.TradePriceImprovement).ToStringPercent();
			_colonyHeaderDescription = "NegotiatorTradeDialogInfo".Translate(negotiatorName, negotiatorValue);

			_headerHeight = Text.LineHeightOf(GameFont.Medium) + Text.LineHeightOf(GameFont.Small);

			_traderHeader = TradeSession.trader.TraderName;
			_traderHeaderDescription = TradeSession.trader.TraderKind.LabelCap;

			_caravanWidget = new CaravanWidget(tradeables, currency);
		}

		public override Vector2 InitialSize => new Vector2(UI.screenWidth * 0.75f, UI.screenHeight * 0.8f);

		private void PopulateTable(Table<TableRow<Tradeable>> table, Transactor transactor)
		{
			table.Clear();
			foreach (Defs.TradeColumnDef columnDef in _settings.GetVisibleTradeColumns())
			{
				var column = table.AddColumn(columnDef.LabelCap, columnDef.defaultWidth, (ref Rect rect, TableRow<Tradeable> row) => columnDef._callback(ref rect, row.RowObject, transactor, ref _refresh));
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

			Rect top, bottom;
			// Colony
			left.SplitHorizontallyWithMargin(out top, out bottom, out _, GenUI.GapTiny, _headerHeight);

			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(top, _colonyHeader);

			Text.Anchor = TextAnchor.LowerCenter;
			Text.Font = GameFont.Small;
			Widgets.Label(top, _colonyHeaderDescription);


			_colonyTable.Draw(bottom.ContractedBy(GenUI.GapTiny));


			// Trader
			right.SplitHorizontallyWithMargin(out top, out bottom, out _, GenUI.GapTiny, _headerHeight);

			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(top, _traderHeader);

			Text.Anchor = TextAnchor.LowerCenter;
			Text.Font = GameFont.Small;
			Widgets.Label(top, _traderHeaderDescription);

			_traderTable.Draw(bottom.ContractedBy(GenUI.GapTiny));







			if (_refresh)
			{
				_refresh = false;
				_colonyTable.Refresh();
				_traderTable.Refresh();
				_caravanWidget.SetDirty();
				TradeSession.deal.UpdateCurrencyCount();
			}
		}
	}
}
