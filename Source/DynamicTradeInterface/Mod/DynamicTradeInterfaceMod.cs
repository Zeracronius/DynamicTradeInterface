using DynamicTradeInterface.Defs;
using DynamicTradeInterface.UserInterface;
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

		Dialog_TradeConfiguration? _configWindow;
		public override void DoSettingsWindowContents(Rect inRect)
		{
			if (_configWindow == null)
			{
				_configWindow = new Dialog_TradeConfiguration();
				_configWindow.OnClosed += ConfigWindow_OnClosed;
				_configWindow.PreOpen();
				_configWindow.PostOpen();
			}

			_configWindow.DoWindowContents(inRect);
		}

		private void ConfigWindow_OnClosed(object sender, bool e)
		{
			_configWindow?.PreClose();
			_configWindow?.PostClose();
			_configWindow = null;
		}
	}
}