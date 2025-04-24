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
	internal static class GenepackDrawable
	{
		private static Dictionary<GeneDef, GeneAssistant.GeneType> _bankedGenes = new Dictionary<GeneDef, GeneAssistant.GeneType>();

		public static IEnumerable<(Texture, string?, Color?)> GetIcons(Tradeable tradeable)
		{
			if (GeneAssistant.Active && tradeable.AnyThing is Genepack genepack)
			{
				List<GeneAssistant.GeneType>? types = genepack.GeneSet?.GenesListForReading?
					.Select((gene) => _bankedGenes.TryGetValue(gene, GeneAssistant.GeneType.Missing))
					.Distinct()
					.ToList();

				string tooltip = string.Empty;
				GeneAssistant.GeneType geneType = GeneAssistant.GeneType.None;
				if (types?.Count > 0)
				{
					if (types.Any((x) => x == GeneAssistant.GeneType.Missing))
					{
						geneType = GeneAssistant.GeneType.Missing;
						tooltip = "DynamicTradeWindowGenepackMissing".Translate();
					}
					else if (types.All((x) => x == GeneAssistant.GeneType.Isolated))
					{
						geneType = GeneAssistant.GeneType.Isolated;
						tooltip = "DynamicTradeWindowGenepackIsolated".Translate();
					}
					else
					{
						geneType = GeneAssistant.GeneType.Mixed;
						tooltip = "DynamicTradeWindowGenepackMixed".Translate();
					}
				}

				Texture? icon = GeneAssistant.IconFor(geneType);
				if (icon != null)
					yield return (icon, tooltip, null);
			}

			yield break;
		}

		public static string GetSearchString(Tradeable tradeable)
		{
			if (GeneAssistant.Active && tradeable.AnyThing is Genepack genepack)
			{
				List<GeneAssistant.GeneType>? types = genepack.GeneSet?.GenesListForReading?
					.Select((gene) => _bankedGenes.TryGetValue(gene, GeneAssistant.GeneType.Missing))
					.Distinct()
					.ToList();

				string tooltip = "";
				if (types?.Count > 0)
				{
					if (types.Any((x) => x == GeneAssistant.GeneType.Missing))
					{
						tooltip = "DynamicTradeWindowGenepackMissing".Translate();
					}
					else if (types.All((x) => x == GeneAssistant.GeneType.Isolated))
					{
						tooltip = "DynamicTradeWindowGenepackIsolated".Translate();
					}
					else
					{
						tooltip = "DynamicTradeWindowGenepackMixed".Translate();
					}
				}

				return tooltip;
			}

			return "";
		}


		public static void PostOpen(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (transactor != Transactor.Colony)
				return;


			// Get all player buildings across all colonies.
			List<Building> allPlayerBuildings = new List<Building>();
			List<Map> maps = Find.Maps;
			for (int i = maps.Count - 1; i >= 0; i--)
			{
				Map map = maps[i];
				if (map.IsPlayerHome)
					allPlayerBuildings.AddRange(map.listerBuildings.allBuildingsColonist);
			}

			foreach (Building building in allPlayerBuildings)
			{
				// Find all genepack container components across all buildings
				CompGenepackContainer? genepackContainer = building.TryGetComp<CompGenepackContainer>();
				if (genepackContainer == null)
					continue;

				foreach (Genepack? genepack in genepackContainer.ContainedGenepacks)
				{
					GeneSet? geneSet = genepack?.GeneSet;

					if (geneSet == null)
						continue;

					List<GeneDef> genes = geneSet.GenesListForReading;

					// If genepack only contains a single gene, mark it as isolated.
					if (genes.Count == 1)
					{
						// Do overwrite even if gene has already been detected as mixed.
						_bankedGenes[genes[0]] = GeneAssistant.GeneType.Isolated;
						continue;
					}

					// Otherwise mark it as mixed.
					for (int i = genes.Count - 1; i >= 0; i--)
					{
						// Don't overwrite if gene has already been logged as isolated.
						if (_bankedGenes.ContainsKey(genes[i]) == false)
							_bankedGenes.TryAdd(genes[i], GeneAssistant.GeneType.Mixed);
					}
				}
			}
		}

		public static void PostClosed(IEnumerable<Tradeable> rows, Transactor transactor)
		{
			if (transactor == Transactor.Colony)
				_bankedGenes.Clear();
		}
	}
}
