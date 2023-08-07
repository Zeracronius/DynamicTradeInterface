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
		static void Prefix(Dialog_Trade __instance, out UserInterface.Window_DynamicTrade? __state)
		{
			if (Event.current.control == false)
			{
				__state = new UserInterface.Window_DynamicTrade();
				WindowStack stack = Find.WindowStack;
				if (stack.TryRemove(__instance, false))
					stack.Add(__state);

				return;
			}
			__state = null;
		}

		static void Postfix(UserInterface.Window_DynamicTrade? __state, List<Tradeable> ___cachedTradeables)
		{
			if (__state != null)
				__state.Initialize(___cachedTradeables);
		}
	}
}
