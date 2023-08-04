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
	internal static class ColumnPrice
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor)
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

			Color color = Color.white;
			TradeAction action = TradeAction.None;
			if (transactor == Transactor.Trader)
			{
				action = TradeAction.PlayerBuys;
				switch (row.PriceTypeFor(action))
				{
					case PriceType.VeryCheap:
						color = new Color(0f, 1f, 0f);
						break;
					case PriceType.Cheap:
						color = new Color(0.5f, 1f, 0.5f);
						break;
					case PriceType.Normal:
						color = Color.white;
						break;
					case PriceType.Expensive:
						color = new Color(1f, 0.5f, 0.5f);
						break;
					case PriceType.Exorbitant:
						color = new Color(1f, 0f, 0f);
						break;
				}
			}
			else
			{
				action = TradeAction.PlayerSells;
				switch (row.PriceTypeFor(action))
				{
					case PriceType.VeryCheap:
						color = new Color(1f, 0f, 0f);
						break;
					case PriceType.Cheap:
						color = new Color(1f, 0.5f, 0.5f);
						break;
					case PriceType.Normal:
						color = Color.white;
						break;
					case PriceType.Expensive:
						color = new Color(0.5f, 1f, 0.5f);
						break;
					case PriceType.Exorbitant:
						color = new Color(0f, 1f, 0f);
						break;
				}
			}




			float priceFor = row.GetPriceFor(TradeAction.PlayerSells);

			string label = TradeSession.TradeCurrency == TradeCurrency.Silver ? priceFor.ToStringMoney() : priceFor.ToString();
			//Rect rect2 = new Rect(rect);
			//rect2.xMax -= 5f;
			//rect2.xMin += 5f;

			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, new TipSignal(() => row.GetPriceTooltip(action), row.GetHashCode() * 297));
			}

			GUI.color = color;
			Widgets.Label(rect, label);
			GUI.color = Color.white;
		}
	}
}
