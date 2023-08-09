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
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			Text.Anchor = TextAnchor.MiddleLeft;
			GUI.color = row.TraderWillTrade ? Color.white : TradeUI.NoTradeColor;
			Widgets.Label(rect, row.LabelCap);
			GUI.color = Color.white;

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

			Text.Anchor = TextAnchor.UpperLeft;
		}


		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) => row.LabelCap;
		}
	}
}
