using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.Collections;
using DynamicTradeInterface.Defs;
using DynamicTradeInterface.InterfaceComponents;
using DynamicTradeInterface.InterfaceComponents.TableBox;
using DynamicTradeInterface.Mod;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;

namespace DynamicTradeInterface.UserInterface
{
	[HotSwappable]
	internal class Window_DynamicTrade : Window
	{
		static Vector2 _mainButtonSize = new Vector2(160f, 40f);
		static Vector2 _showSellableItemsIconSize = new Vector2(32f, 32f);

		Table<TableRow<Tradeable>> _colonyTable;
		Table<TableRow<Tradeable>> _traderTable;
		Mod.DynamicTradeInterfaceSettings _settings;
		Tradeable? _currency;
		List<Tradeable> _tradeables;
		CaravanWidget? _caravanWidget;
		bool _refresh;
		bool _giftOnly;

		GameFont _rowFont;

		float _headerHeight;
		string _colonyHeader;
		string _colonyHeaderDescription;
		string _traderHeader;
		string _traderHeaderDescription;

		string _cancelButtonText;
		string _resetButtonText;
		string _acceptButtonText;
		string _offerGiftsText;
		string _acceptText;
		string _cannotAffordText;

		string _showSellableItemsDesc;
		string _tradeModeTip;
		string _giftModeTip;
		string _searchText;

		Texture2D _tradeModeIcon;
		Texture2D _showSellableItemsIcon;
		Texture2D _giftModeIcon;
		Texture2D _arrowIcon;
		Texture2D _resetIcon;

		Dictionary<TradeColumnDef, long>? _frameCache;


		Faction? _traderFaction;

		IEnumerable<TradeColumnDef> _columns;

		// Profiling
		Stopwatch _stopWatch;

		private Queue<string> _confirmations;

