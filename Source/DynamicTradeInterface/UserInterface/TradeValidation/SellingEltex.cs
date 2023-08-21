using System.Collections.Generic;
using DynamicTradeInterface.Mod;
using RimWorld;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.TradeValidation;

public class SellingEltex
{
	private static readonly string _eltexDefName = "VPE_Eltex";

	private static bool _initialized = false;
	private static readonly HashSet<ThingDef> _eltexThingDefs = new HashSet<ThingDef>();

	/// <summary>
	/// This class is lazily initialized to avoid connecting mod-specific validators to the main mod initialization process.
	/// Based on 1.4/Source/VanillaPsycastsExpanded/PsycastUtility.cs
	/// </summary>
	private static void TryInitialize()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;

		// errorOnFail is disabled to handle errors here instead of just logging a generic error.
		ThingDef eltexDef = DefDatabase<ThingDef>.GetNamed(_eltexDefName, false);
		if (eltexDef.defName != _eltexDefName)
		{
			// In this error state, SellingEltex will still be active, but since _eltexThings is empty it will never
			// invalidate a trade.
			Logging.Error("The SellingEltex trade validator could not find an eltex definition.");
			return;
		}

		_eltexThingDefs.Add(eltexDef);

		foreach (var recipeDef in DefDatabase<RecipeDef>.AllDefsListForReading)
		{
			foreach (var ingredient in recipeDef.ingredients)
			{
				if (ingredient.IsFixedIngredient && ingredient.FixedIngredient == eltexDef)
				{
					_eltexThingDefs.Add(recipeDef.ProducedThingDef);
					break;
				}
			}
		}

		foreach (var thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
		{
			if (thingDef.costList == null)
			{
				continue;
			}

			foreach (var cost in thingDef.costList)
			{
				if (cost.thingDef == eltexDef)
				{
					_eltexThingDefs.Add(thingDef);
					break;
				}
			}
		}
	}

	/// <summary>
	/// Checks if the Empire might get angry about this trade deal.
	/// Based on VanillaPsycastsExpanded/HarmonyPatches/Transferable_CanAdjustBy_Patch.cs
	/// </summary>
	/// <returns>Null if the deal contains no eltex or if it is being sold to the Empire.</returns>
	public static TaggedString? Validate()
	{
		TryInitialize();

		if (TradeSession.trader.Faction == Faction.OfEmpire)
		{
			return null;
		}

		foreach (var tradeable in TradeSession.deal.AllTradeables)
		{
			if (tradeable.ActionToDo == TradeAction.PlayerSells && _eltexThingDefs.Contains(tradeable.ThingDef))
			{
				return TradeSession.giftMode ? "VPE.GiftingEltexWarning".Translate() : "VPE.SellingEltexWarning".Translate();
			}
		}

		return null;
	}
}