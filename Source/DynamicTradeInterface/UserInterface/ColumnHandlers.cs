using DynamicTradeInterface.Attributes;
using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;

namespace DynamicTradeInterface.UserInterface
{
	[HotSwappable]
	static internal class ColumnHandlers
	{
		public static void DrawIcon(ref Rect rect, Tradeable row, TradeAction action)
		{
			if (row.IsThing)
			{
				Thing thing = row.AnyThing;
				if (thing != null)
					Widgets.ThingIcon(rect, thing);
			}
			else
			{
				row.DrawIcon(rect);
			}
		}

		public static void DrawInfo(ref Rect rect, Tradeable row, TradeAction action)
		{
			if (row.IsThing)
			{
				Thing thing = row.AnyThing;
				if (thing != null)
					Widgets.InfoCardButton(rect.x, rect.y, thing);
			}
		}

		public static void DrawCaption(ref Rect rect, Tradeable row, TradeAction action)
		{
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = row.TraderWillTrade ? Color.white : TradeUI.NoTradeColor;
			Widgets.Label(rect, row.LabelCap);
			GUI.color = Color.white;

			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, () =>
				{
					Thing thing = row.AnyThing;
					if (thing != null)
					{
						string tipDescription = row.TipDescription;
						if (String.IsNullOrWhiteSpace(tipDescription) == false)
						{
							string text = row.LabelCap;
							text = text + ": " + tipDescription + TransferableUIUtility.ContentSourceDescription(thing);
							return text;
						}
					}
					return "";
				}, row.GetHashCode());
			}

			Text.Anchor = TextAnchor.UpperLeft;
		}

		public static void DrawExtraIcons(ref Rect rect, Tradeable row, TradeAction action)
		{
			float width = 0f;
			TransferableUIUtility.DoExtraIcons(row, rect, ref width);
		}


		public static void DrawPrice(ref Rect rect, Tradeable row, TradeAction action)
		{
			if (row.IsCurrency || !row.TraderWillTrade)
			{
				return;
			}
			rect = rect.Rounded();
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, new TipSignal(() => row.GetPriceTooltip(action), row.GetHashCode() * 297));
			}
			if (action == TradeAction.PlayerBuys)
			{
				switch (row.PriceTypeFor(action))
				{
					case PriceType.VeryCheap:
						GUI.color = new Color(0f, 1f, 0f);
						break;
					case PriceType.Cheap:
						GUI.color = new Color(0.5f, 1f, 0.5f);
						break;
					case PriceType.Normal:
						GUI.color = Color.white;
						break;
					case PriceType.Expensive:
						GUI.color = new Color(1f, 0.5f, 0.5f);
						break;
					case PriceType.Exorbitant:
						GUI.color = new Color(1f, 0f, 0f);
						break;
				}
			}
			else
			{
				switch (row.PriceTypeFor(action))
				{
					case PriceType.VeryCheap:
						GUI.color = new Color(1f, 0f, 0f);
						break;
					case PriceType.Cheap:
						GUI.color = new Color(1f, 0.5f, 0.5f);
						break;
					case PriceType.Normal:
						GUI.color = Color.white;
						break;
					case PriceType.Expensive:
						GUI.color = new Color(0.5f, 1f, 0.5f);
						break;
					case PriceType.Exorbitant:
						GUI.color = new Color(0f, 1f, 0f);
						break;
				}
			}
			float priceFor = row.GetPriceFor(action);
			string label = TradeSession.TradeCurrency == TradeCurrency.Silver ? priceFor.ToStringMoney() : priceFor.ToString();
			Rect rect2 = new Rect(rect);
			rect2.xMax -= 5f;
			rect2.xMin += 5f;
			//if (Text.Anchor == TextAnchor.MiddleLeft)
			//{
			//	rect2.xMax += 300f;
			//}
			//if (Text.Anchor == TextAnchor.MiddleRight)
			//{
			//	rect2.xMin -= 300f;
			//}
			Widgets.Label(rect2, label);
			GUI.color = Color.white;
		}

		public static void DrawCounter(ref Rect rect, Tradeable row, TradeAction action)
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

				if (!row.Interactive)
				{
						GUI.color = ((row.CountToTransfer == 0) ? TransferableUIUtility.ZeroCountColor : Color.white);
						Text.Anchor = TextAnchor.MiddleCenter;
						Widgets.Label(rect2, row.CountToTransfer.ToStringCached());
				}
				else
				{
					Rect rect3 = rect2.ContractedBy(2f);
					rect3.xMax -= 15f;
					rect3.xMin += 16f;
					int val = row.CountToTransfer;
					string buffer = row.EditBuffer;
					Widgets.TextFieldNumeric(rect3, ref val, ref buffer, row.GetMinimumToTransfer(), row.GetMaximumToTransfer());
					row.AdjustTo(val);
					row.EditBuffer = buffer;
				}
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;

				if (row.CountToTransfer != 0)
				{
					Texture2D tradeArrow = Mod.Textures.TradeArrow;
					Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(tradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(tradeArrow.height / 2), tradeArrow.width, tradeArrow.height);
					TransferablePositiveCountDirection positiveCountDirection2 = row.PositiveCountDirection;
					if ((positiveCountDirection2 == TransferablePositiveCountDirection.Source && row.CountToTransfer > 0) || (positiveCountDirection2 == TransferablePositiveCountDirection.Destination && row.CountToTransfer < 0))
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

		public static void DrawButtons(ref Rect rect, Tradeable row, TradeAction action)
		{
			if (row.Interactive == false)
				return;

			int baseCount = row.PositiveCountDirection == TransferablePositiveCountDirection.Source ? 1 : -1;
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
						if (baseCount == 1)
						{
							row.AdjustTo(row.GetMaximumToTransfer());
						}
						else
						{
							row.AdjustTo(row.GetMinimumToTransfer());
						}
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
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				baseButtonRect.x += baseButtonRect.width + gap;

				if (largeRange)
				{
					if (Widgets.ButtonText(baseButtonRect, ">>"))
					{
						if (baseCount == 1)
						{
							row.AdjustTo(row.GetMinimumToTransfer());
						}
						else
						{
							row.AdjustTo(row.GetMaximumToTransfer());
						}
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				}
			}
		}







		private static void Invalidate()
		{

		}

	}
}
