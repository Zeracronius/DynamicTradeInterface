using DynamicTradeInterface.InterfaceComponents;
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
	internal static class ColumnQuality
	{
		struct Cache
		{
			public string Label;
			public string Tooltip;
			public Color? Color;
			public byte Value;
		}

		private static Dictionary<Tradeable, Cache> _rowCache = new Dictionary<Tradeable, Cache>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor _)
		{
			foreach (var row in rows)
			{
				if (_rowCache.ContainsKey(row))
					continue;

				Thing thing = row.AnyThing;
				if (thing != null)
				{
					CompQuality? quality = thing.TryGetComp<CompQuality>();
					if (quality != null)
					{
						Color? color = null;
						if (QualityColors.Active)
						{
							Color buffer = Color.white;
							QualityColors.GetColor(row, default, ref buffer);
							color = buffer;
						}

						string caption = quality.Quality.GetLabel().CapitalizeFirst();
						_rowCache[row] = new Cache
						{
							Label = caption,
							Tooltip = caption,
							Value = (byte)quality.Quality,
							Color = color,
						};
					}
				}
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_rowCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_rowCache.TryGetValue(row, out Cache quality))
			{
				Color color = GUI.color;
				if (quality.Color.HasValue)
					GUI.color = quality.Color.Value;

				Widgets.Label(rect, quality.Label);
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegion(rect, quality.Tooltip);
				}
				GUI.color = color;
			}
		}

		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) =>
			{
				if (_rowCache.TryGetValue(row, out Cache cache))
					return (int)cache.Value;
				else
					return -1;
			};
		}

		public static string SearchValue(Tradeable row, Transactor transactor)
		{
			if (_rowCache.TryGetValue(row, out Cache cache))
				return cache.Label;

			return string.Empty;
		}
	}
}
