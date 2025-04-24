using DynamicTradeInterface.Defs;
using DynamicTradeInterface.InterfaceComponents;
using DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal class ColumnExtraIcons
	{
		private static Dictionary<Tradeable, List<(Texture, string?, Color?)>> _rowCache = new Dictionary<Tradeable, List<(Texture, string?, Color?)>>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackDrawable.PostOpen(rows, transactor);

			foreach (var row in rows)
			{
				if (!_rowCache.ContainsKey(row))
				{
					List<(Texture, string?, Color?)> icons = new List<(Texture, string?, Color?)>();
					foreach (MoreIconsDef iconDef in DefDatabase<MoreIconsDef>.AllDefsListForReading)
					{
						foreach (var item in iconDef.GetIcons(row))
						{
							icons.Add(item);
						}
					}
					_rowCache[row] = icons;
				}
			}
		}

		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackDrawable.PostClosed(rows, transactor);
			_rowCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_rowCache.TryGetValue(row, out var cache))
			{
				Rect iconBoundary = new Rect(rect.xMax - rect.height, rect.y, rect.height, rect.height);
				foreach ((Texture, string?, Color?) drawable in cache)
				{
					Rect iconRect = iconBoundary.ContractedBy(1);
					GUI.DrawTexture(iconRect, drawable.Item1, ScaleMode.ScaleToFit, true, 1, color: drawable.Item3 ?? Color.white, 0, 0);
					if (Mouse.IsOver(iconRect))
					{
						Widgets.DrawHighlight(iconRect);
						TooltipHandler.TipRegion(iconRect, drawable.Item2);
					}
					iconBoundary.x -= iconBoundary.width;
				}
			}
		}


		public static string SearchValue(Tradeable row, Transactor transactor)
		{
			if (_rowCache.TryGetValue(row, out var cache))
			{
				return string.Join(" ", DefDatabase<MoreIconsDef>.AllDefsListForReading.Select(x => x.GetSearchString(row)));
			}
			return string.Empty;
		}
	}
}
