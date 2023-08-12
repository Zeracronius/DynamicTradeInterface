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
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (!row.TraderWillTrade)
				return;

			if (row.IsThing == false)
				return;

			Thing thing = row.AnyThing;
			if (thing == null)
				return;

			TradeAction action = transactor == Transactor.Colony ? TradeAction.PlayerSells : TradeAction.PlayerBuys;
			float price = row.GetPriceFor(action) / thing.def.BaseMass;
			string label = TradeSession.TradeCurrency == TradeCurrency.Silver ? price.ToStringMoney() : price.ToString("N3");
			Widgets.Label(rect, label);
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
