using DynamicTradeInterface.Mod;
using DynamicTradeInterface.UserInterface;
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
			bool control = Event.current.control;
			bool openDefault = DynamicTradeInterfaceMod.Settings.OpenAsDefault;

			if (DynamicTradeInterfaceMod.Settings.PauseAfterTrade)
			{
				Find.TickManager.Pause();
			}

			if ((openDefault && control == false) ||
				(openDefault == false && control))
			{
				var dynamicTradeWindow = new Window_DynamicTrade(___giftsOnly);

				WindowStack stack = Find.WindowStack;
				if (stack.TryRemove(__instance, false))
					stack.Add(dynamicTradeWindow);

				return;
			}
		}
	}
}
