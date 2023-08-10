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
	internal class ColumnDurability
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (row.IsThing)
			{
				Thing thing = row.AnyThing;
				if (thing != null && thing.def?.useHitPoints == true)
				{
					float hitpointsRatio = (float)thing.MaxHitPoints / thing.HitPoints;

					GUI.color = new Color(1, hitpointsRatio, hitpointsRatio);
					Widgets.Label(rect, ((int)(hitpointsRatio * 100)).ToStringCached() + "%");
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
					Thing thing = row.AnyThing;
					if (thing != null && thing.def?.useHitPoints == true)
						return (float)thing.MaxHitPoints / thing.HitPoints * 100;
				}
				return 101;
			};
		}
	}
}
