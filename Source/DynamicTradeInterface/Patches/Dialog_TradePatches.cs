using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.Patches
{
	[HarmonyPatch(typeof(Dialog_Trade), nameof(Dialog_Trade.PostOpen))]
	internal class Dialog_TradePatches
	{
		static void Prefix(Dialog_Trade __instance, bool ___giftsOnly)
		{
			if (Event.current.control == false)
			{
				var dynamicTradeWindow = new UserInterface.Window_DynamicTrade(___giftsOnly);

				WindowStack stack = Find.WindowStack;
				if (stack.TryRemove(__instance, false))
					stack.Add(dynamicTradeWindow);

				return;
			}
		}
	}
}
