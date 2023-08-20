using DynamicTradeInterface.Mod;
using RimWorld;
using RimWorld.Planet;
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
		private struct Cache
		{
			public Pawn Pawn;
			public Intelligence Intelligence;
			public bool Rideable;
			public bool Bonded;
			public bool Pregnant;
			public bool Sick;
			public bool IsColonyMech;
			public Pawn OverseerPawn;
			public bool Captive;
			public bool PlayerFaction;
			public bool TraderHomeFaction;
		}

		private static Dictionary<Tradeable, Cache> _rowCache = new Dictionary<Tradeable, Cache>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			foreach (var row in rows)
			{
				if (row.AnyThing is Pawn pawn != true)
					continue;

				if (_rowCache.ContainsKey(row))
					continue;

				string joinAsText = (pawn.guest?.joinStatus == JoinStatus.JoinAsColonist ? "JoinsAsColonist" : "JoinsAsSlave").Translate();
				Cache cache = new Cache
				{
					Pawn = pawn,
					Intelligence = pawn.RaceProps.intelligence,
					Rideable = pawn.IsCaravanRideable(),
					Bonded = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null,
					Pregnant = pawn.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true),
					Sick = pawn.health.hediffSet.AnyHediffMakesSickThought,
					IsColonyMech = pawn.IsColonyMech,
					OverseerPawn = pawn.GetOverseer(),
					Captive = TransferableUIUtility.TransferableIsCaptive(row),
					PlayerFaction = pawn.Faction?.IsPlayer ?? false,
					TraderHomeFaction = pawn.HomeFaction == TradeSession.trader?.Faction,
				};

				_rowCache[row] = cache;
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_rowCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_rowCache.TryGetValue(row, out var cache) == false)
				return;
			
			float curX = rect.xMax;


			if (cache.Intelligence == Intelligence.Animal)
			{
				DoAnimalIcons(ref rect, ref curX, cache);
			}
			
			if (ModsConfig.BiotechActive)
			{
				DoBiotechIcons(ref rect, ref curX, cache);
			}

			if (ModsConfig.IdeologyActive)
			{
				DoIdeologyIcons(ref rect, ref curX, cache);
			}
		}
		private static void DoAnimalIcons(ref Rect rect, ref float curX, Cache cache)
		{
			Rect iconRect = new Rect(curX, rect.y, rect.height, rect.height).ContractedBy(1);
			iconRect.x -= iconRect.width;
			if (cache.Rideable)
			{
				GUI.DrawTexture(iconRect, Textures.RideableIcon);
				if (Mouse.IsOver(iconRect))
					TooltipHandler.TipRegion(iconRect, CaravanRideableUtility.GetIconTooltipText(cache.Pawn));

				iconRect.x -= iconRect.width;
			}

			if (cache.Bonded)
			{
				TransferableUIUtility.DrawBondedIcon(cache.Pawn, iconRect);
				iconRect.x -= iconRect.width;
			}

			if (cache.Pregnant)
			{
				TransferableUIUtility.DrawPregnancyIcon(cache.Pawn, iconRect);
				iconRect.x -= iconRect.width;
			}

			if (cache.Sick)
			{
				if (Mouse.IsOver(iconRect))
				{
					IEnumerable<string> entries = cache.Pawn.health.hediffSet.hediffs.Where(x => x.def.makesSickThought).Select(x => x.LabelCap);
					TooltipHandler.TipRegion(iconRect, "CaravanAnimalSick".Translate() + ":\n\n" + entries.ToLineList(" - "));
				}
				GUI.DrawTexture(iconRect, Textures.SickIcon);
				iconRect.x -= iconRect.width;
			}

			curX = iconRect.x;
		}

		private static void DoBiotechIcons(ref Rect rect, ref float curX, Cache cache)
		{
			Rect iconRect = new Rect(curX, rect.y, rect.height, rect.height).ContractedBy(1);
			iconRect.x -= iconRect.width;

			if (cache.IsColonyMech)
			{
				if (cache.OverseerPawn != null)
				{
					GUI.DrawTexture(rect, PortraitsCache.Get(cache.OverseerPawn, new Vector2(rect.width, rect.height), Rot4.South));
					if (Mouse.IsOver(rect))
					{
						Widgets.DrawHighlight(rect);
						TooltipHandler.TipRegion(rect, "MechOverseer".Translate(cache.OverseerPawn));
					}
					iconRect.x -= iconRect.width;
				}
			}

			XenotypeDef? xeno = cache.Pawn.genes?.Xenotype;
			if (xeno != null && xeno != XenotypeDefOf.Baseliner)
			{
				Widgets.DrawTextureFitted(iconRect, xeno.Icon, 1);
				if (Mouse.IsOver(iconRect))
					TooltipHandler.TipRegion(iconRect, xeno.LabelCap);

				iconRect.x -= iconRect.width;
			}

			curX = iconRect.x;
		}

		private static void DoIdeologyIcons(ref Rect rect, ref float curX, Cache cache)
		{
			if (cache.Pawn.guest == null)
				return;

			Rect iconRect = new Rect(curX, rect.y, rect.height, rect.height).ContractedBy(1);

			if (cache.Captive)
			{
				if (cache.TraderHomeFaction)
				{
					GUI.DrawTexture(iconRect, GuestUtility.RansomIcon);
					if (Mouse.IsOver(iconRect))
					{
						TooltipHandler.TipRegion(rect, "SellingAsRansom".Translate());
					}
				}
				else
				{
					GUI.DrawTexture(iconRect, GuestUtility.SlaveIcon);
					if (Mouse.IsOver(iconRect))
					{
						TooltipHandler.TipRegion(iconRect, "SellingAsSlave".Translate());
					}
				}
				iconRect.x -= iconRect.width;
			}
		}
	}
}
