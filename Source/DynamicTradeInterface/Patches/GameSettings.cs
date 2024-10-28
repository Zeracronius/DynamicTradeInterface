using DynamicTradeInterface.Defs;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DynamicTradeInterface.Patches
{
	[HarmonyPatch]
	internal static class GameSettings
	{
		public static List<Notifications.NotificationEntry> Notifications = new List<Notifications.NotificationEntry>();

		[HarmonyPatch(typeof(Game), "ExposeSmallComponents"), HarmonyPostfix]
		private static void ExposeComponents()
		{
			Scribe_Collections.Look(ref Notifications, "DTI_Notifications", LookMode.Deep);

			if (Notifications == null)
				Notifications = new List<Notifications.NotificationEntry>();
		}
	}
}
