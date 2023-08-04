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
	internal static class ColumnQuantity
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor)
		{
			Widgets.Label(rect, row.CountHeldBy(transactor).ToStringCached());
		}
	}
}
