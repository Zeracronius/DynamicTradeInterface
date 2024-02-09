﻿using DynamicTradeInterface.Attributes;
using DynamicTradeInterface.UserInterface.Columns.ColumnCounterTypes;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal static class ColumnCounter
	{
		private class Cache
		{
			public bool WillTrade;
			public bool Interactive;
			public int MinimumQuantity;
			public int MaximumQuantity;
			public bool SellingToSlavery;
			public bool CanSellToSlavery;
			public string? Identity;
		}

		private static string? _dynamicTradeUnwilling;
		private static string? _positiveBuysNegativeSells;
		private static string? _negotiatorWillNotTradeSlavesTip;

		private static Dictionary<Tradeable, Cache> _editableCache = new Dictionary<Tradeable, Cache>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			foreach (var row in rows)
				_editableCache[row] = new Cache()
				{
					WillTrade = row.TraderWillTrade,
					Interactive = row.Interactive,
					MinimumQuantity = row.GetMinimumToTransfer(),
					MaximumQuantity = row.GetMaximumToTransfer(),
					SellingToSlavery = TransferableUIUtility.TradeIsPlayerSellingToSlavery(row, TradeSession.trader.Faction),
					CanSellToSlavery = new HistoryEvent(HistoryEventDefOf.SoldSlave, TradeSession.playerNegotiator.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo(),
					Identity = row.AnyThing.ToString(),
				};

			if (transactor == Transactor.Colony)
			{
				_dynamicTradeUnwilling = "DynamicTradeWindowUnwilling".Translate();
				_positiveBuysNegativeSells = "PositiveBuysNegativeSells".Translate();
				_negotiatorWillNotTradeSlavesTip = "NegotiatorWillNotTradeSlavesTip".Translate(TradeSession.playerNegotiator, TradeSession.playerNegotiator.Ideo.name);
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (transactor == Transactor.Colony)
			{
				_editableCache.Clear();
				_dynamicTradeUnwilling = null;
				_positiveBuysNegativeSells = null;
				_negotiatorWillNotTradeSlavesTip = null;
			}
		}


		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_editableCache.TryGetValue(row, out Cache cached) == false)
				return;

			if (cached.WillTrade == false)
			{
				DrawWillNotTradeText(rect, _dynamicTradeUnwilling);
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegionByKey(rect, "TraderWillNotTrade");
				}
				return;
			}
			
			if (ModsConfig.IdeologyActive && cached.SellingToSlavery && cached.CanSellToSlavery == false)
			{
				DrawWillNotTradeText(rect, _dynamicTradeUnwilling);
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
					TooltipHandler.TipRegion(rect, _negotiatorWillNotTradeSlavesTip);
				}
			}
			else
			{
				Rect rect2 = new Rect(rect.center.x - 45f, rect.center.y - 12.5f, 90f, 25f).Rounded();

				int countToTransfer = row.CountToTransfer;
				if (!cached.Interactive)
				{
					GUI.color = countToTransfer == 0 ? TransferableUIUtility.ZeroCountColor : Color.white;
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect2, countToTransfer.ToStringCached());
				}
				else
				{
					Rect rect3 = rect2.ContractedBy(2f);
					rect3.xMax -= 15f;
					rect3.xMin += 16f;
					int val = countToTransfer;

					int minTransfer, maxTransfer;

					minTransfer = cached.MinimumQuantity;
					maxTransfer = cached.MaximumQuantity;

					string buffer = row.EditBuffer;
					Components.TextFieldNumeric(rect3, cached.Identity + (byte)transactor, ref val, ref buffer, minTransfer, maxTransfer);

					if (val != countToTransfer)
					{
						row.AdjustTo(val);
						countToTransfer = row.CountToTransfer;
						refresh = true;
					}

					row.EditBuffer = buffer;
				}
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;

				if (Mouse.IsOver(rect))
					TooltipHandler.TipRegion(rect, _positiveBuysNegativeSells);

				if (countToTransfer != 0)
				{
					TransferablePositiveCountDirection positiveDirection = row.PositiveCountDirection;
					Texture2D arrowIcon = Mod.Textures.TradeArrow;
					Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(arrowIcon.width / 2), rect2.y + rect2.height / 2f - (float)(arrowIcon.height / 2), arrowIcon.width, arrowIcon.height);
				
					if ((positiveDirection == TransferablePositiveCountDirection.Source && countToTransfer > 0) || (positiveDirection == TransferablePositiveCountDirection.Destination && countToTransfer < 0))
					{
						position.x += position.width;
						position.width *= -1f;
					}
					GUI.DrawTexture(position, arrowIcon);
				}
			}
		}

		private static void DrawWillNotTradeText(Rect rect, string? text)
		{
			rect.height += 4f;
			rect = rect.Rounded();
			GUI.color = TradeUI.NoTradeColor;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
		}


		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) => new CounterComparable(row.CountToTransfer);
		}
	}
}
