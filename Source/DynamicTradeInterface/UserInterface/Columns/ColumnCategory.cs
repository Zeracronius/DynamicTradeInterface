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
	internal static class ColumnCategory
	{
		private static Dictionary<Tradeable, string> _editableCache = new Dictionary<Tradeable, string>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			foreach (var row in rows)
			{
				ThingDef? def = row.ThingDef;
				if (def != null)
					_editableCache[row] = def.FirstThingCategory.LabelCap;
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_editableCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_editableCache.TryGetValue(row, out string category))
			{
				Widgets.Label(rect, category);
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegion(rect, category);
				}
			}
		}

		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) =>
			{
				if (_editableCache.TryGetValue(row, out string category))
					return category;
				else
					return string.Empty;
			};
		}
	}
}
