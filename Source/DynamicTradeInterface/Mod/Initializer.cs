using Verse;

namespace DynamicTradeInterface.Mod
{

	[StaticConstructorOnStartup]
	internal static class Initializer
	{
		static Initializer()
		{
			DynamicTradeInterfaceMod.Harmony.PatchAll();
			DynamicTradeInterfaceMod.Current = LoadedModManager.GetMod<DynamicTradeInterfaceMod>();
			DynamicTradeInterfaceMod.Settings = DynamicTradeInterfaceMod.Current.GetSettings<DynamicTradeInterfaceSettings>();
			DynamicTradeInterfaceMod.Settings.Initialize();
		}
	}
}
