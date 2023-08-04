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
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor)
		{
			if (row.IsThing)
			{
				Thing thing = row.AnyThing;
				if (thing != null)
					Widgets.InfoCardButton(rect.x, rect.y, thing);
			}
		}
	}
}
