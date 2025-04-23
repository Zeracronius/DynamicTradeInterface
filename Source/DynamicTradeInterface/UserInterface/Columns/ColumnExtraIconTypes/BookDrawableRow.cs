using DynamicTradeInterface.InterfaceComponents;
using DynamicTradeInterface.Mod;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

#if !V1_4

namespace DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes
{
	internal static class BookDrawable
	{
		static AccessTools.FieldRef<ReadingOutcomeDoerGainResearch, Dictionary<ResearchProjectDef, float>> researchValues = AccessTools.FieldRefAccess<ReadingOutcomeDoerGainResearch, Dictionary<ResearchProjectDef, float>>("values");



		public static IEnumerable<(Texture, string?, Color?)> GetIcons(Tradeable tradeable)
		{
			if (tradeable.AnyThing is Book book)
			{

				List<BookOutcomeDoer> doers = book.BookComp.Doers.ToList();
				StringBuilder tooltipBuilder = new StringBuilder();

				// Check book for research projects
				int researchCount = 0;
				int finishedResearchCount = 0;
				foreach (var researchDoer in doers.OfType<ReadingOutcomeDoerGainResearch>())
				{
					List<ResearchProjectDef> projects = researchValues(researchDoer).Keys.ToList();
					foreach (ResearchProjectDef project in projects)
					{
						researchCount++;
						if (project.IsFinished)
							finishedResearchCount++;
					}
				}

				Texture? icon = null;
				Color? color = null;
				if (researchCount > 0)
				{
					icon = Textures.Schematic;

					if (finishedResearchCount == researchCount)
					{
						color = Color.green;
					}
					else if (finishedResearchCount > 0)
					{
						color = Color.yellow;
					}
					else
					{
						color = Color.red;
					}
					tooltipBuilder.AppendLine($"{finishedResearchCount}/{researchCount} projects researched.");
				}

				// ReadingOutcomeDoerGainResearch
				// ReadingOutcomeDoerJoyFactorModifier
				// BookOutcomeDoerGainSkillExp

				if (icon != null)
					yield return (icon, tooltipBuilder.ToString(), color);
			}
			yield break;
		}

		public static string GetSearchString(Tradeable tradeable)
		{
			if (tradeable.AnyThing is Book book)
			{
				// Check book for research projects
				string searchString = string.Empty;
				List<BookOutcomeDoer> doers = book.BookComp.Doers.ToList();
				foreach (var researchDoer in doers.OfType<ReadingOutcomeDoerGainResearch>())
				{
					List<ResearchProjectDef> projects = researchValues(researchDoer).Keys.ToList();
					foreach (ResearchProjectDef project in projects)
					{
						if (searchString.Length > 0)
							searchString += " ";

						searchString += project.label;
					}
				}

				return searchString;
			}

			return string.Empty;
		}
	}
}

#endif