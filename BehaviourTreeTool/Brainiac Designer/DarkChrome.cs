using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using WeifenLuo.WinFormsUI.Docking;

namespace Brainiac.Design
{
	/// <summary>
	/// Dark window chrome for the editor when <see cref="Theme.Dark"/> is on: Windows' own dark mode for the title bar,
	/// scrollbars, tree views and combo boxes; the DockPanel's skin; and a pass over every control (including ones added
	/// later, as the property grid rebuilds itself on each selection) that swaps light backgrounds and dark text for the
	/// palette. The graph itself is drawn by the nodes and BehaviorTreeView, which read <see cref="Theme"/> directly.
	/// </summary>
	internal static class DarkChrome
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int SetPreferredAppModeDelegate(int mode);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate bool AllowDarkModeForAppDelegate(bool allow);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate bool AllowDarkModeForWindowDelegate(IntPtr window, bool allow);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate void VoidDelegate();

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr LoadLibrary(string name);
		[DllImport("kernel32.dll")] private static extern IntPtr GetProcAddress(IntPtr module, IntPtr ordinal);
		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)] private static extern int SetWindowTheme(IntPtr window, string subAppName, string subIdList);
		[DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

		private static AllowDarkModeForWindowDelegate _allowDarkModeForWindow;
		private static readonly HashSet<Control> _hooked = new HashSet<Control>();

		/// <summary>True once Windows' app-wide dark mode took, so the DarkMode_* control themes draw dark.</summary>
		public static bool NativeDark { get; private set; }

		/// <summary>Put the process into Windows' dark mode and the ToolStrips onto the dark renderer. Call before any window.</summary>
		public static void EnableForApp()
		{
			ToolStripManager.Renderer = new DarkToolStripRenderer();

			// The opt-in is only reachable through uxtheme ordinals (as OpenCAGE's ThemeNative): 135 is SetPreferredAppMode
			// from 1903 (AllowDarkModeForApp before it), 133 AllowDarkModeForWindow, 104 RefreshImmersiveColorPolicyState,
			// 136 FlushMenuThemes. Best effort - on an older Windows the controls just keep their light native parts.
			try
			{
				int build = 0;
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
					if (key != null) int.TryParse(key.GetValue("CurrentBuildNumber") as string, out build);
				if (build < 17763)
					return;

				IntPtr uxtheme = LoadLibrary("uxtheme.dll");
				if (uxtheme == IntPtr.Zero)
					return;
				bool appDark = false;
				if (build >= 18334)
				{
					var setMode = Get<SetPreferredAppModeDelegate>(uxtheme, 135);
					if (setMode != null) { setMode(2); appDark = true; } //ForceDark
				}
				else
				{
					var allow = Get<AllowDarkModeForAppDelegate>(uxtheme, 135);
					if (allow != null) { allow(true); appDark = true; }
				}
				_allowDarkModeForWindow = Get<AllowDarkModeForWindowDelegate>(uxtheme, 133);
				Get<VoidDelegate>(uxtheme, 104)?.Invoke();
				Get<VoidDelegate>(uxtheme, 136)?.Invoke();
				NativeDark = appDark && _allowDarkModeForWindow != null;
			}
			catch { NativeDark = false; }
		}

		private static T Get<T>(IntPtr module, int ordinal) where T : class
		{
			IntPtr address = GetProcAddress(module, new IntPtr(ordinal));
			return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, typeof(T)) as T;
		}

		/// <summary>Theme a top-level window: its title bar, then everything in it.</summary>
		public static void ApplyWindow(Form form)
		{
			form.BackColor = Theme.Surface;
			form.ForeColor = Theme.Text;
			if (form.IsHandleCreated) DarkTitleBar(form.Handle);
			else form.HandleCreated += (s, e) => DarkTitleBar(((Form)s).Handle);
			Apply(form);
		}

		private static void DarkTitleBar(IntPtr handle)
		{
			try
			{
				if (_allowDarkModeForWindow != null) _allowDarkModeForWindow(handle, true);
				int on = 1;
				if (DwmSetWindowAttribute(handle, 20, ref on, sizeof(int)) != 0)
					DwmSetWindowAttribute(handle, 19, ref on, sizeof(int));
			}
			catch { }
		}

		/// <summary>The DockPanel draws its strips and captions from its skin rather than from control colours.</summary>
		public static void SkinDockPanel(DockPanel dockPanel)
		{
			dockPanel.DockBackColor = Theme.Window;
			DockPanelSkin skin = dockPanel.Skin;

			SetStrip(skin.AutoHideStripSkin.DockStripGradient, Theme.Surface);
			SetTab(skin.AutoHideStripSkin.TabGradient, Theme.Raised, Theme.TextDim);

			SetStrip(skin.DockPaneStripSkin.DocumentGradient.DockStripGradient, Theme.Surface);
			SetTab(skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient, Theme.Raised, Theme.Text);
			SetTab(skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient, Theme.Surface, Theme.TextDim);

			SetStrip(skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient, Theme.Surface);
			SetTab(skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient, Theme.Raised, Theme.Text);
			SetTab(skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient, Theme.Surface, Theme.TextDim);
			SetTab(skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient, Theme.Selection, Theme.Text);
			SetTab(skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient, Theme.Raised, Theme.Text);
		}

		private static void SetStrip(DockPanelGradient gradient, Color colour)
		{
			gradient.StartColor = colour;
			gradient.EndColor = colour;
		}

		private static void SetTab(TabGradient gradient, Color colour, Color text)
		{
			gradient.StartColor = colour;
			gradient.EndColor = colour;
			gradient.TextColor = text;
		}

		/// <summary>Theme a control and everything under it, now and whenever more is added.</summary>
		public static void Apply(Control control)
		{
			ApplyOne(control);
			foreach (Control child in control.Controls)
				Apply(child);

			if (_hooked.Add(control))
			{
				control.ControlAdded += (s, e) => Apply(e.Control);
				control.Disposed += (s, e) => _hooked.Remove((Control)s);
			}
		}

		private static bool IsLight(Color c) { return c.A > 0 && c.R + c.G + c.B > 3 * 150; }
		private static bool IsDarkText(Color c) { return c.R + c.G + c.B < 3 * 110; }

		private static void ApplyOne(Control c)
		{
			if (c is BehaviorTreeView)
			{
				c.BackColor = Theme.Canvas;
				return;
			}
			if (c is MdiClient)
			{
				c.BackColor = Theme.Window;
				return;
			}

			if (IsDarkText(c.ForeColor))
				c.ForeColor = Theme.Text;

			TreeView tree = c as TreeView;
			if (tree != null)
			{
				tree.BackColor = Theme.Window;
				tree.ForeColor = Theme.Text;
				tree.LineColor = Theme.TextDim;
				Native(tree, "DarkMode_Explorer");
				return;
			}

			TextBoxBase text = c as TextBoxBase;
			if (text != null)
			{
				text.BackColor = Theme.Input;
				text.ForeColor = Theme.Text;
				TextBox single = text as TextBox;
				if (single != null && single.BorderStyle == BorderStyle.Fixed3D)
					single.BorderStyle = BorderStyle.FixedSingle;
				if (text.Multiline) Native(text, "DarkMode_Explorer");
				return;
			}

			ComboBox combo = c as ComboBox;
			if (combo != null)
			{
				combo.BackColor = Theme.Input;
				combo.ForeColor = Theme.Text;
				if (NativeDark) Native(combo, "DarkMode_CFD");
				else combo.FlatStyle = FlatStyle.Flat;
				return;
			}

			UpDownBase upDown = c as UpDownBase;
			if (upDown != null)
			{
				upDown.BackColor = Theme.Input;
				upDown.ForeColor = Theme.Text;
				return;
			}

			Button button = c as Button;
			if (button != null)
			{
				button.FlatStyle = FlatStyle.Flat;
				button.UseVisualStyleBackColor = false;
				button.BackColor = Theme.Raised;
				button.ForeColor = Theme.Text;
				button.FlatAppearance.BorderColor = Theme.Border;
				button.FlatAppearance.MouseOverBackColor = Theme.Hover;
				button.FlatAppearance.MouseDownBackColor = Theme.Selection;
				return;
			}

			CheckBox check = c as CheckBox;
			if (check != null)
			{
				check.ForeColor = Theme.Text;
				Native(check, "DarkMode_Explorer");
				return;
			}

			ToolStrip strip = c as ToolStrip;
			if (strip != null)
			{
				strip.BackColor = Theme.Surface;
				strip.ForeColor = Theme.Text;
				foreach (ToolStripItem item in strip.Items)
					item.ForeColor = Theme.Text;
				return;
			}

			// containers, labels, the dock windows: the property grid's category headers are DarkGray, the rest light grey/white
			if (c.BackColor == Color.DarkGray)
				c.BackColor = Theme.Raised;
			else if (IsLight(c.BackColor))
				c.BackColor = c is Label && c.Parent != null ? c.Parent.BackColor : Theme.Surface;

			ScrollableControl scroll = c as ScrollableControl;
			if (scroll != null && scroll.AutoScroll)
				Native(c, "DarkMode_Explorer");
		}

		private static void Native(Control c, string theme)
		{
			if (!NativeDark) return;
			if (c.IsHandleCreated) SetWindowTheme(c.Handle, theme, null);
			else c.HandleCreated += (s, e) => SetWindowTheme(((Control)s).Handle, theme, null);
		}

		/// <summary>ToolStrips and menus in the palette.</summary>
		private class DarkToolStripRenderer : ToolStripProfessionalRenderer
		{
			public DarkToolStripRenderer() : base(new DarkColours()) { RoundedEdges = false; }

			protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
			{
				e.TextColor = e.Item.Enabled ? Theme.Text : Theme.TextDim;
				base.OnRenderItemText(e);
			}

			protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
			{
				e.ArrowColor = Theme.Text;
				base.OnRenderArrow(e);
			}
		}

		private class DarkColours : ProfessionalColorTable
		{
			public override Color ToolStripGradientBegin { get { return Theme.Surface; } }
			public override Color ToolStripGradientMiddle { get { return Theme.Surface; } }
			public override Color ToolStripGradientEnd { get { return Theme.Surface; } }
			public override Color ToolStripBorder { get { return Theme.Surface; } }
			public override Color ToolStripDropDownBackground { get { return Theme.Raised; } }
			public override Color ToolStripContentPanelGradientBegin { get { return Theme.Surface; } }
			public override Color ToolStripContentPanelGradientEnd { get { return Theme.Surface; } }
			public override Color MenuStripGradientBegin { get { return Theme.Surface; } }
			public override Color MenuStripGradientEnd { get { return Theme.Surface; } }
			public override Color MenuBorder { get { return Theme.Border; } }
			public override Color MenuItemBorder { get { return Theme.Border; } }
			public override Color MenuItemSelected { get { return Theme.Hover; } }
			public override Color MenuItemSelectedGradientBegin { get { return Theme.Hover; } }
			public override Color MenuItemSelectedGradientEnd { get { return Theme.Hover; } }
			public override Color MenuItemPressedGradientBegin { get { return Theme.Raised; } }
			public override Color MenuItemPressedGradientMiddle { get { return Theme.Raised; } }
			public override Color MenuItemPressedGradientEnd { get { return Theme.Raised; } }
			public override Color ImageMarginGradientBegin { get { return Theme.Raised; } }
			public override Color ImageMarginGradientMiddle { get { return Theme.Raised; } }
			public override Color ImageMarginGradientEnd { get { return Theme.Raised; } }
			public override Color ButtonSelectedHighlight { get { return Theme.Hover; } }
			public override Color ButtonSelectedHighlightBorder { get { return Theme.Border; } }
			public override Color ButtonSelectedBorder { get { return Theme.Border; } }
			public override Color ButtonSelectedGradientBegin { get { return Theme.Hover; } }
			public override Color ButtonSelectedGradientMiddle { get { return Theme.Hover; } }
			public override Color ButtonSelectedGradientEnd { get { return Theme.Hover; } }
			public override Color ButtonPressedGradientBegin { get { return Theme.Selection; } }
			public override Color ButtonPressedGradientMiddle { get { return Theme.Selection; } }
			public override Color ButtonPressedGradientEnd { get { return Theme.Selection; } }
			public override Color ButtonCheckedGradientBegin { get { return Theme.Selection; } }
			public override Color ButtonCheckedGradientMiddle { get { return Theme.Selection; } }
			public override Color ButtonCheckedGradientEnd { get { return Theme.Selection; } }
			public override Color SeparatorDark { get { return Theme.Border; } }
			public override Color SeparatorLight { get { return Theme.Surface; } }
			public override Color GripDark { get { return Theme.Border; } }
			public override Color GripLight { get { return Theme.Surface; } }
			public override Color OverflowButtonGradientBegin { get { return Theme.Surface; } }
			public override Color OverflowButtonGradientMiddle { get { return Theme.Surface; } }
			public override Color OverflowButtonGradientEnd { get { return Theme.Surface; } }
		}
	}
}