		public Window_DynamicTrade(bool giftOnly = false)
		{
			_rowFont = GameFont.Small;
			_giftOnly = giftOnly;

			_colonyTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text))
			{
				DrawScrollbarAlways = true,
				LineFont = _rowFont,
				CanSelectRows = false,
				DrawSearchBox = false,
				DrawBorder = true,
			};
			_colonyTable.LineFont = GameFont.Small;
			_traderTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text))
			{
				DrawScrollbarAlways = true,
				LineFont = _rowFont,
				CanSelectRows = false,
				DrawSearchBox = false,
				DrawBorder = true,
			};
			_traderTable.LineFont = GameFont.Small;
			_settings = Mod.DynamicTradeInterfaceMod.Settings;
			_tradeables = new List<Tradeable>();
			_stopWatch = new Stopwatch();
			_columns = _settings.GetVisibleTradeColumns();
			_refresh = false;
			_colonyHeader = string.Empty;
			_colonyHeaderDescription = string.Empty;
			_traderHeader = string.Empty;
			_traderHeaderDescription = string.Empty;
			_cancelButtonText = string.Empty;
			_resetButtonText = string.Empty;
			_acceptButtonText = string.Empty;
			_offerGiftsText = string.Empty;
			_cannotAffordText = string.Empty;
			_showSellableItemsDesc = string.Empty;
			_tradeModeTip = string.Empty;
			_giftModeTip = string.Empty;
			_acceptText = string.Empty;
			_searchText = string.Empty;

			_tradeModeIcon = Textures.TradeModeIcon;
			_showSellableItemsIcon = Textures.ShowSellableItemsIcon;
			_giftModeIcon = Textures.GiftModeIcon;
			_arrowIcon = Textures.TradeArrow;
			_resetIcon = Textures.ResetIcon;

			resizeable = true;
			draggable = true;
			forcePause = true;
			absorbInputAroundWindow = true;
			_confirmations = new Queue<string>();
		}


		public override void PreOpen()
		{
			base.PreOpen();
			DragSelect.Initialize();

			_currency = TradeSession.deal.CurrencyTradeable;
			_traderFaction = TradeSession.trader.Faction;


			RefreshData();

			_colonyHeader = Faction.OfPlayer.Name;
			string negotiatorName = TradeSession.playerNegotiator.Name.ToStringFull;
			string negotiatorValue = TradeSession.playerNegotiator.GetStatValue(StatDefOf.TradePriceImprovement).ToStringPercent();
			_colonyHeaderDescription = "NegotiatorTradeDialogInfo".Translate(negotiatorName, negotiatorValue);

			_headerHeight = Text.LineHeightOf(GameFont.Medium) + Text.LineHeightOf(GameFont.Small);

			_traderHeader = _traderFaction?.Name ?? TradeSession.trader.TraderName;
			_traderHeaderDescription = TradeSession.trader.TraderKind.LabelCap;

				
			_offerGiftsText = "OfferGifts".Translate();
			_acceptText = "AcceptButton".Translate();

			if (TradeSession.giftMode)
				_acceptButtonText = $"{_offerGiftsText} (0)";
			else
				_acceptButtonText = _acceptText;


			_resetButtonText = "ResetButton".Translate();
			_cancelButtonText = "CancelButton".Translate();
			_cannotAffordText = "MessageColonyCannotAfford".Translate();
			_showSellableItemsDesc = "CommandShowSellableItemsDesc".Translate();
			_tradeModeTip = "TradeModeTip".Translate();
			_giftModeTip = "GiftModeTip".Translate(_traderFaction);

			_caravanWidget = new CaravanWidget(_tradeables, _currency);
			_caravanWidget.Initialize();

		}

		public override Vector2 InitialSize => new Vector2(UI.screenWidth * _settings.TradeWidthPercentage, UI.screenHeight * _settings.TradeHeightPercentage);

		public override void PreClose()
		{
			base.PreClose();
			_settings.TradeWidthPercentage = windowRect.width / UI.screenWidth;
			_settings.TradeHeightPercentage = windowRect.height / UI.screenHeight;


			if (_settings.RememberSortings)
			{
				SaveSortings(_colonyTable, _settings.StoredColonySorting);
				SaveSortings(_traderTable, _settings.StoredTraderSorting);
			}

			_settings.Write();
		}

		private void SaveSortings(Table<TableRow<Tradeable>> table, List<ColumnSorting> settingsList)
		{
			settingsList.Clear();
			foreach (var item in table.GetSortQueue())
			{
				TradeColumnDef? column = (item.Item1 as TableColumn)?.Tag as TradeColumnDef;
				if (column == null)
					continue;

				settingsList.Add(new ColumnSorting(column, item.Item2));
			}
		}

		private void LoadSortings(Table<TableRow<Tradeable>> table, List<ColumnSorting> settingsList)
		{
			foreach (var item in settingsList)
			{
				TradeColumnDef columnDef = item.ColumnDef;
				if (columnDef._orderValueCallback == null)
					continue;

				TableColumn<TableRow<Tradeable>> column = table.Columns.FirstOrDefault(x => x.Tag == columnDef);
				if (column == null)
					continue;

				table.Sort(column, item.Ascending ? SortDirection.Ascending : SortDirection.Descending, false);
			}
		}

		public override void PostClose()
		{
			// Trigger PostOpen for each column in both tables.
			foreach (TradeColumnDef column in _columns)
			{
				column._postClosedCallback?.Invoke(_colonyTable.RowItems.Select(x => x.RowObject), Transactor.Colony);
				column._postClosedCallback?.Invoke(_traderTable.RowItems.Select(x => x.RowObject), Transactor.Trader);
			}

			base.PostClose();
		}


		private delegate void ColumnCallback(ref Rect rect, TableRow<Tradeable> row, TradeColumnDef columnDef, Transactor transactor);
		private void PopulateTable(Table<TableRow<Tradeable>> table, Transactor transactor)
		{
			table.Clear();

			ColumnCallback callback = ColumnCallbackSimple;
			if (_settings.ProfilingEnabled)
			{
				callback = ColumnCallbackProfiled;

				if (_frameCache == null)
					_frameCache = new Dictionary<TradeColumnDef, long>();
			}
			else
				_frameCache = null;

			foreach (Defs.TradeColumnDef columnDef in _columns)
			{
				var column = table.AddColumn(columnDef.LabelCap, columnDef.defaultWidth, tooltip: columnDef.tooltip,
						callback: (ref Rect rect, TableRow<Tradeable> row) => callback(ref rect, row, columnDef, transactor),
						orderByCallback: (rows, ascending, column, reset) => OrderByColumn(rows, ascending, column, columnDef, transactor, reset));

				column.InitialSortDirection = columnDef.initialSort;
				if (column.Width <= 1f)
					column.IsFixedWidth = false;

				column.ShowHeader = columnDef.showCaption;
				column.Tag = columnDef;
			}

			foreach (Tradeable item in _tradeables.Where(x => x.CountHeldBy(transactor) > 0))
			{
				table.AddRow(new TableRow<Tradeable>(item, item.Label + " " + item.ThingDef?.label));
			}



			Text.Font = _rowFont;
			foreach (TradeColumnDef column in _columns)
				column._postOpenCallback?.Invoke(table.RowItems.Select(x => x.RowObject), transactor);
			table.Refresh();
		}

		// Used to render columns directly.
		private void ColumnCallbackSimple(ref Rect rect, TableRow<Tradeable> row, TradeColumnDef columnDef, Transactor transactor)
		{
			columnDef._callback!(ref rect, row.RowObject, transactor, ref _refresh);
		}

		// Used to measure the time columns take to render.
		private void ColumnCallbackProfiled(ref Rect rect, TableRow<Tradeable> row, TradeColumnDef columnDef, Transactor transactor)
		{
			_stopWatch.Restart();
			columnDef._callback!(ref rect, row.RowObject, transactor, ref _refresh);
			_stopWatch.Stop();


			_frameCache!.TryGetValue(columnDef, out long frametime);
			_frameCache![columnDef] = frametime + _stopWatch.ElapsedTicks;
		}

		private void OrderByColumn(ListFilter<TableRow<Tradeable>> rows, SortDirection ascending, TableColumn column, Defs.TradeColumnDef columnDef, Transactor transactor, bool reset)
		{
			if (columnDef._orderValueCallback == null)
				return;

			Func<Tradeable, IComparable> keySelector = columnDef._orderValueCallback(transactor);

			if (keySelector != null)
			{
				if (ascending == SortDirection.Ascending)
					rows.OrderBy((row) => keySelector(row.RowObject), reset, column);
				else
					rows.OrderByDescending((row) => keySelector(row.RowObject), reset, column);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (_frameCache != null)
				_frameCache.Clear();

			bool giftMode = TradeSession.giftMode;

			if (_caravanWidget?.InCaravan == true)
			{
				_caravanWidget.Draw(new Rect(12f, 0f, inRect.width - 24f, 40f));
				inRect.yMin += 52f;
			}

			DragSelect.Reset();

			// Trade interface configuration button.
			if (Widgets.ButtonImage(new Rect(inRect.x, inRect.y, 30, 30), Textures.SettingsIcon))
			{
				var settingsMenu = new Dialog_TradeConfiguration();
				settingsMenu.OnClosed += SettingsMenu_OnClosed;
				Find.WindowStack.Add(settingsMenu);
			}

			float currencyLineHeight = 0;
			if (_currency != null)
				currencyLineHeight = Text.LineHeightOf(_rowFont);
			inRect.SplitHorizontallyWithMargin(out Rect body, out Rect footer, out _, GenUI.GapTiny, bottomHeight: currencyLineHeight + _mainButtonSize.y + GenUI.GapSmall);

			Rect left, right;
			Rect top, bottom;

			if (giftMode == false)
				body.SplitVerticallyWithMargin(out left, out right, out _, GenUI.GapTiny, inRect.width / 2);
			else
				left = right = body;

			// Colony
			left.SplitHorizontallyWithMargin(out top, out bottom, out _, GenUI.GapSmall + Text.LineHeightOf(GameFont.Small), _headerHeight);

			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(top, _colonyHeader);

			Text.Anchor = TextAnchor.LowerCenter;
			Text.Font = GameFont.Small;
			Widgets.Label(top, _colonyHeaderDescription);

			Text.Anchor = TextAnchor.UpperLeft;
			DrawSearchBox(top.x, top.yMax + GenUI.GapTiny, body.width, (int)Text.LineHeightOf(GameFont.Small));

			_colonyTable.Draw(bottom);

			if (giftMode == false)
			{
				// Trader
				right.SplitHorizontallyWithMargin(out top, out bottom, out _, GenUI.GapSmall + Text.LineHeightOf(GameFont.Small), _headerHeight);

				Text.Anchor = TextAnchor.UpperCenter;
				Text.Font = GameFont.Medium;
				Widgets.Label(top, _traderHeader);

				Text.Anchor = TextAnchor.LowerCenter;
				Text.Font = GameFont.Small;
				Widgets.Label(top, _traderHeaderDescription);

				Text.Anchor = TextAnchor.UpperLeft;
				_traderTable.Draw(bottom);
			}

			if (_currency != null && TradeSession.giftMode == false)
				DrawCurrencyRow(new Rect(footer.x, footer.y, footer.width, currencyLineHeight), _currency);


			float width = _mainButtonSize.x * 2 + _mainButtonSize.y + GenUI.GapTiny * 2;
			Rect mainButtonRect = new Rect(footer.center.x - width / 2, footer.yMax - GenUI.GapTiny - _mainButtonSize.y, _mainButtonSize.x, _mainButtonSize.y);
			// Accept
			if (Widgets.ButtonText(mainButtonRect, _acceptButtonText))
			{
				OnAccept();
			}
			mainButtonRect.x += mainButtonRect.width + GenUI.GapTiny;

			// Reset
			Rect resetButtonRect = new Rect(mainButtonRect.x, mainButtonRect.y, mainButtonRect.height, mainButtonRect.height);
			float textureSize = mainButtonRect.height - GenUI.GapSmall - GenUI.GapTiny;
			if (Widgets.ButtonImageWithBG(resetButtonRect, _resetIcon, new Vector2(textureSize, textureSize)))
			{
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				ResetTrade();
				_refresh = true;
			}
			if (Mouse.IsOver(resetButtonRect))
				TooltipHandler.TipRegion(resetButtonRect, _resetButtonText);

			mainButtonRect.x += resetButtonRect.width + GenUI.GapTiny;

			// Cancel
			if (Widgets.ButtonText(mainButtonRect, _cancelButtonText))
			{
				Close();
				Event.current.Use();
				return;
			}


			// Show sellable items
			float y = _mainButtonSize.y;
			Rect showSellableRect = new Rect(footer.width - y, mainButtonRect.y, y, y);
			if (Widgets.ButtonImageWithBG(showSellableRect, _showSellableItemsIcon, _showSellableItemsIconSize))
			{
				Find.WindowStack.Add(new Dialog_SellableItems(TradeSession.trader));
			}
			TooltipHandler.TipRegionByKey(showSellableRect, _showSellableItemsDesc);


			// Gift/Trade mode toggle
			if (_traderFaction != null && _giftOnly == false && _traderFaction.def.permanentEnemy == false)
			{
				Rect rect7 = new Rect(showSellableRect.x - y - 4f, showSellableRect.y, y, y);
				if (TradeSession.giftMode)
				{
					if (Widgets.ButtonImageWithBG(rect7, _tradeModeIcon, new Vector2(32f, 32f)))
					{
						TradeSession.giftMode = false;
						TradeSession.deal.Reset();
						_refresh = true;
						RefreshData();
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
					TooltipHandler.TipRegion(rect7, _tradeModeTip);
				}
				else
				{
					if (Widgets.ButtonImageWithBG(rect7, _giftModeIcon, new Vector2(32f, 32f)))
					{
						TradeSession.giftMode = true;
						TradeSession.deal.Reset();
						_refresh = true;
						RefreshData();
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
					TooltipHandler.TipRegion(rect7, _giftModeTip);
				}
			}


			if (_frameCache != null)
			{
				foreach (KeyValuePair<TradeColumnDef, long> item in _frameCache)
				{
					if (_settings.TradeColumnProfilings.TryGetValue(item.Key, out Queue<long> profilings) == false)
					{
						profilings = new Queue<long>();
					}

					profilings.Enqueue(item.Value);
					if (profilings.Count > 200)
						profilings.Dequeue();

					_settings.TradeColumnProfilings[item.Key] = profilings;
				}
			}


			if (_refresh)
			{
				_refresh = false;
				_colonyTable.Refresh();
				_traderTable.Refresh();
				_caravanWidget?.SetDirty();
				TradeSession.deal.UpdateCurrencyCount();


				if (TradeSession.giftMode)
				{
					string goodwillChange = FactionGiftUtility.GetGoodwillChange(TradeSession.deal.AllTradeables, _traderFaction).ToStringWithSign();
					_acceptButtonText = $"{_offerGiftsText} ({goodwillChange})";
				}
				else
					_acceptButtonText = _acceptText;
			}
		}

		private void DrawSearchBox(float x, float y, float width, float height)
		{
			float clearButtonSize = Text.LineHeight;
			Rect searchBox = new Rect(x, y, width - clearButtonSize - Table<ITableRow>.CELL_SPACING, clearButtonSize);
			_searchText = Widgets.TextField(searchBox, _searchText);
			if (Widgets.ButtonText(new Rect(searchBox.xMax + Table<ITableRow>.CELL_SPACING, y, clearButtonSize, clearButtonSize), "X"))
				_searchText = "";


			if (_searchText == string.Empty)
				Widgets.NoneLabelCenteredVertically(new Rect(searchBox.x + 5, searchBox.y, _colonyTable.SEARCH_PLACEHOLDER_SIZE, Text.LineHeight), _colonyTable.SEARCH_PLACEHOLDER);

			_colonyTable.Filter = _searchText;
			_traderTable.Filter = _searchText;
		}

		private void SettingsMenu_OnClosed(object sender, bool e)
		{
			RefreshData();
		}

		private void RefreshData()
		{
			LoadWares();

			PopulateTable(_colonyTable, Transactor.Colony);
			PopulateTable(_traderTable, Transactor.Trader);

			if (_settings.RememberSortings)
			{
				LoadSortings(_colonyTable, _settings.StoredColonySorting);
				LoadSortings(_traderTable, _settings.StoredTraderSorting);
			}
		}

		private void LoadWares()
		{
			IEnumerable<Tradeable> filteredWares = TradeSession.deal.AllTradeables;
			if (TradeSession.giftMode == false)
				filteredWares = filteredWares.Where(x => x.IsCurrency == false);

			if (TradeSession.trader.TraderKind.hideThingsNotWillingToTrade || _settings.ExcludeUnwillingItems)
				filteredWares = filteredWares.Where(x => x.TraderWillTrade);

			_tradeables.Clear();
			_tradeables.AddRange(filteredWares.OrderByDescending(x => x.TraderWillTrade)
				.ThenBy((Tradeable tr) => tr, TransferableSorterDefOf.Category.Comparer)
				.ThenBy((Tradeable tr) => tr, TransferableSorterDefOf.MarketValue.Comparer)
				.ThenBy((Tradeable tr) => TransferableUIUtility.DefaultListOrderPriority(tr))
				.ThenBy((Tradeable tr) => tr.ThingDef.label)
				.ThenBy((Tradeable tr) => tr.AnyThing.TryGetQuality(out var qc) ? ((int)qc) : (-1))
				.ThenBy((Tradeable tr) => tr.AnyThing.HitPoints));
		}

		private void DrawCurrencyRow(Rect currencyRowRect, Tradeable currency)
		{
			bool shouldFlash = false;
			if (Dialog_Trade.lastCurrencyFlashTime > 0)
			{
				shouldFlash = Time.time - Dialog_Trade.lastCurrencyFlashTime < 1f;
				if (shouldFlash)
				{
					GUI.DrawTexture(currencyRowRect, TransferableUIUtility.FlashTex);
				}
				else
					Dialog_Trade.lastCurrencyFlashTime = 0;
			}

			float curX = currencyRowRect.x;
			if (currency.IsThing)
			{
				Thing thing = currency.AnyThing;
				if (thing != null)
				{
					Widgets.ThingIcon(new Rect(curX, currencyRowRect.y, 40, currencyRowRect.height), thing);
					curX += 40;
					Widgets.InfoCardButton(curX, currencyRowRect.y, thing);
					curX += 20;

					Rect labelRect = new Rect(curX, currencyRowRect.y, 200, currencyRowRect.height);
					Widgets.Label(labelRect, currency.LabelCap);

					if (Mouse.IsOver(labelRect))
					{
						TooltipHandler.TipRegion(labelRect, () =>
						{
							string tipDescription = currency.TipDescription;
							if (String.IsNullOrWhiteSpace(tipDescription) == false)
							{
								string text = currency.LabelCap;
								text = text + ": " + tipDescription + TransferableUIUtility.ContentSourceDescription(thing);
								return text;
							}
							return string.Empty;
						}, currency.GetHashCode());
					}
				}
			}

			float centerX = currencyRowRect.center.x;
			currencyRowRect.SplitVerticallyWithMargin(out Rect left, out Rect right, out _, 100, currencyRowRect.width / 2);

			// Colony currency
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(left, currency.CountHeldBy(Transactor.Colony).ToStringCached());

			// Counter
			int countToTransfer = currency.CountToTransfer;
			GUI.color = countToTransfer == 0 ? TransferableUIUtility.ZeroCountColor : Color.white;
			Rect currencyLabelRect = new Rect(centerX - 50, currencyRowRect.y, 100, currencyRowRect.height);
			Widgets.Label(currencyLabelRect, countToTransfer.ToStringCached());
			GUI.color = Color.white;
			
			// Arrow
			if (countToTransfer != 0)
			{
				Rect position = new Rect(currencyLabelRect.x + currencyLabelRect.width / 2f - (float)(_arrowIcon.width / 2), currencyLabelRect.y + currencyLabelRect.height / 2f - (float)(_arrowIcon.height / 2), _arrowIcon.width, _arrowIcon.height);
				TransferablePositiveCountDirection positiveDirection = currency.PositiveCountDirection;
				if ((positiveDirection == TransferablePositiveCountDirection.Source && countToTransfer > 0) || (positiveDirection == TransferablePositiveCountDirection.Destination && countToTransfer < 0))
				{
					position.x += position.width;
					position.width *= -1f;
				}
				GUI.DrawTexture(position, _arrowIcon);
			}




			// Trader currency
			Widgets.Label(right, currency.CountHeldBy(Transactor.Trader).ToStringCached());

			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void ConfirmTrade()
		{
			_confirmations.Clear();
			// Execute all validations and queue confirmation dialog texts for those that fail.
			foreach (var validator in _settings.ValidationDefs)
			{
				// _settings.ValidationDefs only contains validators with non-null actions and text. It is safe to not check
				// for null values in this if block.
				var confirmationText = validator.validationCallback!();
				if (confirmationText.HasValue)
				{
					_confirmations.Enqueue(confirmationText);
				}
			}

			ShowNextConfirmation();
		}

		private void ShowNextConfirmation()
		{
			if (_confirmations.TryDequeue(out var validationText))
			{
				SoundDefOf.ClickReject.PlayOneShotOnCamera();
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(validationText, ShowNextConfirmation));
				return;
			}

			ExecuteTrade();
		}

		private void OnAccept()
		{
			ConfirmTrade();
			Event.current.Use();
		}

		private void ExecuteTrade()
		{
			// This check exists in TradeSession.deal.TryExecute and directly references Dialog_Trade to flash silver.
			if (_currency == null || _currency.CountPostDealFor(Transactor.Colony) < 0)
			{
				FlashSilver();
				Messages.Message(_cannotAffordText, MessageTypeDefOf.RejectInput, historical: false);
				return;
			}

			if (TradeSession.deal.TryExecute(out var actuallyTraded))
			{
				if (actuallyTraded)
				{
					SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
					TradeSession.playerNegotiator.GetCaravan()?.RecacheImmobilizedNow();
					Close(doCloseSound: false);
				}
				else
				{
					Close();
				}
			}
		}

		private void ResetTrade()
		{
			if (_tradeables == null)
				return;

			for (int i = _tradeables.Count - 1; i >= 0; i--)
			{
				_tradeables[i].ForceTo(0);
			}
		}

		public void FlashSilver()
		{
			Dialog_Trade.lastCurrencyFlashTime = Time.time;
		}
	}
}
