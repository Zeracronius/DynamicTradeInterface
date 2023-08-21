using RimWorld;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.UserInterface.TradeValidation;

public static class InsufficientSilver
{
	public static TaggedString? Validate()
	{
		if (TradeSession.deal.DoesTraderHaveEnoughSilver())
		{
			return null;
		}

		Dialog_Trade.lastCurrencyFlashTime = Time.time;
		return "ConfirmTraderShortFunds".Translate();
	}
}