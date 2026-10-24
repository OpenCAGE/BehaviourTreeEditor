using System.Drawing;

namespace Brainiac.Design
{
	/// <summary>
	/// The editor's colour theme. OpenCAGE launches the editor with -darkMode=true when its own dark mode is on, and the
	/// palette follows OpenCAGE's (Scripts\Theming\ThemeColours.cs) so the two windows match. Set before any window or node
	/// type is touched: the node classes pick their pens and brushes up from here in their static initialisers.
	/// </summary>
	public static class Theme
	{
		public static bool Dark = false;

		public static readonly Color Window = Color.FromArgb(30, 30, 30);
		public static readonly Color Surface = Color.FromArgb(37, 37, 38);
		public static readonly Color Raised = Color.FromArgb(45, 45, 48);
		public static readonly Color Input = Color.FromArgb(51, 51, 55);
		public static readonly Color Border = Color.FromArgb(63, 63, 70);
		public static readonly Color Text = Color.FromArgb(241, 241, 241);
		public static readonly Color TextDim = Color.FromArgb(158, 158, 158);
		public static readonly Color Selection = Color.FromArgb(9, 71, 113);
		public static readonly Color Hover = Color.FromArgb(62, 62, 64);

		/// <summary>The graph's background.</summary>
		public static readonly Color Canvas = Color.FromArgb(28, 28, 30);

		/// <summary>The lines joining the nodes (and the fainter ones into referenced behaviours).</summary>
		public static Color Edge { get { return Dark ? Color.FromArgb(196, 196, 200) : Color.Black; } }
		public static Color EdgeReadOnly { get { return Dark ? Color.FromArgb(92, 92, 98) : Color.LightGray; } }

		/// <summary>The outline of the selected / current node.</summary>
		public static Brush NodeOutline { get { return Dark ? Brushes.White : Brushes.Black; } }
	}
}
