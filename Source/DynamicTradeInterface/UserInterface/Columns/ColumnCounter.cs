using DynamicTradeInterface.Attributes;
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
	[HotSwappable]
	internal static class ColumnCounter
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (!row.TraderWillTrade)
			{
				DrawWillNotTradeText(rect, "TraderWillNotTrade".Translate());
			}
			else if (ModsConfig.IdeologyActive && TransferableUIUtility.TradeIsPlayerSellingToSlavery(row, TradeSession.trader.Faction) && !new HistoryEvent(HistoryEventDefOf.SoldSlave, TradeSession.playerNegotiator.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
			{
				DrawWillNotTradeText(rect, "NegotiatorWillNotTradeSlaves".Translate(TradeSession.playerNegotiator));
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
					TooltipHandler.TipRegion(rect, "NegotiatorWillNotTradeSlavesTip".Translate(TradeSession.playerNegotiator, TradeSession.playerNegotiator.Ideo.name));
				}
			}
			else
			{
				Rect rect2 = new Rect(rect.center.x - 45f, rect.center.y - 12.5f, 90f, 25f).Rounded();

				bool shouldFlash = false;
				if (Dialog_Trade.lastCurrencyFlashTime > 0)
				{
					shouldFlash = Time.time - Dialog_Trade.lastCurrencyFlashTime < 1f;
					if (shouldFlash && row.IsCurrency)
					{
						GUI.DrawTexture(rect2, TransferableUIUtility.FlashTex);
					}
				}

				int countToTransfer = row.CountToTransfer;

				TransferablePositiveCountDirection positiveDirection = row.PositiveCountDirection;

				if (!row.Interactive)
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

					minTransfer = row.GetMinimumToTransfer();
					maxTransfer = row.GetMaximumToTransfer();

					string buffer = val.ToStringCached();
					Widgets.TextFieldNumeric(rect3, ref val, ref buffer, minTransfer, maxTransfer);

					if (val != countToTransfer)
					{
						row.AdjustTo(val);
						refresh = true;
					}
				}
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;

				if (countToTransfer != 0)
				{
					Texture2D tradeArrow = Mod.Textures.TradeArrow;
					Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(tradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(tradeArrow.height / 2), tradeArrow.width, tradeArrow.height);
					
					if ((positiveDirection == TransferablePositiveCountDirection.Source && countToTransfer > 0) || (positiveDirection == TransferablePositiveCountDirection.Destination && countToTransfer < 0))
					{
						position.x += position.width;
						position.width *= -1f;
					}
					GUI.DrawTexture(position, tradeArrow);
				}


				if (shouldFlash == false)
					Dialog_Trade.lastCurrencyFlashTime = 0;
			}
		}

		private static void DrawWillNotTradeText(Rect rect, string text)
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
	}
}
