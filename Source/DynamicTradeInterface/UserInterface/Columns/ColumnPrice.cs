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
	internal static class ColumnPrice
	{

		private static Dictionary<Tradeable, (string, Color)> _colonyCache = new Dictionary<Tradeable, (string, Color)>();
		private static Dictionary<Tradeable, (string, Color)> _traderCache = new Dictionary<Tradeable, (string, Color)>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			Dictionary<Tradeable, (string, Color)> cache;
			if (transactor == Transactor.Colony)
				cache = _colonyCache;
			else
				cache = _traderCache;


			cache.Clear();

			foreach (var row in rows)
			{
				if (!row.TraderWillTrade)
					continue;

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

				float priceFor = row.GetPriceFor(action);
				string label = TradeSession.TradeCurrency == TradeCurrency.Silver ? priceFor.ToStringMoney() : priceFor.ToString();
				cache[row] = (label, color);
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_colonyCache.Clear();
			_traderCache.Clear();
		}


		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			Dictionary<Tradeable, (string, Color)> cache;
			if (transactor == Transactor.Colony)
				cache = _colonyCache;
			else
				cache = _traderCache;

			if (cache.TryGetValue(row, out (string, Color) cached))
			{
				GUI.color = cached.Item2;
				Widgets.Label(rect, cached.Item1);
				GUI.color = Color.white;

				if (Mouse.IsOver(rect))
				{
					TradeAction action;
					if (transactor == Transactor.Trader)
						action = TradeAction.PlayerBuys;
					else
						action = TradeAction.PlayerSells;

					TooltipHandler.TipRegion(rect, new TipSignal(() => row.GetPriceTooltip(action), row.GetHashCode() * 297));
				}
			}
		}

		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			TradeAction action = TradeAction.PlayerSells;
			if (transactor == Transactor.Trader)
				action = TradeAction.PlayerBuys;

			return (Tradeable row) => row.GetPriceFor(action);
		}
	}
}
