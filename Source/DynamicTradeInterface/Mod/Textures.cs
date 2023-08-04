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
	}
}
