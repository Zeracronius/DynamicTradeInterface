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
using DynamicTradeInterface.Attributes;
using HarmonyLib;

namespace DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes
{
	/// <summary>
	/// Proxy class used to call the protected methods on PawnColumnWorker_LifeStage.
	/// </summary>
	/// <seealso cref="RimWorld.PawnColumnWorker_LifeStage" />
	internal class PawnColumnLifeStageProxy : PawnColumnWorker_LifeStage
	{
		public Texture2D GetIcon(Pawn pawn)
		{
			return base.GetIconFor(pawn);
		}

		public string GetTooltip(Pawn pawn)
		{
			return base.GetIconTip(pawn);
		}
	}

	[HotSwappable]
	internal static class PawnDrawable
	{
		static PawnColumnLifeStageProxy _columnWorkerProxy = new PawnColumnLifeStageProxy();
		static Func<Pawn_TrainingTracker, TrainableDef, int> _getStepsDelegate = AccessTools.MethodDelegate<Func<Pawn_TrainingTracker, TrainableDef, int>>("RimWorld.Pawn_TrainingTracker:GetSteps");

		public static IEnumerable<(Texture, string?, Color?)> GetIcons(Tradeable tradeable)
		{
			if (tradeable.AnyThing is Pawn pawn)
			{
				foreach (var item in DoAnimalIcons(pawn))
					yield return (item.Item1, item.Item2, null);

				if (ModsConfig.BiotechActive)
					foreach (var item in DoBiotechIcons(pawn))
						yield return (item.Item1, item.Item2, null);

				if (ModsConfig.IdeologyActive)
					foreach (var item in DoIdeologyIcons(tradeable, pawn))
						yield return (item.Item1, item.Item2, null);
			}

			yield break;
		}


		private static IEnumerable<(Texture, string?)> DoAnimalIcons(Pawn pawn)
		{
			Texture ageIcon = _columnWorkerProxy.GetIcon(pawn);
			if (ageIcon != null)
				yield return (ageIcon, _columnWorkerProxy.GetTooltip(pawn));

			// Ridable
			if (pawn.IsCaravanRideable())
				yield return (Textures.RideableIcon, CaravanRideableUtility.GetIconTooltipText(pawn));

			// Bonded
			if (pawn.relations?.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null)
				yield return (ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond"), TrainableUtility.GetIconTooltipText(pawn));

			// Is pregnant
			if (pawn.health?.hediffSet?.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true) == true)
				yield return (ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant"), PawnColumnWorker_Pregnant.GetTooltipText(pawn));

			// Is sick
			if (pawn.health?.hediffSet?.AnyHediffMakesSickThought == true)
			{
				IEnumerable<string> entries = pawn.health.hediffSet.hediffs.Where(x => x.def.makesSickThought).Select(x => x.LabelCap);
				yield return (Textures.SickIcon, "CaravanAnimalSick".Translate() + ":\n\n" + entries.ToLineList(" - "));
			}

			if (pawn.training != null)
			{
				string? tooltipText = null;
				StringBuilder tooltip = new StringBuilder();
				List<TrainableDef> trainableDefs = TrainableUtility.TrainableDefsInListOrder;
				for (int i = 0; i < trainableDefs.Count; i++)
				{
					var trainableDef = trainableDefs[i];
					if (pawn.training.HasLearned(trainableDef))
					{
						int steps = _getStepsDelegate(pawn.training, trainableDef);
						if (trainableDef == TrainableDefOf.Tameness && steps == trainableDef.steps)
							continue;

						if (tooltip.Length == 0)
							tooltip.AppendLine("DynamicTradeWindowExtraIconsColumnTrained".Translate() + ":");

						tooltip.Append(trainableDef.LabelCap);
						tooltip.Append(": ");

						tooltip.Append(steps);
						tooltip.Append(" / ");
						tooltip.AppendLine(trainableDef.steps.ToString());
					}
				}
				if (tooltip.Length > 0)
					tooltipText = tooltip.ToString();

				yield return (Textures.TamenessIcon, tooltipText);
			}
		}

		private static IEnumerable<(Texture, string)> DoBiotechIcons(Pawn pawn)
		{
			if (pawn.IsColonyMech)
			{
				Pawn overseerPawn = pawn.GetOverseer();
				if (overseerPawn != null)
					yield return (PortraitsCache.Get(overseerPawn, new Vector2(36, 36), Rot4.South), "MechOverseer".Translate(overseerPawn));
			}

			XenotypeDef? xeno = pawn.genes?.Xenotype;
			if (xeno != null && xeno != XenotypeDefOf.Baseliner)
				yield return (xeno.Icon, xeno.LabelCap);
		}

		private static IEnumerable<(Texture, string)> DoIdeologyIcons(Tradeable tradeable, Pawn pawn)
		{
			if (pawn.guest == null)
				yield break;

			if (TransferableUIUtility.TransferableIsCaptive(tradeable))
			{
				if (pawn.HomeFaction == TradeSession.trader?.Faction)
					yield return (GuestUtility.RansomIcon, "SellingAsRansom".Translate());
				else
					yield return (GuestUtility.SlaveIcon, "SellingAsSlave".Translate());
			}
		}

		public static string GetSearchString(Tradeable tradeable)
		{
			if (tradeable.AnyThing is Pawn pawn == false)
				return "";

			StringBuilder result = new StringBuilder();

			if (_columnWorkerProxy.GetIcon(pawn) != null)
			{
				result.Append(pawn.ageTracker.CurLifeStage.label);
				result.Append(' ');
			}

			if (pawn.IsCaravanRideable())
			{
				result.Append(CaravanRideableUtility.GetIconTooltipText(pawn));
				result.Append(' ');
			}

			if (pawn.relations?.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null)
			{
				result.Append("AnimalBonded".Translate().Trim());
				result.Append(' ');
			}

			if (pawn.health?.hediffSet?.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true) == true)
			{
				result.Append("AnimalPregnant".Translate().Trim());
				result.Append(' ');
			}

			if (pawn.health?.hediffSet?.AnyHediffMakesSickThought == true)
			{
				result.Append("CaravanAnimalSick".Translate().Trim());
				result.Append(' ');
			}

			if (pawn.training != null)
			{
				result.Append("DynamicTradeWindowExtraIconsColumnTrained".Translate());
				result.Append(' ');
			}

			XenotypeDef? xeno = pawn.genes?.Xenotype;
			if (xeno != null && xeno != XenotypeDefOf.Baseliner)
			{
				result.Append(xeno.label);
				result.Append(' ');
			}

			return result.ToString();
		}
	}
}
