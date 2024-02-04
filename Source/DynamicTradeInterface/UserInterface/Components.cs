using DynamicTradeInterface.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Mono.Security.X509.X520;

namespace DynamicTradeInterface.UserInterface
{
	[HotSwappable]
	public static class Components
	{
		private static string? _previousFocus;

		public static void TextFieldNumeric(Rect rect, string identity, ref int val, ref string buffer, int min = 0, int max = int.MaxValue)
		{
			if (buffer == null)
				buffer = val.ToString();

			GUI.SetNextControlName(identity);
			string text2 = GUI.TextField(rect, buffer, Text.CurTextFieldStyle);
			
			// If current focus is not this, and it was this before, then parse.
			string currentFocus = GUI.GetNameOfFocusedControl();
			if (currentFocus.Length == 0)
				return;

			// If the control is no longer focused, ensure that content actually matches value.
			if (_previousFocus == identity && currentFocus != identity)
			{
				_previousFocus = currentFocus;
				buffer = val.ToString();
				return;
			}

			// If textbox result is different from buffer, then try parse.
			if (text2 != buffer)
			{
				_previousFocus = currentFocus;
				int length = text2.Length;
				bool numeric = true;
				bool dashed = false;

				if (length > 0)
				{
					int i = 0;
					if (text2[0] == '-')
					{
						i = 1;
						dashed = true;
					}

					for (; i < length; i++)
					{
						char character = text2[i];
						if (Char.IsDigit(character) == false)
						{
							numeric = false;
							break;
						}
					}

				}

				if (numeric)
				{

					// Empty or only a dash
					if (length == 0 || (dashed && length == 1))
					{
						buffer = text2;
						return;
					}

					if (int.TryParse(text2, out int parsed))
					{
						if (parsed > max)
							parsed = max;
						else if (parsed < min)
							parsed = min;

						val = parsed;
						buffer = val.ToString();
						return;
					}

					buffer = text2;
				}
			}
		}
	}
}
