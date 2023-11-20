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

		
		public static readonly Texture2D RideableIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Rideable");
		public static readonly Texture2D SickIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Sick"); 
	}
}
