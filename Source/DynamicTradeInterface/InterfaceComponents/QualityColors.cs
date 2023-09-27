using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents
{
	[StaticConstructorOnStartup]
	internal static class QualityColors
	{
		public static bool Active;

		static QualityColors()
		{
			Active = ModsConfig.IsActive("legodude17.qualcolor");
			if (Active)
			{
				MethodInfo original = AccessTools.Method("QualityColors.QualityColorsMod:AddColors");
				Harmony.ReversePatch(original, new HarmonyMethod(typeof(QualityColors), "GetColor"));
			}
		}

		public static void GetColor(Transferable trad, Rect idRect, ref Color labelColor)
		{

		}
	}
}
