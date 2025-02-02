using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.Columns
{
	internal static class ColumnCategory
	{
		struct Cache
		{
			public string Label;
			public string Tooltip;
		}

		private static Dictionary<Tradeable, Cache> _editableCache = new Dictionary<Tradeable, Cache>();

		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			StringBuilder toolTipBuilder = new StringBuilder();
			foreach (var row in rows)
			{
				ThingDef? def = row.ThingDef;
				if (def != null)
				{
					toolTipBuilder.Clear();
					Cache cache = new Cache();
					string label = "";

					Thing thing = row.AnyThing;
					if (thing is Pawn pawn && def.race.intelligence == Intelligence.Humanlike)
					{
						label = "Pawn";

						XenotypeDef? xenotype = pawn.genes?.Xenotype;
						if (xenotype != null)
						{
							label = xenotype.LabelCap;
							toolTipBuilder.AppendLine(label);
						}

						toolTipBuilder.AppendLine("Pawn");
					}

					if (def.thingCategories?.Count > 0)
					{
						for (int i = 0; i < def.thingCategories.Count; i++)
						{
							ThingCategoryDef category = def.thingCategories[i];
							if (i == 0 && label.Length == 0)
								label = category.LabelCap;
							toolTipBuilder.AppendLine(category.LabelCap);
						}
					}

					cache.Label = label;
					cache.Tooltip = toolTipBuilder.ToString();
					_editableCache[row] = cache;
				}
			}
		}


		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			_editableCache.Clear();
		}

		public static void Draw(ref Rect rect, Tradeable row, Transactor transactor, ref bool refresh)
		{
			if (_editableCache.TryGetValue(row, out Cache category))
			{
				Widgets.Label(rect, category.Label);
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegion(rect, category.Tooltip);
				}
			}
		}

		public static Func<Tradeable, IComparable> OrderbyValue(Transactor transactor)
		{
			return (Tradeable row) =>
			{
				if (_editableCache.TryGetValue(row, out Cache category))
					return category.Tooltip;
				else
					return string.Empty;
			};
		}

		public static string SearchValue(Tradeable row, Transactor transactor)
		{
			if (_editableCache.TryGetValue(row, out Cache category))
				return category.Tooltip;

			return string.Empty;
		}
	}
}
