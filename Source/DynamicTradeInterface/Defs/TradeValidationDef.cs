using System.Collections.Generic;
using Verse;

namespace DynamicTradeInterface.Defs;

public class TradeValidationDef : Def
{
	/// <summary>
	/// TradeValidationActions must operate only on TradeSession.deal and thus require no parameters.
	/// </summary>
	internal delegate TaggedString? TradeValidationAction();

	/// <summary>
	/// Colon-based method identifier string for method called to validate a trade deal.
	/// </summary> 
	public string? validationCallbackHandler = null;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (var error in base.ConfigErrors()) 
		{
			yield return error;
		}

		if (string.IsNullOrEmpty(validationCallbackHandler))
		{
			yield return "TradeValidationDef must have a validation callback handler defined.";
		}
	}

	internal TradeValidationAction? validationCallback;
}