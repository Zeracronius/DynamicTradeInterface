using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;

namespace DynamicTradeInterface.Notifications
{
	internal class NotificationEntry : IExposable
	{
		public bool Active;
		public string? RegExText;

		public Regex? Regex { get; private set; }

        public NotificationEntry()
        {
            
        }

        public NotificationEntry(string value)
        {
			SetText(value);
			Active = true;
        }

        public void SetText(string text)
		{
			try
			{
				Regex = new Regex(text);
			}
			catch
			{ }
			RegExText = text;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref Active, "Active");
			Scribe_Values.Look(ref RegExText, "RegEx");

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				try
				{
					Regex = new Regex(RegExText);
				}
				catch
				{ }
			}
		}
	}
}
