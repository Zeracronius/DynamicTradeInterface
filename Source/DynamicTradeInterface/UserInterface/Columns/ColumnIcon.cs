﻿using RimWorld;
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
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (row.IsThing)
			{
				Thing thing = row.AnyThing;
				if (thing != null && thing.def != null)
				{
					Widgets.ThingIcon(rect, thing);
					if (Mouse.IsOver(rect))
					{
						//Widgets.DrawHighlight(rect);
						if (Widgets.ButtonInvisible(rect))
							Find.WindowStack.Add(new Dialog_InfoCard(thing));
					}
				}
			}
			else
			{
				row.DrawIcon(rect);
			}
		}
	}
}
