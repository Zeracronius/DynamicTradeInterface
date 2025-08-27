using DevTools;
using DevTools.UnitTesting;
using RimWorld;
using System;
using Verse;
using System.Linq;
using DynamicTradeInterface.Defs;
using System.Collections.Generic;
using UnityEngine;
using LudeonTK;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Assertions;

namespace DynamicTradeInterface.Tests
{
	[UnitTest(TestType.Playing)]
	public class WindowTests
	{
		[Test]
		[ExecutionPriority(Priority.First)]
		public void Open()
		{
			var map = Find.CurrentMap;

			IncidentDef incident = IncidentDefOf.TraderCaravanArrival;
			var storyComp = Find.Storyteller.storytellerComps.OfType<StorytellerComp_FactionInteraction>()
				.FirstOrDefault(x => ((StorytellerCompProperties_FactionInteraction)x.props).incident == incident);
			IncidentParms parms = storyComp.GenerateParms(incident.category, map);

			Pawn playerTrader = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).FirstOrDefault(x => x.health.capacities.CapableOf(PawnCapacityDefOf.Talking));
			Assert.IsNotNull(playerTrader, "Player has no trader pawn");

			Assert.IsTrue(incident.Worker.TryExecute(parms), "Could not spawn trader");

			// Find spawned trader
			Test.BeginGroup("Caravan");
			foreach (Pawn target in map.mapPawns.PawnsInFaction(parms.faction))
			{
				if (target.kindDef.trader && target.TraderKind != null)
				{
					// Start trade
					Test.BeginGroup(target.TraderKind.LabelCap);
					Find.WindowStack.Add(new Dialog_Trade(playerTrader, target));
					TestWindow();
					Test.EndGroup(target.TraderKind.LabelCap);
					break;
				}
			}
			Test.EndGroup("Caravan");


			foreach (TraderKindDef traderKind in DefDatabase<TraderKindDef>.AllDefs.Where((TraderKindDef t) => t.orbital))
			{
				parms = new IncidentParms
				{
					target = Find.CurrentMap,
					traderKind = traderKind
				};
				IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(parms);
			}

			var orbitalShipManager = Find.CurrentMap.passingShipManager;
			foreach (var tradeShip in orbitalShipManager.passingShips.OfType<TradeShip>())
			{
				Test.BeginGroup(tradeShip.TraderKind.LabelCap);
				Find.WindowStack.Add(new Dialog_Trade(playerTrader, tradeShip));
				TestWindow();
				Test.EndGroup(tradeShip.TraderKind.LabelCap);
			}


			//ColumnButtons();
			//ColumnCaption();
			//ColumnCategory();
			//ColumnCounter();
			//ColumnDurability();
			//ColumnPrice();
			//ColumnPricePerWeight();
			//ColumnQuantity();
		}

		private void TestWindow()
		{
			UserInterface.Window_DynamicTrade tradeWindow = Find.WindowStack.WindowOfType<UserInterface.Window_DynamicTrade>();
			Assert.IsNotNull(tradeWindow);

			Test.BeginGroup("Colony");
			var colonyWares = tradeWindow.ColonyTable.RowItems.Select(x => x.RowObject).ToList();
			ColumnMoreIcons(colonyWares);
			Test.EndGroup("Colony");

			Test.BeginGroup("Trader");
			var traderWares = tradeWindow.TraderTable.RowItems.Select(x => x.RowObject).ToList();
			ColumnMoreIcons(traderWares);
			Test.EndGroup("Trader");

			tradeWindow.Close();
		}

		public void ColumnButtons()
		{
			// Min, Max, Increments, Affordable
			Test.BeginGroup(nameof(ColumnButtons));

			Test.EndGroup(nameof(ColumnButtons));
		}

		public void ColumnCaption()
		{
			// Has data (Can click?)
			Test.BeginGroup(nameof(ColumnCaption));

			Test.EndGroup(nameof(ColumnCaption));
		}

		public void ColumnCategory()
		{
			// Has data
			Test.BeginGroup(nameof(ColumnCategory));

			Test.EndGroup(nameof(ColumnCategory));
		}

		public void ColumnCounter()
		{
			// Can manually enter, too high, too low.
			Test.BeginGroup(nameof(ColumnCounter));

			Test.EndGroup(nameof(ColumnCounter));
		}

		public void ColumnDurability()
		{
			// Has data, compare with assumed
			Test.BeginGroup(nameof(ColumnDurability));

			Test.EndGroup(nameof(ColumnCategory));
		}

		public void ColumnPrice()
		{
			// Has data
			Test.BeginGroup(nameof(ColumnPrice));

			Test.EndGroup(nameof(ColumnPrice));
		}

		public void ColumnPricePerWeight()
		{
			// Has data, compare with assumed
			Test.BeginGroup(nameof(ColumnPricePerWeight));
			
			Test.EndGroup(nameof(ColumnPricePerWeight));
		}

		public void ColumnQuantity()
		{
			// Test counting extra for orbital
			Test.BeginGroup(nameof(ColumnQuantity));

			

			Test.EndGroup(nameof(ColumnQuantity));
		}

		public void ColumnMoreIcons(List<Tradeable> wares)
		{
			Test.BeginGroup(nameof(ColumnMoreIcons));
			foreach (var item in DefDatabase<MoreIconsDef>.AllDefsListForReading)
			{
				Test.BeginGroup(item.defName);
				Expect.Any(wares, x => item.GetIcons(x).Any());
				Test.EndGroup(item.defName);
			}
			Test.EndGroup(nameof(ColumnMoreIcons));
		}

		[Test]
		public void Confirm()
		{
			// Can afford
			// Can't afford
			// Eltex integration
			// Warnings

		}


	}
}
