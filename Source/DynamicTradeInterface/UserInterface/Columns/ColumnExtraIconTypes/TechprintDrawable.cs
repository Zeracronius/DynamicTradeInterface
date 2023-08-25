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
	internal class TechprintDrawable : IDrawable
	{
		Texture? _icon;
		string _tooltip;

		public TechprintDrawable(CompTechprint techprintComp)
		{
			ResearchProjectDef project = techprintComp.Props.project;
			if (project.TechprintRequirementMet)
				_icon = InterfaceComponents.Techprints.TechprintIcon_Complete;
			else if (project.TechprintsApplied > 0)
				_icon = InterfaceComponents.Techprints.TechprintIcon_Part;
			else
				_icon = InterfaceComponents.Techprints.TechprintIcon_Missing;

			_tooltip = "ShowKnownTechprints.Tooltip".Translate(project.TechprintsApplied, project.TechprintCount, project.UnlockedDefs.Select(x => x.label).ToCommaList().CapitalizeFirst());
		}

		public void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
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
	}
}
