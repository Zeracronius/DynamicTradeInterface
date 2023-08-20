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
			var texture = CreateReadableBaseTexture(source);

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

			for (int mip = 0; mip < texture.mipmapCount; ++mip)
			{
				var pixels = texture.GetPixels32(mip)
					.Select((c) => Color32.Lerp(c, blend, 0.5f))
					.ToArray();
				texture.SetPixels32(pixels, mip);
			}
			texture.Apply();

			return texture;
		}

		private static Texture2D CreateReadableBaseTexture(Texture2D source)
		{
			// https://github.com/SmashPhil/SmashTools/blob/6d084a8aff0eb15128033af81e8b3c0a5f8a366f/SmashTools/SmashTools/Utility/Extensions/Game/Ext_Texture.cs#L72

			var temp = RenderTexture.GetTemporary(
				source.width,
				source.height,
				0,
				RenderTextureFormat.Default,
				RenderTextureReadWrite.Linear);
			Graphics.Blit(source, temp);
			var previous = RenderTexture.active;
			RenderTexture.active = temp;

			Texture2D result = new Texture2D(source.width, source.height) {
				name = source.name
			};

			result.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
			result.Apply();

			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(temp);

			return result;
		}
	}
}
