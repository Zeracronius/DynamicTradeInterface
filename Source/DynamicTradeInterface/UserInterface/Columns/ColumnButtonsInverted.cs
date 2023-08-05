using DynamicTradeInterface.Attributes;
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
	[HotSwappable]
	internal static class ColumnButtonsInverted
	{
		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (row.Interactive == false)
				return;

			TransferablePositiveCountDirection positiveDirection = row.PositiveCountDirection;

			// If table is trader, flip direction.
			if (transactor == Transactor.Trader)
				positiveDirection = positiveDirection == TransferablePositiveCountDirection.Source ? TransferablePositiveCountDirection.Destination : TransferablePositiveCountDirection.Source;

			int baseCount = positiveDirection == TransferablePositiveCountDirection.Source ? 1 : -1;

			// Source is left.
			int minQuantity, maxQuantity;
			minQuantity = row.GetMinimumToTransfer();
			maxQuantity = row.GetMaximumToTransfer();


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
						if (positiveDirection == TransferablePositiveCountDirection.Source)
						{
							row.AdjustTo(maxQuantity);
						}
						else
						{
							row.AdjustTo(minQuantity);
						}
						refresh = true;
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
					refresh = true;
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
					refresh = true;
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();

				}
				baseButtonRect.x += baseButtonRect.width + gap;

				if (largeRange)
				{
					if (Widgets.ButtonText(baseButtonRect, ">>"))
					{
						if (positiveDirection == TransferablePositiveCountDirection.Source)
						{
							row.AdjustTo(minQuantity);
						}
						else
						{
							row.AdjustTo(maxQuantity);
						}
						refresh = true;
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
				}
			}
		}
	}
}
