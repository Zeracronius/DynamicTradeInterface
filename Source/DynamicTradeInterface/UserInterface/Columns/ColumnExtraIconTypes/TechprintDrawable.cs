using DynamicTradeInterface.InterfaceComponents;
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

	internal class TechprintDrawable
	{
		static Dictionary<Tradeable, TechprintDrawableRow> _drawableRows = new Dictionary<Tradeable, TechprintDrawableRow>();

		public static bool Initialise(Tradeable item)
		{
			if (Techprints.Active)
			{
				var techComp = item.AnyThing.TryGetComp<CompTechprint>();
				if (techComp != null)
				{
					_drawableRows[item] = new TechprintDrawableRow(techComp);
					return true;
				}
			}

			return false;
		}

		public static void Draw(ref Rect rect, Tradeable item, Transactor transactor, ref bool refresh)
		{
			if (_drawableRows.TryGetValue(item, out TechprintDrawableRow row))
				row.Draw(ref rect);
		}

		public static string GetSearchString(Tradeable item)
		{
			if (_drawableRows.TryGetValue(item, out TechprintDrawableRow row))
				return row.GetSearchString();

			return "";
		}
	}



	internal class TechprintDrawableRow
	{
		Texture? _icon;
		string _tooltip;
		string _searchTerm;

		public TechprintDrawableRow(CompTechprint techprintComp)
		{
			ResearchProjectDef project = techprintComp.Props.project;
			_searchTerm = project.label;
			if (project.TechprintRequirementMet)
			{
				_icon = InterfaceComponents.Techprints.TechprintIcon_Complete;
				_searchTerm += " complete";
			}
			else if (project.TechprintsApplied > 0)
			{
				_icon = InterfaceComponents.Techprints.TechprintIcon_Part;
				_searchTerm += " partial";
			}
			else
			{
				_icon = InterfaceComponents.Techprints.TechprintIcon_Missing;
				_searchTerm += " missing";
			}

			_tooltip = "ShowKnownTechprints.Tooltip".Translate(project.TechprintsApplied, project.TechprintCount, project.UnlockedDefs.Select(x => x.label).ToCommaList().CapitalizeFirst());
		}

		public void Draw(ref Rect rect)
		{
			if (_icon != null)
			{
				Rect iconRect = new Rect(rect.xMax - rect.height, rect.y, rect.height, rect.height).ContractedBy(1);
				GUI.DrawTexture(iconRect, _icon);

				if (Mouse.IsOver(iconRect))
				{
					Widgets.DrawHighlight(iconRect);
					TooltipHandler.TipRegion(iconRect, _tooltip);
				}
			}
		}

		public string GetSearchString()
		{
			return _searchTerm;
		}
	}
}
