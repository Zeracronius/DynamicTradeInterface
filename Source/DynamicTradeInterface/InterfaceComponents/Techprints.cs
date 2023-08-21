using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents
{
	[StaticConstructorOnStartup]
	internal class Techprints
	{
		public static readonly Texture2D? TechprintIcon_Missing;
		public static readonly Texture2D? TechprintIcon_Part;
		public static readonly Texture2D? TechprintIcon_Complete;
		public static bool Active;

		static Techprints()
		{
			Active = ModsConfig.RoyaltyActive && ModsConfig.IsActive("Spacemoth.ShowKnownTechprints");
			if (Active)
			{
				TechprintIcon_Missing = ContentFinder<Texture2D>.Get("UI/Icons/TechprintIcon_Missing");
				TechprintIcon_Part = ContentFinder<Texture2D>.Get("UI/Icons/TechprintIcon_Part");
				TechprintIcon_Complete = ContentFinder<Texture2D>.Get("UI/Icons/TechprintIcon_Complete");
			}
		}
	}
}
