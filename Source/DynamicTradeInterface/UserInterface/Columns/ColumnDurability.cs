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
	internal static class ColumnDurability
	{
		private static Dictionary<Tradeable, Cache> _traderRows = new Dictionary<Tradeable, Cache>();
		private static Dictionary<Tradeable, Cache> _colonyRows = new Dictionary<Tradeable, Cache>();

		private class Cache
		{
			public string? DurabilityString;
			public float Durability = 101;
			public Color Color;
		}


		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			Dictionary<Tradeable, Cache> cache = transactor == Transactor.Trader ? _traderRows : _colonyRows;
			bool stackDurability = Mod.DynamicTradeInterfaceMod.Settings.StackDurability;
			List<Thing> things;
			foreach (Tradeable row in rows)
			{
				if (row.IsThing == false)
					continue;

				Cache cacheRow = new Cache();
				if (transactor == Transactor.Trader)
					things = row.thingsTrader;
				else
					things = row.thingsColony;

				int hitpoints = 0;
				int maxHitpoints = 0;


				int count = things.Count;
				if (count == 0 || things[0].def?.useHitPoints == false)
					continue;

				if (stackDurability == false)
				{
					// Only show durability for unstacked items
					if (things.Count == 1 && things[0].stackCount == 1)
					{
						Thing thing = things[0];
						hitpoints = thing.HitPoints;
						maxHitpoints = thing.MaxHitPoints;
					}
				}
				else
				{
					for (int i = 0; i < things.Count; i++)
					{
						Thing thing = things[i];
						hitpoints += thing.HitPoints;
						maxHitpoints += thing.MaxHitPoints;
					}
				}

				if (hitpoints > 0 && hitpoints != maxHitpoints)
				{
					float hitpointsRatio = (float)hitpoints / maxHitpoints;
					cacheRow.Color = new Color(1, hitpointsRatio, hitpointsRatio);
					cacheRow.Durability = hitpointsRatio * 100;
					cacheRow.DurabilityString = cacheRow.Durability.ToString("N0") + "%";
				}
				cache[row] = cacheRow;
			}

		}

		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_traderRows.Clear();
			_colonyRows.Clear();
		}


		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (row.IsThing)
			{
				Dictionary<Tradeable, Cache> cachedRow = transactor == Transactor.Trader ? _traderRows : _colonyRows;
				if (cachedRow.TryGetValue(row, out Cache cacheRow))
				{
					GUI.color = cacheRow.Color;
					Widgets.Label(rect, cacheRow.DurabilityString);
					GUI.color = Color.white;
				}
			}
		}

		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) =>
			{
				if (row.IsThing)
				{
					Dictionary<Tradeable, Cache> cachedRow = transactor == Transactor.Trader ? _traderRows : _colonyRows;
					if (cachedRow.TryGetValue(row, out Cache cacheRow))
						return cacheRow.Durability;
				}
				return 101f;
			};
		}
	}
}
