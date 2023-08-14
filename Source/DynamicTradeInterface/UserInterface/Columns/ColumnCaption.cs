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
	internal static class ColumnCaption
	{
		private static Dictionary<Tradeable, (string, Color)> _labelCache = new Dictionary<Tradeable, (string, Color)>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			foreach (var row in rows)
				_labelCache[row] = (row.LabelCap, row.TraderWillTrade ? Color.white : TradeUI.NoTradeColor);
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_labelCache.Clear();
		}


		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_labelCache.TryGetValue(row, out (string, Color) cached) == false)
				return;

			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = cached.Item2;
			Widgets.Label(rect, cached.Item1);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;

			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, () =>
				{
					Thing thing = row.AnyThing;
					if (thing != null)
					{
						string tipDescription = row.TipDescription;
						if (string.IsNullOrWhiteSpace(tipDescription) == false)
						{
							return $"{row.LabelCap}: {tipDescription}{TransferableUIUtility.ContentSourceDescription(thing)}";
						}
					}
					return "";
				}, row.GetHashCode());
			}
		}


		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) => _labelCache[row];
		}
	}
}
