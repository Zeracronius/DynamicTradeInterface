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
	internal class ColumnPricePerWeight
	{
		private static Dictionary<Tradeable, string> _colonyCache = new Dictionary<Tradeable, string>();
		private static Dictionary<Tradeable, string> _traderCache = new Dictionary<Tradeable, string>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			Dictionary<Tradeable, string> cache;
			if (transactor == Transactor.Colony)
				cache = _colonyCache;
			else
				cache = _traderCache;


			cache.Clear();
			foreach (var row in rows)
			{
				if (!row.TraderWillTrade)
					continue;

				if (row.IsThing == false)
					continue;

				Thing thing = row.AnyThing;
				if (thing == null)
					continue;

				TradeAction action = transactor == Transactor.Colony ? TradeAction.PlayerSells : TradeAction.PlayerBuys;
				float price = row.GetPriceFor(action) / thing.def.BaseMass;
				cache[row] = TradeSession.TradeCurrency == TradeCurrency.Silver ? price.ToStringMoney() : price.ToString("N3");
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_colonyCache.Clear();
			_traderCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			Dictionary<Tradeable, string> cache;
			if (transactor == Transactor.Colony)
				cache = _colonyCache;
			else
				cache = _traderCache;

			if (cache.TryGetValue(row, out string label))
			{
				Widgets.Label(rect, label);
			}
		}

		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			TradeAction action = transactor == Transactor.Colony ? TradeAction.PlayerSells : TradeAction.PlayerBuys;
			return (Tradeable row) =>
			{
				if (row.IsThing)
				{
					Thing thing = row.AnyThing;
					if (thing != null)
						return row.GetPriceFor(action) / thing.def.BaseMass;
				}
				return 0;
			};
		}
	}
}
