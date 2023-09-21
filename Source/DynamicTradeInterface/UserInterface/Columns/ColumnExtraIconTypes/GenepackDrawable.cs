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
	internal class GenepackDrawable : IDrawable
	{
		private static Dictionary<GeneDef, GeneAssistant.GeneType> _bankedGenes = new Dictionary<GeneDef, GeneAssistant.GeneType>();

		private GeneAssistant.GeneType _geneType = GeneAssistant.GeneType.None;
		private Texture? _icon = null;
		private string _tooltip = string.Empty;

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

		public GenepackDrawable(Genepack genepack)
		{
			List<GeneAssistant.GeneType>? types = genepack.GeneSet
				?.GenesListForReading
				.Select((gene) => _bankedGenes.TryGetValue(gene, GeneAssistant.GeneType.Missing))
				.Distinct()
				.ToList();

			if (types?.Count > 0)
			{
				if (types.Any((x) => x == GeneAssistant.GeneType.Missing))
				{
					_geneType = GeneAssistant.GeneType.Missing;
					_tooltip = "DynamicTradeWindowGenepackMissing".Translate();
				}
				else if (types.All((x) => x == GeneAssistant.GeneType.Isolated))
				{
					_geneType = GeneAssistant.GeneType.Isolated;
					_tooltip = "DynamicTradeWindowGenepackIsolated".Translate();
				}
				else
				{
					_geneType = GeneAssistant.GeneType.Mixed;
					_tooltip = "DynamicTradeWindowGenepackMixed".Translate();
				}
			}

			_icon = GeneAssistant.IconFor(_geneType);
		}

		public void Draw(ref Rect rect, Transactor transactor, ref bool refresh)
		{
			if (_icon != null)
			{
				Rect iconRect = new Rect(rect.xMax - rect.height, rect.y, rect.height, rect.height).ContractedBy(1);
				GUI.DrawTexture(iconRect, _icon);

				if (Mouse.IsOver(iconRect))
					TooltipHandler.TipRegion(iconRect, _tooltip);
			}
		}

		public string GetSearchString()
		{
			return _tooltip;
		}
	}
}
