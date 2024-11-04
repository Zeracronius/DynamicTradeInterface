using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Verse;

namespace DynamicTradeInterface.InterfaceComponents.TreeBox
{
	/// <summary>
	/// Specialised Listing_Tree used by <see cref="FilterTreeBox"/>.
	/// </summary>
	/// <seealso cref="Verse.Listing_Tree" />
	internal class Listing_TreeFilter : Listing_Tree
	{
		Rect _rect = new Rect();

		internal bool Node(TreeNode_FilterBox node, int indentLevel, float editOffset, bool recursiveDraw = true)
		{
			if (node.Visible == false || node.Enabled == false)
				return false;

			bool toggled = OpenCloseWidget(node, indentLevel, -1);

			float highlightHeight = lineHeight;
			if (node.SplitRow)
			{
				editOffset = 0;
				highlightHeight *= 2;
			}

			_rect.y = curY;
			_rect.height = highlightHeight;

			_rect.xMin = XAtIndentLevel(indentLevel) + 18f;
			_rect.width = ColumnWidth - _rect.xMin;
			Widgets.DrawHighlightIfMouseover(_rect);
			if (!node.Tooltip.NullOrEmpty())
			{
				if (Mouse.IsOver(_rect))
				{
					GUI.DrawTexture(_rect, TexUI.HighlightTex);
				}
				TooltipHandler.TipRegion(_rect, node.Tooltip);
			}

			Text.Anchor = TextAnchor.UpperLeft;
			if (node.HideLabel == false)
			{
				_rect.width = LabelWidth - _rect.xMin + editOffset;
				Widgets.Label(_rect, node.Label.Truncate(_rect.width));

				if (node.SplitRow)
				{
					curY += lineHeight;
					_rect.y = curY;
					_rect.width = ColumnWidth - _rect.xMin;
				}
				else
				{
					_rect.x = LabelWidth - editOffset;
					_rect.width = EditAreaWidth + editOffset;
				}
			}
			else
				_rect.width = ColumnWidth - _rect.xMin;


			_rect.height = lineHeight;

			if (node.Callback != null)
				node.Draw(in _rect);

			EndLine();
			if (recursiveDraw)
			{
				if (IsOpen(node, -1))
				{
					int count = node.children.Count;
					for (int i = 0; i < count; i++)
					{
						toggled |= Node((TreeNode_FilterBox)node.children[i], indentLevel + 1, editOffset);
					}
				}
			}

			return toggled;
		}

	}
}
