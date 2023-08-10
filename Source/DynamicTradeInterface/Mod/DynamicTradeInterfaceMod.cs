using DynamicTradeInterface.Defs;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.Mod
{
	public class DynamicTradeInterfaceMod : Verse.Mod
	{
		internal static Harmony Harmony = new Harmony("DynamicTradeInterfaceMod");

#pragma warning disable CS8618 // Will always be initialized by constructor by rimworld.
		internal static DynamicTradeInterfaceSettings Settings;
		internal static DynamicTradeInterfaceMod Current;
#pragma warning restore CS8618

        public DynamicTradeInterfaceMod(ModContentPack content) : base(content)
		{
		}


		public override string SettingsCategory()
		{
			return "Dynamic trade interface";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			//TODO column selection
		}

	}
}