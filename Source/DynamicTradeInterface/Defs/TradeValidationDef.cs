using System.Collections.Generic;
using Verse;

namespace DynamicTradeInterface.Defs;

public class TradeValidationDef : Def
{
	/// <summary>
	/// TradeValidationActions must operate only on TradeSession.deal and thus require no parameters.
	/// </summary>
	internal delegate bool TradeValidationAction();

	/// <summary>
	/// Colon-based method identifier string for method called to validate a trade deal.
	/// </summary> 
	public string? validationCallbackHandler = null;

	/// <summary>
	/// Translatable string ID to show in confirmation dialogs when this validation fails.
	/// </summary>
	public string? textKey;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (var error in base.ConfigErrors()) 
		{
			yield return error;
		}

		if (string.IsNullOrEmpty(validationCallbackHandler))
		{
			yield return "TradeValidationDef must have a handler defined.";
		}

		if (textKey == null || !textKey.CanTranslate())
		{
			yield return "TradeValidationDef must have a translatable string id as its titleText.";
		}
	}
	
	internal TradeValidationAction? validationCallback;
	internal TaggedString? translatedText;
}