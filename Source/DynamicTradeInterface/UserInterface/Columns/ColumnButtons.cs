using DynamicTradeInterface.InterfaceComponents;
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
		private static Dictionary<Tradeable, Cache> _editableCache = new Dictionary<Tradeable, Cache>();
		private static Mod.DynamicTradeInterfaceSettings? _settings;
		private static int _colonyPostDeal = 0;
		private static int _traderPostDeal = 0;
		private static bool _invalidatePostDeal = false;
		private static Color _outOfFundsColor = new Color(0.5f, 0, 0);

		private static string BuyKey = "";
		private static string BuyAll = "";
		private static string BuyAffordable = "";
		private static string SellKey = "";
		private static string SellAll = "";
		private static string SellAffordable = "";
		private static string Unaffordable = "";

		private class Cache
		{
			public bool WillTrade;
			public bool LargeRange;
			public int MinimumQuantity;
			public int MaximumQuantity;
			public float ColonyBuyPrice;
			public float ColonySellPrice;
		}

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_settings = Mod.DynamicTradeInterfaceMod.Settings;
			if (TradeSession.giftMode)
			{
				BuyKey = "DynamicTradeWindowGiftLess";
				BuyAll = "DynamicTradeWindowGiftNone".Translate();
				BuyKey = "DynamicTradeWindowGiftMore";
				SellAll = "DynamicTradeWindowGiftAll".Translate();
			}
			else
			{
				BuyKey = "DynamicTradeWindowBuyMore";
				BuyAll = "DynamicTradeWindowBuyAll".Translate();
				BuyAffordable = "DynamicTradeWindowBuyAffordable".Translate();
				SellKey = "DynamicTradeWindowSellMore";
				SellAll = "DynamicTradeWindowSellAll".Translate();
				SellAffordable = "DynamicTradeWindowSellAffordable".Translate();
				Unaffordable = "DynamicTradeWindowUnaffordable".Translate();
			}


			if (TradeSession.giftMode == false)
			{
				int postDeal = TradeSession.deal.CurrencyTradeable.CountPostDealFor(transactor);
				switch (transactor)
				{
					case Transactor.Colony:
						_colonyPostDeal = postDeal;
						break;

					case Transactor.Trader:
						_traderPostDeal = postDeal;
						break;
				}
			}

			foreach (var row in rows)
				_editableCache[row] = new Cache()
				{
					WillTrade = row.TraderWillTrade == true && row.Interactive == true,
					LargeRange = row.GetRange() > 1,
					MinimumQuantity = row.GetMinimumToTransfer(),
					MaximumQuantity = row.GetMaximumToTransfer(),
					ColonyBuyPrice =  row.GetPriceFor(TradeAction.PlayerBuys),
					ColonySellPrice = row.GetPriceFor(TradeAction.PlayerSells),
				};
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_editableCache.Clear();
			_settings = null;
		}


		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			// Can edit?
			if (_editableCache.TryGetValue(row, out Cache cached) == false)
				return;

			if (cached.WillTrade == false)
				return;

			bool canSellAny = true;
			bool canBuyAny = true;

			if (TradeSession.giftMode == false)
			{
				if (_invalidatePostDeal && Event.current.type == EventType.Repaint)
				{
					_invalidatePostDeal = false;
					_colonyPostDeal = TradeSession.deal.CurrencyTradeable.CountPostDealFor(Transactor.Colony);
					_traderPostDeal = TradeSession.deal.CurrencyTradeable.CountPostDealFor(Transactor.Trader);
				}

				canSellAny = cached.ColonySellPrice < _traderPostDeal;
				canBuyAny = cached.ColonyBuyPrice < _colonyPostDeal;
			}
			int currentAmountToTransfer = row.CountToTransfer;

			bool mouseNear = Mouse.IsOver(rect.ExpandedBy(rect.width / 2, rect.height * 2));
			if (_settings?.GhostButtons == true && currentAmountToTransfer == 0 && mouseNear == false)
				return;

			TransferablePositiveCountDirection positiveDirection = row.PositiveCountDirection;

			int baseCount = positiveDirection == TransferablePositiveCountDirection.Source ? 1 : -1;

			// Source is left.
			int adjustMultiplier = GenUI.CurrentAdjustmentMultiplier();
			int adjustAmount = baseCount * adjustMultiplier;
			bool largeRange = cached.LargeRange;

			float gap = 2;
			// << < 0 > >>
			float width = rect.width / 5 - gap;
			Rect baseButtonRect = new Rect(rect.x, rect.y, width, rect.height);


			Color normal = GUI.color;
			Color buttonColor = normal;
			// Draw left arrows
			Rect button = new Rect(baseButtonRect);
			if (CanAdjustBy(adjustAmount, currentAmountToTransfer, cached.MinimumQuantity, cached.MaximumQuantity))
			{
				if ((canBuyAny == false && positiveDirection == TransferablePositiveCountDirection.Source) ||
					(canSellAny == false && positiveDirection == TransferablePositiveCountDirection.Destination))
					buttonColor = _outOfFundsColor;

				if (largeRange)
				{
					// Tooltip
					if (mouseNear && Mouse.IsOver(button))
					{
						string tooltip;
						if (positiveDirection == TransferablePositiveCountDirection.Source)
						{
							if (Event.current.shift || TradeSession.giftMode || canBuyAny == false)
								tooltip = BuyAll;
							else
								tooltip = BuyAffordable;

							if (canBuyAny == false)
								tooltip += "\n" + Unaffordable;
						}
						else
						{
							if (Event.current.shift || TradeSession.giftMode || canSellAny == false)
								tooltip = SellAll;
							else
								tooltip = SellAffordable;

							if (canSellAny == false)
								tooltip += "\n" + Unaffordable;
						}
						TooltipHandler.TipRegion(button, tooltip);
					}

					// Draw left double arrow
					GUI.color = buttonColor;
					if (Widgets.ButtonText(button, "<<") || DragSelect.IsPainting(button, DragSelect.PaintingDirection.Left))
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
					GUI.color = normal;
					button.x += button.width + gap;
				}
				else
				{
					button.width += gap + baseButtonRect.width;
				}

				// Tooltip
				if (mouseNear && Mouse.IsOver(button))
				{
					string tooltip;
					if (positiveDirection == TransferablePositiveCountDirection.Source)
					{
						tooltip = BuyKey.Translate(Math.Abs(adjustAmount));

						if (canBuyAny == false)
							tooltip += "\n" + Unaffordable;
					}
					else
					{
						tooltip = SellKey.Translate(Math.Abs(adjustAmount));

						if (canSellAny == false)
							tooltip += "\n" + Unaffordable;
					}
					TooltipHandler.TipRegion(button, tooltip);
				}

				// Draw left arrow
				GUI.color = buttonColor;
				if (Widgets.ButtonText(button, "<") || (!largeRange && DragSelect.IsPainting(button, DragSelect.PaintingDirection.Left)))
				{
					row.AdjustBy(adjustAmount);
					refresh = true;
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				GUI.color = normal;

				baseButtonRect.x = button.xMax + gap;
			}
			else
			{
				baseButtonRect.x += baseButtonRect.width * 2 + gap * 2;
			}

			// Draw reset
			if (currentAmountToTransfer != 0)
			{
				if (Widgets.ButtonText(baseButtonRect, "0") || DragSelect.IsPainting(baseButtonRect, DragSelect.PaintingDirection.Middle))
				{
					row.AdjustTo(0);
					refresh = true;
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
			}
			baseButtonRect.x += baseButtonRect.width + gap;

			// Draw right arrows
			if (CanAdjustBy(-adjustAmount, currentAmountToTransfer, cached.MinimumQuantity, cached.MaximumQuantity))
			{
				buttonColor = normal;
				if ((canSellAny == false && positiveDirection == TransferablePositiveCountDirection.Source) ||
					(canBuyAny == false && positiveDirection == TransferablePositiveCountDirection.Destination))
					buttonColor = _outOfFundsColor;

				if (largeRange == false)
					baseButtonRect.width = baseButtonRect.width * 2 + gap;

				// Tooltip
				if (mouseNear && Mouse.IsOver(baseButtonRect))
				{
					string tooltip;
					if (positiveDirection == TransferablePositiveCountDirection.Source)
					{
						tooltip = SellKey.Translate(Math.Abs(adjustAmount));

						if (canSellAny == false)
							tooltip += "\n" + Unaffordable;
					}
					else
					{
						tooltip = BuyKey.Translate(Math.Abs(adjustAmount));

						if (canBuyAny == false)
							tooltip += "\n" + Unaffordable;
					}
					TooltipHandler.TipRegion(baseButtonRect, tooltip);
				}


				GUI.color = buttonColor;
				if (Widgets.ButtonText(baseButtonRect, ">") || (!largeRange && DragSelect.IsPainting(baseButtonRect, DragSelect.PaintingDirection.Right)))
				{
					row.AdjustBy(-adjustAmount);
					refresh = true;
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();

				}
				GUI.color = normal;

				baseButtonRect.x += baseButtonRect.width + gap;

				if (largeRange)
				{
					// Tooltip
					if (mouseNear && Mouse.IsOver(baseButtonRect))
					{
						string tooltip;
						if (positiveDirection == TransferablePositiveCountDirection.Source)
						{
							if (Event.current.shift || TradeSession.giftMode || canSellAny == false)
								tooltip = SellAll;
							else
								tooltip = SellAffordable;

							if (canSellAny == false)
								tooltip += "\n" + Unaffordable;
						}
						else
						{
							if (Event.current.shift || TradeSession.giftMode || canBuyAny == false)
								tooltip = BuyAll;
							else
								tooltip = BuyAffordable;

							if (canBuyAny == false)
								tooltip += "\n" + Unaffordable;
						}
						TooltipHandler.TipRegion(baseButtonRect, tooltip);
					}

					// Draw right double arrow
					GUI.color = buttonColor;
					if (Widgets.ButtonText(baseButtonRect, ">>") || DragSelect.IsPainting(baseButtonRect, DragSelect.PaintingDirection.Right))
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
					GUI.color = normal;
				}
			}

			if (refresh)
				_invalidatePostDeal = true;
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

			int traderCanBuy = -MaxAmount(row, TradeAction.PlayerSells);
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
			int traderCanBuy = -MaxAmount(row, TradeAction.PlayerSells);
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
			int currency = 0;
			if (action == TradeAction.PlayerBuys)
				currency = _colonyPostDeal;
			else
				currency = _traderPostDeal;

			float price = row.GetPriceFor(action);
			int priceOffset = (int)(price * -row.CountToTransfer);

			if (priceOffset > 0)
				currency += priceOffset;
			else
				currency -= priceOffset;

			return (int)(currency / price);
		}
	}
}
