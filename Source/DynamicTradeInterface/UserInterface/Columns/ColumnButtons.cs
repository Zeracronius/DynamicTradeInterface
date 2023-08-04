using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal static class ColumnButtons
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor)
		{
			if (row.Interactive == false)
				return;

			int baseCount = row.PositiveCountDirection == TransferablePositiveCountDirection.Source ? 1 : -1;
			int adjustMultiplier = GenUI.CurrentAdjustmentMultiplier();
			int adjustAmount = baseCount * adjustMultiplier;
			bool largeRange = row.GetRange() > 1;

			float gap = 2;
			// << < 0 > >>
			float width = rect.width / 5 - gap;
			Rect baseButtonRect = new Rect(rect.x, rect.y, width, rect.height);

			// Draw left arrows
			Rect button = new Rect(baseButtonRect);
			if (row.CanAdjustBy(adjustAmount).Accepted)
			{
				if (largeRange)
				{
					if (Widgets.ButtonText(button, "<<"))
					{
						if (baseCount == 1)
						{
							row.AdjustTo(row.GetMaximumToTransfer());
						}
						else
						{
							row.AdjustTo(row.GetMinimumToTransfer());
						}
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
					button.x += button.width + gap;
				}
				else
				{
					button.width += gap + baseButtonRect.width;
				}

				if (Widgets.ButtonText(button, "<"))
				{
					row.AdjustBy(adjustAmount);
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				baseButtonRect.x = button.xMax + gap;
			}
			else
			{
				baseButtonRect.x += baseButtonRect.width * 2 + gap * 2;
			}

			// Draw reset
			if (Widgets.ButtonText(baseButtonRect, "0"))
			{
				row.AdjustTo(0);
			}
			baseButtonRect.x += baseButtonRect.width + gap;

			// Draw right arrows
			if (row.CanAdjustBy(-adjustAmount).Accepted)
			{
				if (largeRange == false)
					baseButtonRect.width = baseButtonRect.width * 2 + gap;


				if (Widgets.ButtonText(baseButtonRect, ">"))
				{
					row.AdjustBy(-adjustAmount);
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				baseButtonRect.x += baseButtonRect.width + gap;

				if (largeRange)
				{
					if (Widgets.ButtonText(baseButtonRect, ">>"))
					{
						if (baseCount == 1)
						{
							row.AdjustTo(row.GetMinimumToTransfer());
						}
						else
						{
							row.AdjustTo(row.GetMaximumToTransfer());
						}
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				}
			}
		}
	}
}
