using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Brainiac.Design
{
    /// <summary>
    /// Asks for a new behaviour tree's name and what to start it from.
    /// </summary>
    internal class NewBehaviorTreeDialog : Form
    {
        /// <summary>The "start from" entry for a tree with nothing in it.</summary>
        public const string EmptyTree = "(an empty tree)";

        private readonly string _folder;
        private readonly TextBox _name = new TextBox();
        private readonly ComboBox _source = new ComboBox();
        private readonly Label _error = new Label();
        private readonly Button _ok = new Button();

        /// <summary>The name chosen.</summary>
        public string TreeName => _name.Text.Trim();

        /// <summary>The tree to copy, or null to start from an empty one.</summary>
        public string SourceTree => (string)_source.SelectedItem == EmptyTree ? null : (string)_source.SelectedItem;

        public NewBehaviorTreeDialog(string folder, IList<string> trees, string startFrom)
        {
            _folder = folder;

            Text = "New Behaviour Tree";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 250);

            Label nameLabel = new Label() { Text = "Name:", Location = new Point(12, 15), AutoSize = true };
            _name.Location = new Point(100, 12);
            _name.Width = 308;
            _name.TextChanged += (s, e) => UpdateValidation();

            Label sourceLabel = new Label() { Text = "Start from:", Location = new Point(12, 44), AutoSize = true };
            _source.Location = new Point(100, 41);
            _source.Width = 308;
            _source.DropDownStyle = ComboBoxStyle.DropDownList;
            _source.Items.Add(EmptyTree);
            foreach (string tree in trees)
                _source.Items.Add(tree);
            _source.SelectedItem = trees.Contains(startFrom) ? startFrom : EmptyTree;

            //Retail's root trees all start with the branches the game drives directly (suspend for load/save, death,
            //scripted control), which NoBehaviour is little more than: an empty tree is for building a subtree
            Label hint = new Label()
            {
                Text = "A copy of NoBehaviour has the branches every character's main tree needs (saving, death, scripted control). " +
                       "Start from an empty tree to build a subtree for other trees to reference (the game can't load it as a main tree until it has nodes).\n\n" +
                       "To give characters the new tree, choose it as a class's Behaviour Tree in OpenCAGE's Character Attributes " +
                       "editor. Each level picks up a newly assigned tree when the level is saved.",
                Location = new Point(12, 72),
                Size = new Size(396, 118),
            };

            _error.Location = new Point(12, 192);
            _error.Size = new Size(396, 18);
            _error.ForeColor = Color.Firebrick;

            _ok.Text = "Create";
            _ok.Location = new Point(252, 216);
            _ok.DialogResult = DialogResult.OK;
            Button cancel = new Button() { Text = "Cancel", Location = new Point(333, 216), DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] { nameLabel, _name, sourceLabel, _source, hint, _error, _ok, cancel });
            AcceptButton = _ok;
            CancelButton = cancel;

            UpdateValidation();
        }

        private void UpdateValidation()
        {
            string error = BehaviorTreeFiles.ValidateName(TreeName, _folder);
            //Nothing typed yet is not worth a red message
            _error.Text = TreeName.Length == 0 ? "" : error ?? "";
            _ok.Enabled = error == null;
        }
    }
}
