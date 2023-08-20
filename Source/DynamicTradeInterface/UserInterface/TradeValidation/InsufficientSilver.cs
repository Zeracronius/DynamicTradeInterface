using RimWorld;
using UnityEngine;

namespace DynamicTradeInterface.UserInterface.TradeValidation;

public static class InsufficientSilver
{
	public static bool Validate()
	{
		var result = TradeSession.deal.DoesTraderHaveEnoughSilver();
		if (!result)
		{
			Dialog_Trade.lastCurrencyFlashTime = Time.time;
		}

		return result;
	}
}