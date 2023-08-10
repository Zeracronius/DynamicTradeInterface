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
	[HotSwappable]
	internal static class ColumnButtons
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (row.Interactive == false)
				return;

			TransferablePositiveCountDirection positiveDirection = row.PositiveCountDirection;

			int baseCount = positiveDirection == TransferablePositiveCountDirection.Source ? 1 : -1;

			// Source is left.
			int minQuantity, maxQuantity;
			minQuantity = row.GetMinimumToTransfer();
			maxQuantity = row.GetMaximumToTransfer();


			int adjustMultiplier = GenUI.CurrentAdjustmentMultiplier();
			int adjustAmount = baseCount * adjustMultiplier;
			bool largeRange = row.GetRange() > 1;

			float gap = 2;
			// << < 0 > >>
			float width = rect.width / 5 - gap;
			Rect baseButtonRect = new Rect(rect.x, rect.y, width, rect.height);

			// Draw left arrows
			Rect button = new Rect(baseButtonRect);
			if (row.CanAdjustBy(adjustAmount).Accepted)
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
			if (Widgets.ButtonText(baseButtonRect, "0"))
			{
				row.AdjustTo(0);
				refresh = true;
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
			baseButtonRect.x += baseButtonRect.width + gap;

			// Draw right arrows
			if (row.CanAdjustBy(-adjustAmount).Accepted)
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
			int currency = TradeSession.deal.CurrencyTradeable.CountHeldBy(transactor);
			return (int)(currency / price);
		}
	}
}
