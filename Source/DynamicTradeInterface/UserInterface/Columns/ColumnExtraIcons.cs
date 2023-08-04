using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal class ColumnExtraIcons
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor)
		{
			float width = 0f;
			TransferableUIUtility.DoExtraIcons(row, rect, ref width);
		}
	}
}
