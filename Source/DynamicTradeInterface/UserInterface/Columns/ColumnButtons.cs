using DynamicTradeInterface.Attributes;
using RimWorld;
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
		private static Dictionary<Tradeable, (bool, bool, int, int)> _editableCache = new Dictionary<Tradeable, (bool, bool, int, int)>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			foreach (var row in rows)
				_editableCache[row] = (row.TraderWillTrade == true && row.Interactive == true, row.GetRange() > 1, row.GetMinimumToTransfer(), row.GetMaximumToTransfer());
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_editableCache.Clear();
		}


		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			// Can edit?
			if (_editableCache.TryGetValue(row, out (bool, bool, int, int) cached) == false)
				return;

			if (cached.Item1 == false)
				return;

			int currentAmountToTransfer = row.CountToTransfer;

			//if (currentAmountToTransfer == 0 && Mouse.IsOver(rect) == false)
			//	return;

			TransferablePositiveCountDirection positiveDirection = row.PositiveCountDirection;

			int baseCount = positiveDirection == TransferablePositiveCountDirection.Source ? 1 : -1;

			// Source is left.
			int adjustMultiplier = GenUI.CurrentAdjustmentMultiplier();
			int adjustAmount = baseCount * adjustMultiplier;
			bool largeRange = cached.Item2;

			float gap = 2;
			// << < 0 > >>
			float width = rect.width / 5 - gap;
			Rect baseButtonRect = new Rect(rect.x, rect.y, width, rect.height);

			// Draw left arrows
			Rect button = new Rect(baseButtonRect);
			if (CanAdjustBy(adjustAmount, currentAmountToTransfer, cached.Item3, cached.Item4))
			{
				if (largeRange)
				{
					if (Widgets.ButtonText(button, "<<"))
					{
						if (positiveDirection == TransferablePositiveCountDirection.Source)
						{
							BuyMore(row);
						}
						else
						{
							SellMore(row);
						}
						refresh = true;
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
					button.x += button.width + gap;
				}
				else
				{
					button.width += gap + baseButtonRect.width;
				}

				if (Widgets.ButtonText(button, "<"))
				{
					row.AdjustBy(adjustAmount);
					refresh = true;
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				baseButtonRect.x = button.xMax + gap;
			}
			else
			{
				baseButtonRect.x += baseButtonRect.width * 2 + gap * 2;
			}

			// Draw reset
			if (currentAmountToTransfer != 0)
			{
				if (Widgets.ButtonText(baseButtonRect, "0"))
				{
					row.AdjustTo(0);
					refresh = true;
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
			}
			baseButtonRect.x += baseButtonRect.width + gap;

			// Draw right arrows
			if (CanAdjustBy(-adjustAmount, currentAmountToTransfer, cached.Item3, cached.Item4))
			{
				if (largeRange == false)
					baseButtonRect.width = baseButtonRect.width * 2 + gap;


				if (Widgets.ButtonText(baseButtonRect, ">"))
				{
					row.AdjustBy(-adjustAmount);
					refresh = true;
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();

				}
				baseButtonRect.x += baseButtonRect.width + gap;

				if (largeRange)
				{
					if (Widgets.ButtonText(baseButtonRect, ">>"))
					{
						if (positiveDirection == TransferablePositiveCountDirection.Source)
						{
							SellMore(row);
						}
						else
						{
							BuyMore(row);
						}
						refresh = true;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				}
			}
		}

		private static bool CanAdjustBy(int target, int current, int minimum, int maximum)
		{
			int total = current + target;
			if (total < minimum || total > maximum)
				return false;

			return true;
		}

		/// <summary>
		/// max sellable ← can sell ← 0 ← can buy ← max buyable
		/// </summary>
		/// <param name="row">The item in question.</param>
		private static void SellMore(Tradeable row)
		{
			if (TradeSession.giftMode || Event.current.shift)
			{
				row.AdjustTo(row.GetMinimumToTransfer());
				return;
			}

			int currentAmount = row.CountToTransfer;
			if (currentAmount > 0)
			{
				BuyLess(row, currentAmount);
				return;
			}

			int traderCanBuy = MaxAmount(row, TradeAction.PlayerSells);
			if (currentAmount > traderCanBuy)
				row.AdjustTo(traderCanBuy);
			else
				row.AdjustTo(row.GetMinimumToTransfer());
		}

		/// <summary>
		/// max sellable → can sell → 0
		/// </summary>
		/// <param name="row">The item in question.</param>
		private static void SellLess(Tradeable row, int currentAmount)
		{
			int traderCanBuy = MaxAmount(row, TradeAction.PlayerSells);
			if (currentAmount < traderCanBuy)
				row.AdjustTo(traderCanBuy);
			else
				row.AdjustTo(0);
		}

		/// <summary>
		/// max sellable → can sell → 0 → can buy → max buyable
		/// </summary>
		/// <param name="row">The item in question.</param>
		private static void BuyMore(Tradeable row)
		{
			if (TradeSession.giftMode || Event.current.shift)
			{
				row.AdjustTo(row.GetMaximumToTransfer());
				return;
			}

			int currentAmount = row.CountToTransfer;
			if (currentAmount < 0)
			{
				SellLess(row, currentAmount);
				return;
			}

			int colonyCanBuy = MaxAmount(row, TradeAction.PlayerBuys);

			// If current value is below what the colony can buy, stop at that first.
			if (currentAmount < colonyCanBuy)
				row.AdjustTo(colonyCanBuy);
			else
				row.AdjustTo(row.GetMaximumToTransfer());
		}

		/// <summary>
		/// 0 ← can buy ← max buyable
		/// </summary>
		/// <param name="row">The item in question.</param>
		private static void BuyLess(Tradeable row, int currentAmount)
		{
			int traderCanBuy = MaxAmount(row, TradeAction.PlayerBuys);
			if (currentAmount > traderCanBuy)
				row.AdjustTo(traderCanBuy);
			else
				row.AdjustTo(0);
		}


		private static int MaxAmount(Tradeable row, TradeAction action)
		{
			Transactor transactor = Transactor.Colony;
			if (action == TradeAction.PlayerSells)
				transactor = Transactor.Trader;

			float price = row.GetPriceFor(action);
			int currency = TradeSession.deal.CurrencyTradeable.CountPostDealFor(transactor);
			return (int)(currency / price);
		}
	}
}
