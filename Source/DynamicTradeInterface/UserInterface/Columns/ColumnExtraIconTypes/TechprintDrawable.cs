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
		public IEnumerable<(Texture, string?, Color?)> GetIcons(Tradeable tradeable)
		{
			if (Techprints.Active)
			{
				var techComp = tradeable.AnyThing.TryGetComp<CompTechprint>();
				if (techComp != null)
				{
					ResearchProjectDef project = techComp.Props.project;
					Texture? icon = Techprints.TechprintIcon_Missing;
					if (project.TechprintRequirementMet)
					{
						icon = Techprints.TechprintIcon_Complete;
					}
					else if (project.TechprintsApplied > 0)
					{
						icon = Techprints.TechprintIcon_Part;
					}

					if (icon != null)
					{
						string tooltip = "ShowKnownTechprints.Tooltip".Translate(project.TechprintsApplied, project.TechprintCount, project.UnlockedDefs.Select(x => x.label).ToCommaList().CapitalizeFirst());
						yield return (icon, tooltip, null);
					}
				}
			}
			yield break;
		}

		public string GetSearchString(Tradeable tradeable)
		{
			if (Techprints.Active)
			{
				var techComp = tradeable.AnyThing.TryGetComp<CompTechprint>();
				if (techComp != null)
				{
					ResearchProjectDef project = techComp.Props.project;
					string searchTerm = project.label;
					if (project.TechprintRequirementMet)
					{
						searchTerm += " complete";
					}
					else if (project.TechprintsApplied > 0)
					{
						searchTerm += " partial";
					}
					else
					{
						searchTerm += " missing";
					}
					return searchTerm;
				}
			}
			return "";
		}
	}
}
