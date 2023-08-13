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
	internal class ColumnExtraIcons
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			float width = rect.x;
			TransferableUIUtility.DoExtraIcons(row, rect, ref width);

			if (ModsConfig.IdeologyActive)
			{
				if (row.IsThing)
				{
					Thing thing = row.AnyThing;
					if (thing is Pawn pawn)
					{
						XenotypeDef? xeno = pawn.genes?.Xenotype;
						if (xeno != null && xeno != XenotypeDefOf.Baseliner)
						{
							Rect xenoIconRect = new Rect(width, rect.y, 24, 24);
							Widgets.DrawTextureFitted(xenoIconRect, xeno.Icon, 1);
							if (Mouse.IsOver(xenoIconRect))
								TooltipHandler.TipRegion(xenoIconRect, xeno.LabelCap);
						}
					}
				}

				//TODO Add icon for marking genepack as known or not.
			}
		}
	}
}
