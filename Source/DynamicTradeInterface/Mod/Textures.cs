using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.Mod
{
	[StaticConstructorOnStartup]
	internal static class Textures
	{
		public static readonly Texture2D TradeArrow = ContentFinder<Texture2D>.Get("UI/Widgets/TradeArrow");
		public static readonly Texture2D ShowSellableItemsIcon = ContentFinder<Texture2D>.Get("UI/Commands/SellableItems");
		public static readonly Texture2D TradeModeIcon = ContentFinder<Texture2D>.Get("UI/Buttons/TradeMode");
		public static readonly Texture2D GiftModeIcon = ContentFinder<Texture2D>.Get("UI/Buttons/GiftMode");
		public static readonly Texture2D SettingsIcon = ContentFinder<Texture2D>.Get("Icons/Settings");
		public static readonly Texture2D ResetIcon = ContentFinder<Texture2D>.Get("Icons/Reset");
		public static readonly Texture2D LockedIcon = ContentFinder<Texture2D>.Get("Icons/Locked");
		public static readonly Texture2D UnlockedIcon = ContentFinder<Texture2D>.Get("Icons/Unlocked");
		public static readonly Texture2D TamenessIcon = ContentFinder<Texture2D>.Get("ui/icons/trainables/Tameness");
		public static readonly Texture2D Plus = ContentFinder<Texture2D>.Get("ui/buttons/Plus");
		public static readonly Texture2D Remove = ContentFinder<Texture2D>.Get("Icons/Remove");
		public static readonly Texture2D Inspect = ContentFinder<Texture2D>.Get("ui/buttons/devroot/OpenInspector");
		public static readonly Texture2D ConfigurePresetsIcon = ContentFinder<Texture2D>.Get("Icons/Filter_presets");
		public static readonly Texture2D NotificationsEmptyIcon = ContentFinder<Texture2D>.Get("Icons/Notification_empty");
		public static readonly Texture2D NotificationsIcon = ContentFinder<Texture2D>.Get("Icons/Notification_present");
		public static readonly Texture2D RideableIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Rideable");
		public static readonly Texture2D SickIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Sick");
		public static readonly Texture2D ArrowLeft = ContentFinder<Texture2D>.Get("UI/widgets/ArrowLeft");
		public static readonly Texture2D ArrowRight = ContentFinder<Texture2D>.Get("UI/widgets/ArrowRight");
		public static readonly Texture2D Summary = ContentFinder<Texture2D>.Get("Icons/Show_summary");
		public static readonly Texture2D CheckboxOff = ContentFinder<Texture2D>.Get("Icons/Checkbox_off");
		public static readonly Texture2D CheckboxOn = ContentFinder<Texture2D>.Get("Icons/Checkbox_on");
		public static readonly Texture2D Save = ContentFinder<Texture2D>.Get("Icons/Save");
		public static readonly Texture2D Schematic = ContentFinder<Texture2D>.Get("Things/Item/Book/Schematic/Schematic");
		public static readonly Texture2D Book = ContentFinder<Texture2D>.Get("Things/Item/Book/Textbook/Textbook");

		public static Texture2D GenerateTexture(Texture2D source, Color32 tint)
		{
			var texture = CreateReadableBaseTexture(source);

			for (int mip = 0; mip < texture.mipmapCount; ++mip)
			{
				var pixels = texture.GetPixels32(mip)
					.Select((c) => Color32.Lerp(c, tint, 0.5f))
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

			Texture2D result = new Texture2D(source.width, source.height)
			{
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
