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
		internal static DynamicTradeInterfaceSettings Settings;

		public DynamicTradeInterfaceMod(ModContentPack content) : base(content)
		{
			Settings = GetSettings<DynamicTradeInterfaceSettings>();
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