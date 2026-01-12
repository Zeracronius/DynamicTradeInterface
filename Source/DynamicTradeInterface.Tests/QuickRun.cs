using DevTools.Testing;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using Verse;

namespace DynamicTradeInterface.Tests
{
	[UnitTest(TestType.Playing), TestCategory("manual")]
	public class QuickRun
	{
		[Test]
		[ExecutionPriority(Priority.First)]
		public IEnumerator StartTrade()
		{
			var map = Find.CurrentMap;

			IncidentDef incident = IncidentDefOf.TraderCaravanArrival;
			var storyComp = Find.Storyteller.storytellerComps.OfType<StorytellerComp_FactionInteraction>()
				.FirstOrDefault(x => ((StorytellerCompProperties_FactionInteraction)x.props).incident == incident);
			IncidentParms parms = storyComp.GenerateParms(incident.category, map);

			Pawn playerTrader = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).FirstOrDefault(x => x.health.capacities.CapableOf(PawnCapacityDefOf.Talking));
			Assert.IsNotNull(playerTrader, "Player has no trader pawn");

			Assert.IsTrue(incident.Worker.TryExecute(parms), "Could not spawn trader");

			foreach (TraderKindDef traderKind in DefDatabase<TraderKindDef>.AllDefs.Where((TraderKindDef t) => t.orbital))
			{
				parms = new IncidentParms
				{
					target = Find.CurrentMap,
					traderKind = traderKind
				};
				IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(parms);
				break;
			}

			var orbitalShipManager = Find.CurrentMap.passingShipManager;
			foreach (var tradeShip in orbitalShipManager.passingShips.OfType<TradeShip>())
			{
				Find.WindowStack.Add(new Dialog_Trade(playerTrader, tradeShip));
				break;
			}

			yield return Test.Suspend(-1);
		}
	}
}
