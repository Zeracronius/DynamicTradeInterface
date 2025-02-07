using DynamicTradeInterface.Mod;
using System.Linq;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents
{
	[StaticConstructorOnStartup]
	internal static class GeneAssistant
	{
		private static readonly Texture2D? _missingIcon;
		private static readonly Texture2D? _mixedIcon;
		private static readonly Texture2D? _isolatedIcon;

		public static readonly bool Active;

		public enum GeneType
		{
			None,
			Missing,
			Mixed,
			Isolated,
		}

		static GeneAssistant()
		{
			Active = ModsConfig.BiotechActive && ModsConfig.IsActive("rimworld.randomcoughdrop.geneassistant");
			if (Active)
			{
				var genepack = ContentFinder<Texture2D>.Get("Things/Item/Special/Genepack/Genepack_e");
				if (genepack != null)
				{
					_missingIcon = CreateTexture(genepack, GeneType.Missing);
					_mixedIcon = CreateTexture(genepack, GeneType.Mixed);
					_isolatedIcon = CreateTexture(genepack, GeneType.Isolated);
				}
			}
		}

		public static Texture2D? IconFor(GeneType type)
		{
			switch (type)
			{
				case GeneType.Missing:
					return _missingIcon;
				case GeneType.Mixed:
					return _mixedIcon;
				case GeneType.Isolated:
					return _isolatedIcon;
				default:
					return null;
			}
		}

		private static Texture2D CreateTexture(Texture2D source, GeneType type)
		{
			Color32 blend = default;
			switch (type)
			{
				case GeneType.Missing:
					blend = new Color32(255, 0, 0, 1);
					break;
				case GeneType.Mixed:
					blend = new Color32(255, 255, 0, 1);
					break;
				case GeneType.Isolated:
					blend = new Color32(0, 255, 0, 1);
					break;
			}

			return Textures.GenerateTexture(source, blend);
		}
	}
}
