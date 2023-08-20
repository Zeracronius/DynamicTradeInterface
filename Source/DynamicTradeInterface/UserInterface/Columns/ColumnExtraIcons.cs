using DynamicTradeInterface.InterfaceComponents;
using DynamicTradeInterface.Mod;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal static class EnumerableExt
	{
		public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
		{
			foreach (IEnumerable<T> outer in source)
				foreach (T inner in outer)
					yield return inner;
		}

		public static IEnumerable<TResult> SelectNotNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult?> selector)
		{
			foreach (TSource elem in source)
			{
				TResult? result = selector(elem);
				if (result != null)
					yield return result;
			}
		}
	}

	internal class ColumnExtraIcons
	{
		private abstract class ITradeable
		{
			public abstract void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh);
		}

		private class PawnTradeable : ITradeable
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
			private bool _playerFaction;
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
				_playerFaction = pawn.Faction?.IsPlayer ?? false;
				_traderHomeFaction = pawn.HomeFaction == TradeSession.trader?.Faction;
			}

			public override void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
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

		private class GenepackTradeable : ITradeable
		{
			private static Dictionary<GeneDef, GeneAssistant.GeneType> _bankedGenes = new Dictionary<GeneDef, GeneAssistant.GeneType>();

			private GeneAssistant.GeneType _geneType = GeneAssistant.GeneType.None;

			private Texture? Icon => GeneAssistant.IconFor(_geneType);

			public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
			{
				if (transactor != Transactor.Colony)
					return;

				var genepacks = Find.CurrentMap
					?.listerBuildings
					.AllBuildingsColonistOfDef(ThingDefOf.GeneBank)
					.SelectNotNull((building) => building.TryGetComp<CompGenepackContainer>()?.ContainedGenepacks)
					.Flatten()
					.SelectNotNull((genepack) => genepack.GeneSet?.GenesListForReading);
				if (genepacks != null)
				{
					foreach (var genes in genepacks)
					{
						if (genes.Count == 1)
							_bankedGenes[genes[0]] = GeneAssistant.GeneType.Isolated;
						else
							foreach (var gene in genes)
								_bankedGenes.TryAdd(gene, GeneAssistant.GeneType.Mixed);
					}
				}
			}

			public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
			{
				if (transactor == Transactor.Colony)
					_bankedGenes.Clear();
			}

			public GenepackTradeable(Tradeable tradeable, Genepack genepack)
			{
				var types = genepack.GeneSet
					?.GenesListForReading
					.Select((gene) => _bankedGenes.TryGetValue(gene, GeneAssistant.GeneType.Missing))
					.Distinct()
					.ToList();
				if (types?.Count > 0)
				{
					if (types.Any((x) => x == GeneAssistant.GeneType.Missing))
						_geneType = GeneAssistant.GeneType.Missing;
					else if (types.All((x) => x == GeneAssistant.GeneType.Isolated))
						_geneType = GeneAssistant.GeneType.Isolated;
					else
						_geneType = GeneAssistant.GeneType.Mixed;
				}
			}

			public override void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
			{
				var icon = Icon;
				if (icon != null)
				{
					Rect iconRect = new Rect(rect.xMax - rect.height, rect.y, rect.height, rect.height).ContractedBy(1);
					GUI.DrawTexture(iconRect, icon);
				}
			}
		}

		private static Dictionary<Tradeable, ITradeable> _rowCache = new Dictionary<Tradeable, ITradeable>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackTradeable.PostOpen(rows, transactor);

			foreach (var row in rows)
			{
				if (!_rowCache.ContainsKey(row))
				{
					if (row.AnyThing is Pawn pawn)
						_rowCache[row] = new PawnTradeable(row, pawn);
					else if (row.AnyThing is Genepack genepack && GeneAssistant.Active)
						_rowCache[row] = new GenepackTradeable(row, genepack);
				}
			}
		}

		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (GeneAssistant.Active)
				GenepackTradeable.PostClosed(rows, transactor);
			_rowCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_rowCache.TryGetValue(row, out var cache))
				cache.Draw(ref rect, row, transactor, ref refresh);
		}
	}
}
