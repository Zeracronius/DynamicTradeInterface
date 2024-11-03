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
	internal static class ColumnQuantity
	{
		private static Dictionary<Tradeable, (string, string?)> _cacheTrader = new Dictionary<Tradeable, (string, string?)>();
		private static Dictionary<Tradeable, (string, string?)> _cacheColony = new Dictionary<Tradeable, (string, string?)>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			Dictionary<Tradeable, (string, string?)> cache;
			switch (transactor)
			{
				case Transactor.Colony:
					cache = _cacheColony;
					break;

				case Transactor.Trader:
					cache = _cacheTrader;
					break;
				
				default:
					return;
			}

			cache.Clear();
			foreach (Tradeable row in rows)
			{
				int availableForTrading = row.CountHeldBy(transactor);

				if (transactor == Transactor.Colony && Mod.DynamicTradeInterfaceMod.Settings.ShowAvailableOnMap)
				{
					Thing thing = row.AnyThing;
					if (thing?.def != null)
					{
						Map? map = TradeSession.playerNegotiator?.Map;
						if (map != null && map.listerThings != null)
						{
							List<Thing> things = map.listerThings.ThingsOfDef(thing.def);

							int count = things?.Count ?? 0;
							if (count > 0)
							{
								int thingsOnMap = 0;
								for (int i = 0; i < count; i++)
									thingsOnMap += things![i].stackCount;

								// If more Things are available for trading than currently shown under Available, then show in parenthesis with tooltip.
								if (thingsOnMap != availableForTrading)
								{
									cache[row] = ($"{availableForTrading} ({thingsOnMap})", $"{availableForTrading} available for trading.{Environment.NewLine}{thingsOnMap} total on map.");
									continue;
								}
							}
						}
					}
				}

				// If execution gets this far, then default.
				cache[row] = ($"{availableForTrading}", null);
			}
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			Dictionary<Tradeable, (string, string?)> cache;
			switch (transactor)
			{
				case Transactor.Colony:
					cache = _cacheColony;
					break;

				case Transactor.Trader:
					cache = _cacheTrader;
					break;

				default:
					return;
			}


			float y = rect.y;
			if (cache.TryGetValue(row, out (string, string?) value))
				Widgets.Label(rect, ref y, value.Item1, tip: value.Item2);
		}

		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) => row.CountHeldBy(transactor);
		}
	}
}
