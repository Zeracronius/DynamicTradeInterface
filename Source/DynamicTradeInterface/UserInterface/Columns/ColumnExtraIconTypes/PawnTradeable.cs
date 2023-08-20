using DynamicTradeInterface.Mod;
using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes
{
	internal class PawnTradeable : ITradeable
	{
		private Pawn _pawn;
		private Intelligence _intelligence;
		private bool _rideable;
		private bool _bonded;
		private bool _pregnant;
		private bool _sick;
		private bool _isColonyMech;
		private Pawn _overseerPawn;
		private bool _captive;
		private bool _traderHomeFaction;

		public PawnTradeable(Tradeable tradeable, Pawn pawn)
		{
			string joinAsText = (pawn.guest?.joinStatus == JoinStatus.JoinAsColonist ? "JoinsAsColonist" : "JoinsAsSlave").Translate();
			_pawn = pawn;
			_intelligence = pawn.RaceProps.intelligence;
			_rideable = pawn.IsCaravanRideable();
			_bonded = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null;
			_pregnant = pawn.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true);
			_sick = pawn.health.hediffSet.AnyHediffMakesSickThought;
			_isColonyMech = pawn.IsColonyMech;
			_overseerPawn = pawn.GetOverseer();
			_captive = TransferableUIUtility.TransferableIsCaptive(tradeable);
			_traderHomeFaction = pawn.HomeFaction == TradeSession.trader?.Faction;
		}

		public void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			float curX = rect.xMax;

			if (_intelligence == Intelligence.Animal)
				DoAnimalIcons(ref rect, ref curX);

			if (ModsConfig.BiotechActive)
				DoBiotechIcons(ref rect, ref curX);

			if (ModsConfig.IdeologyActive)
				DoIdeologyIcons(ref rect, ref curX);
		}

		private void DoAnimalIcons(ref Rect rect, ref float curX)
		{
			Rect iconRect = new Rect(curX, rect.y, rect.height, rect.height).ContractedBy(1);
			iconRect.x -= iconRect.width;
			if (_rideable)
			{
				GUI.DrawTexture(iconRect, Textures.RideableIcon);
				if (Mouse.IsOver(iconRect))
					TooltipHandler.TipRegion(iconRect, CaravanRideableUtility.GetIconTooltipText(_pawn));

				iconRect.x -= iconRect.width;
			}

			if (_bonded)
			{
				TransferableUIUtility.DrawBondedIcon(_pawn, iconRect);
				iconRect.x -= iconRect.width;
			}

			if (_pregnant)
			{
				TransferableUIUtility.DrawPregnancyIcon(_pawn, iconRect);
				iconRect.x -= iconRect.width;
			}

			if (_sick)
			{
				if (Mouse.IsOver(iconRect))
				{
					IEnumerable<string> entries = _pawn.health.hediffSet.hediffs.Where(x => x.def.makesSickThought).Select(x => x.LabelCap);
					TooltipHandler.TipRegion(iconRect, "CaravanAnimalSick".Translate() + ":\n\n" + entries.ToLineList(" - "));
				}
				GUI.DrawTexture(iconRect, Textures.SickIcon);
				iconRect.x -= iconRect.width;
			}

			curX = iconRect.x;
		}

		private void DoBiotechIcons(ref Rect rect, ref float curX)
		{
			Rect iconRect = new Rect(curX, rect.y, rect.height, rect.height).ContractedBy(1);
			iconRect.x -= iconRect.width;

			if (_isColonyMech)
			{
				if (_overseerPawn != null)
				{
					GUI.DrawTexture(rect, PortraitsCache.Get(_overseerPawn, new Vector2(rect.width, rect.height), Rot4.South));
					if (Mouse.IsOver(rect))
					{
						Widgets.DrawHighlight(rect);
						TooltipHandler.TipRegion(rect, "MechOverseer".Translate(_overseerPawn));
					}
					iconRect.x -= iconRect.width;
				}
			}

			XenotypeDef? xeno = _pawn.genes?.Xenotype;
			if (xeno != null && xeno != XenotypeDefOf.Baseliner)
			{
				Widgets.DrawTextureFitted(iconRect, xeno.Icon, 1);
				if (Mouse.IsOver(iconRect))
					TooltipHandler.TipRegion(iconRect, xeno.LabelCap);

				iconRect.x -= iconRect.width;
			}

			curX = iconRect.x;
		}

		private void DoIdeologyIcons(ref Rect rect, ref float curX)
		{
			if (_pawn.guest == null)
				return;

			Rect iconRect = new Rect(curX, rect.y, rect.height, rect.height).ContractedBy(1);

			if (_captive)
			{
				if (_traderHomeFaction)
				{
					GUI.DrawTexture(iconRect, GuestUtility.RansomIcon);
					if (Mouse.IsOver(iconRect))
						TooltipHandler.TipRegion(rect, "SellingAsRansom".Translate());
				}
				else
				{
					GUI.DrawTexture(iconRect, GuestUtility.SlaveIcon);
					if (Mouse.IsOver(iconRect))
						TooltipHandler.TipRegion(iconRect, "SellingAsSlave".Translate());
				}
				iconRect.x -= iconRect.width;
			}
		}
	}
}
