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
	internal static class ColumnIcon
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor)
		{
			if (row.IsThing)
			{
				Thing thing = row.AnyThing;
				if (thing != null)
					Widgets.ThingIcon(rect, thing);
			}
			else
			{
				row.DrawIcon(rect);
			}
		}
	}
}
