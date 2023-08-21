using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.InterfaceComponents;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal static class ColumnButtons
	{
		private static Dictionary<Tradeable, Button> _editableCache = new Dictionary<Tradeable, Button>();
		private static Mod.DynamicTradeInterfaceSettings? _settings;

		private class Button
		{
			private Tradeable _row;
			private bool _tradeable;
			private bool _largeRange;
			private int _minimum;
			private int _maximum;

			public Button(Tradeable row)
			{
				_row = row;
				_tradeable = row.TraderWillTrade == true && row.Interactive == true;
				_largeRange = row.GetRange() > 1;
				_minimum = row.GetMinimumToTransfer();
				_maximum = row.GetMaximumToTransfer();
			}

			public void Draw(ref Rect rect, Transactor transactor, ref bool refresh)
			{
				if (!_tradeable)
					return;

				int currentAmountToTransfer = _row.CountToTransfer;

				if (_settings?.GhostButtons == true && currentAmountToTransfer == 0 && Mouse.IsOver(rect.ExpandedBy(rect.width / 2, rect.height * 2)) == false)
					return;

				TransferablePositiveCountDirection positiveDirection = _row.PositiveCountDirection;

				int baseCount = positiveDirection == TransferablePositiveCountDirection.Source ? 1 : -1;

				// Source is left.
				int adjustMultiplier = GenUI.CurrentAdjustmentMultiplier();
				int adjustAmount = baseCount * adjustMultiplier;

				float gap = 2;
				// << < 0 > >>
				float width = rect.width / 5 - gap;
				Rect baseButtonRect = new Rect(rect.x, rect.y, width, rect.height);

				// Draw left arrows
				Rect button = new Rect(baseButtonRect);
				if (CanAdjustBy(adjustAmount, currentAmountToTransfer)) {
					if (_largeRange) {
						if (Widgets.ButtonText(button, "<<") || DragSelect.IsPainting(button, DragSelect.PaintingDirection.Left)) {
							if (positiveDirection == TransferablePositiveCountDirection.Source) {
								BuyMore();
							} else {
								SellMore();
							}
							refresh = true;
							SoundDefOf.Tick_High.PlayOneShotOnCamera();
						}
						button.x += button.width + gap;
					} else {
						button.width += gap + baseButtonRect.width;
					}

					if (Widgets.ButtonText(button, "<") || (!_largeRange && DragSelect.IsPainting(button, DragSelect.PaintingDirection.Left))) {
						_row.AdjustBy(adjustAmount);
						refresh = true;
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
					baseButtonRect.x = button.xMax + gap;
				} else {
					baseButtonRect.x += baseButtonRect.width * 2 + gap * 2;
				}

				// Draw reset
				if (currentAmountToTransfer != 0) {
					if (Widgets.ButtonText(baseButtonRect, "0") || DragSelect.IsPainting(baseButtonRect, DragSelect.PaintingDirection.Middle)) {
						_row.AdjustTo(0);
						refresh = true;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				}
				baseButtonRect.x += baseButtonRect.width + gap;

				// Draw right arrows
				if (CanAdjustBy(-adjustAmount, currentAmountToTransfer)) {
					if (_largeRange == false)
						baseButtonRect.width = baseButtonRect.width * 2 + gap;


					if (Widgets.ButtonText(baseButtonRect, ">") || (!_largeRange && DragSelect.IsPainting(baseButtonRect, DragSelect.PaintingDirection.Right))) {
						_row.AdjustBy(-adjustAmount);
						refresh = true;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();

					}
					baseButtonRect.x += baseButtonRect.width + gap;

					if (_largeRange) {
						if (Widgets.ButtonText(baseButtonRect, ">>") || DragSelect.IsPainting(baseButtonRect, DragSelect.PaintingDirection.Right)) {
							if (positiveDirection == TransferablePositiveCountDirection.Source) {
								SellMore();
							} else {
								BuyMore();
							}
							refresh = true;
							SoundDefOf.Tick_Low.PlayOneShotOnCamera();
						}
					}
				}
			}

			private bool CanAdjustBy(int target, int current)
			{
				int total = current + target;
				if (total < _minimum || total > _maximum)
					return false;

				return true;
			}

			/// <summary>
			/// max sellable ← can sell ← 0 ← can buy ← max buyable
			/// </summary>
			/// <param name="row">The item in question.</param>
			private void SellMore()
			{
				if (TradeSession.giftMode || Event.current.shift) {
					_row.AdjustTo(_row.GetMinimumToTransfer());
					return;
				}

				int currentAmount = _row.CountToTransfer;
				if (currentAmount > 0) {
					BuyLess(currentAmount);
					return;
				}

				int traderCanBuy = MaxAmount(TradeAction.PlayerSells);
				if (currentAmount > traderCanBuy)
					_row.AdjustTo(traderCanBuy);
				else
					_row.AdjustTo(_row.GetMinimumToTransfer());
			}

			/// <summary>
			/// max sellable → can sell → 0
			/// </summary>
			/// <param name="row">The item in question.</param>
			private void SellLess(int currentAmount)
			{
				int traderCanBuy = MaxAmount(TradeAction.PlayerSells);
				if (currentAmount < traderCanBuy)
					_row.AdjustTo(traderCanBuy);
				else
					_row.AdjustTo(0);
			}

			/// <summary>
			/// max sellable → can sell → 0 → can buy → max buyable
			/// </summary>
			/// <param name="row">The item in question.</param>
			private void BuyMore()
			{
				if (TradeSession.giftMode || Event.current.shift) {
					_row.AdjustTo(_row.GetMaximumToTransfer());
					return;
				}

				int currentAmount = _row.CountToTransfer;
				if (currentAmount < 0) {
					SellLess(currentAmount);
					return;
				}

				int colonyCanBuy = MaxAmount(TradeAction.PlayerBuys);

				// If current value is below what the colony can buy, stop at that first.
				if (currentAmount < colonyCanBuy)
					_row.AdjustTo(colonyCanBuy);
				else
					_row.AdjustTo(_row.GetMaximumToTransfer());
			}

			/// <summary>
			/// 0 ← can buy ← max buyable
			/// </summary>
			/// <param name="row">The item in question.</param>
			private void BuyLess(int currentAmount)
			{
				int traderCanBuy = MaxAmount(TradeAction.PlayerBuys);
				if (currentAmount > traderCanBuy)
					_row.AdjustTo(traderCanBuy);
				else
					_row.AdjustTo(0);
			}

			private int MaxAmount(TradeAction action)
			{
				Transactor transactor = Transactor.Colony;
				if (action == TradeAction.PlayerSells)
					transactor = Transactor.Trader;

				float price = _row.GetPriceFor(action);
				int currency = TradeSession.deal.CurrencyTradeable.CountPostDealFor(transactor);
				return (int)(currency / price);
			}
		}

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			foreach (var row in rows)
				_editableCache[row] = new Button(row);

			_settings = Mod.DynamicTradeInterfaceMod.Settings;
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_editableCache.Clear();
			_settings = null;
		}


		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			// Can edit?
			if (_editableCache.TryGetValue(row, out Button button))
				button.Draw(ref rect, transactor, ref refresh);
		}
	}
}
