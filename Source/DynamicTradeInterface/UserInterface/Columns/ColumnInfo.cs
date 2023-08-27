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
	internal static class ColumnInfo
	{
		private static Dictionary<Tradeable, (Thing, bool)> _editableCache = new Dictionary<Tradeable, (Thing, bool)>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			foreach (var row in rows)
			{
				if (row.IsThing)
				{
					Thing thing = row.AnyThing;
					if (thing != null)
					_editableCache[row] = (thing, Mod.DynamicTradeInterfaceMod.Settings.GhostButtons);
				}
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_editableCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_editableCache.TryGetValue(row, out (Thing, bool) cached) == false)
				return;

			if (cached.Item2 == true && Mouse.IsOver(rect.ExpandedBy(rect.width * 2, rect.height * 2)) == false)
				return;

			Widgets.InfoCardButton(rect.x, rect.y, cached.Item1);
		}
	}
}
