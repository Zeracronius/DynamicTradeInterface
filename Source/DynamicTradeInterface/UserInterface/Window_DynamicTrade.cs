using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.InterfaceComponents.TableBox;
using DynamicTradeInterface.Mod;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

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

		GameFont _rowFont;


		static Vector2 _mainButtonSize = new Vector2(160f, 40f);
		static Vector2 _showSellableItemsIconSize = new Vector2(32f, 32f);


		float _headerHeight;
		string _colonyHeader;
		string _colonyHeaderDescription;
		string _traderHeader;
		string _traderHeaderDescription;

		string _cancelButtonText;
		string _resetButtonText;
		string _acceptButtonText;
		string _confirmShortFundsText;
		string _offerGiftsText;
		string _cannotAffordText;

		public Window_DynamicTrade()
		{
			_rowFont = GameFont.Small;

			_colonyTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text))
			{
				DrawScrollbarAlways = true,
				LineFont = _rowFont,
			};
			_colonyTable.LineFont = GameFont.Small;
			_traderTable = new Table<TableRow<Tradeable>>((item, text) => item.SearchString.Contains(text))
			{
				DrawScrollbarAlways = true,
				LineFont = _rowFont,
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

		public void Initialize(List<Tradeable> tradeables)
		{
			_currency = TradeSession.deal.CurrencyTradeable;
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

			if (TradeSession.giftMode)
			{
				_offerGiftsText = "OfferGifts".Translate();

				string goodwillChange = FactionGiftUtility.GetGoodwillChange(TradeSession.deal.AllTradeables, TradeSession.trader.Faction).ToStringWithSign();
				_acceptButtonText = $"{_offerGiftsText} ({goodwillChange})";
			}
			else
				_acceptButtonText = "AcceptButton".Translate();


			_resetButtonText = "ResetButton".Translate();
			_cancelButtonText = "CancelButton".Translate();
			_confirmShortFundsText = "ConfirmTraderShortFunds".Translate();
			_cannotAffordText = "MessageColonyCannotAfford".Translate();



			_caravanWidget = new CaravanWidget(tradeables, _currency);
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

			float currencyLineHeight = 0;
			if (_currency != null)
				currencyLineHeight = Text.LineHeightOf(_rowFont);
			inRect.SplitHorizontallyWithMargin(out Rect body, out Rect footer, out _, GenUI.GapTiny, bottomHeight: currencyLineHeight + _mainButtonSize.y + GenUI.GapSmall);

			Rect left, right;
			body.SplitVerticallyWithMargin(out left, out right, out _, GenUI.GapTiny, inRect.width / 2);

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

			if (_currency != null)
				DrawCurrencyRow(new Rect(footer.x, footer.y, footer.width, currencyLineHeight), _currency);


			float width = _mainButtonSize.x * 3 + GenUI.GapTiny * 2;
			Rect mainButtonRect = new Rect(footer.center.x - width / 2, footer.yMax - GenUI.GapTiny - _mainButtonSize.y, _mainButtonSize.x, _mainButtonSize.y);
			// Accept
			if (Widgets.ButtonText(mainButtonRect, _acceptButtonText))
			{
				OnAccept();
			}
			mainButtonRect.x += mainButtonRect.width + GenUI.GapTiny;

			// Reset
			if (Widgets.ButtonText(mainButtonRect, _resetButtonText))
			{
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				ResetTrade();
				_refresh = true;
			}
			mainButtonRect.x += mainButtonRect.width + GenUI.GapTiny;

			// Cancel
			if (Widgets.ButtonText(mainButtonRect, _cancelButtonText))
			{
				Close();
				Event.current.Use();
				return;
			}



			float y = _mainButtonSize.y;
			Rect rect6 = new Rect(footer.width - y, mainButtonRect.y, y, y);
			if (Widgets.ButtonImageWithBG(rect6, Textures.ShowSellableItemsIcon, _showSellableItemsIconSize))
			{
				Find.WindowStack.Add(new Dialog_SellableItems(TradeSession.trader));
			}


			if (_refresh)
			{
				_refresh = false;
				_colonyTable.Refresh();
				_traderTable.Refresh();
				_caravanWidget.SetDirty();
				TradeSession.deal.UpdateCurrencyCount();


				if (TradeSession.giftMode)
				{
					string goodwillChange = FactionGiftUtility.GetGoodwillChange(TradeSession.deal.AllTradeables, TradeSession.trader.Faction).ToStringWithSign();
					_acceptButtonText = $"{_offerGiftsText} ({goodwillChange})";
				}
			}
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
				Texture2D tradeArrow = Mod.Textures.TradeArrow;
				Rect position = new Rect(currencyLabelRect.x + currencyLabelRect.width / 2f - (float)(tradeArrow.width / 2), currencyLabelRect.y + currencyLabelRect.height / 2f - (float)(tradeArrow.height / 2), tradeArrow.width, tradeArrow.height);
				TransferablePositiveCountDirection positiveDirection = currency.PositiveCountDirection;
				if ((positiveDirection == TransferablePositiveCountDirection.Source && countToTransfer > 0) || (positiveDirection == TransferablePositiveCountDirection.Destination && countToTransfer < 0))
				{
					position.x += position.width;
					position.width *= -1f;
				}
				GUI.DrawTexture(position, tradeArrow);
			}




			// Trader currency
			Widgets.Label(right, currency.CountHeldBy(Transactor.Trader).ToStringCached());

			Text.Anchor = TextAnchor.UpperLeft;
		}


		private void OnAccept()
		{
			if (TradeSession.deal.DoesTraderHaveEnoughSilver())
			{
				ExecuteTrade();
			}
			else
			{
				FlashSilver();
				SoundDefOf.ClickReject.PlayOneShotOnCamera();
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(_confirmShortFundsText, ExecuteTrade));
			}
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
