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

#if V1_5

namespace DynamicTradeInterface.UserInterface.Columns.ColumnExtraIconTypes
{
	internal class BookDrawable
	{
		static Dictionary<Tradeable, BookDrawableRow> _drawableRows = new Dictionary<Tradeable, BookDrawableRow>();

		public static bool Initialise(Tradeable item)
		{
			if (item.AnyThing is Book book)
			{
				_drawableRows[item] = new BookDrawableRow(book);
				return true;
			}

			return false;
		}

		public static void Draw(ref Rect rect, Tradeable item, Transactor transactor, ref bool refresh)
		{
			if (_drawableRows.TryGetValue(item, out BookDrawableRow row))
				row.Draw(ref rect);
		}

		public static string GetSearchString(Tradeable item)
		{
			if (_drawableRows.TryGetValue(item, out BookDrawableRow row))
				return row.GetSearchString();

			return "";
		}
	}



	internal class BookDrawableRow
	{
		Texture2D? _icon = null;
		Color _iconColor = Color.white;
		string _tooltip;

		AccessTools.FieldRef<ReadingOutcomeDoerGainResearch, Dictionary<ResearchProjectDef, float>> researchValues = AccessTools.FieldRefAccess<ReadingOutcomeDoerGainResearch, Dictionary<ResearchProjectDef, float>>("values");

		public BookDrawableRow(Book book)
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

			if (researchCount > 0)
			{
				_icon = Textures.Schematic;

				if (finishedResearchCount == researchCount)
				{
					_iconColor = Color.green;
				}
				else if (finishedResearchCount > 0)
				{
					_iconColor = Color.yellow;
				}
				else
				{
					_iconColor = Color.red;
				}
				tooltipBuilder.AppendLine($"{finishedResearchCount}/{researchCount} projects researched.");
			}

			// ReadingOutcomeDoerGainResearch
			// ReadingOutcomeDoerJoyFactorModifier
			// BookOutcomeDoerGainSkillExp

			_tooltip = tooltipBuilder.ToString();
		}

		public void Draw(ref Rect rect)
		{
			if (_icon != null)
			{
				Rect iconRect = new Rect(rect.xMax - rect.height, rect.y, rect.height, rect.height).ContractedBy(1);
				GUI.DrawTexture(iconRect, _icon, ScaleMode.ScaleToFit, true, 1, color: _iconColor, 0, 0);

				if (Mouse.IsOver(iconRect))
				{
					Widgets.DrawHighlight(iconRect);
					TooltipHandler.TipRegion(iconRect, _tooltip);
				}
			}
		}

		public string GetSearchString()
		{
			return _tooltip;
		}
	}
}

#endif