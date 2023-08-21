using HarmonyLib;
using System;
using System.Diagnostics;
using System.Reflection;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents
{
	[StaticConstructorOnStartup]
	internal static class PsycastsExpanded
	{
		private static readonly MethodInfo? _isEltexOrHasEltexMaterial = null;

		public static readonly bool Active = false;

		static PsycastsExpanded()
		{
			if (ModsConfig.RoyaltyActive && ModsConfig.IsActive("VanillaExpanded.VPsycastsE"))
			{
				Type? type = Type.GetType("VanillaPsycastsExpanded.PsycastUtility, VanillaPsycastsExpanded, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null");
				_isEltexOrHasEltexMaterial = AccessTools.Method(type, "IsEltexOrHasEltexMaterial", new Type[] { typeof(ThingDef) });
				Active = _isEltexOrHasEltexMaterial != null;
			}
		}

		public static bool IsEltexOrHasEltexMaterial(ThingDef thing)
		{
			Debug.Assert(Active);
			return (bool)_isEltexOrHasEltexMaterial!.Invoke(null, new object[] { (object)thing });
		}
	}
}
