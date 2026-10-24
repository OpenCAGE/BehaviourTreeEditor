////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2009, Daniel Kollmann
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted
// provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice, this list of conditions
//   and the following disclaimer.
//
// - Redistributions in binary form must reproduce the above copyright notice, this list of
//   conditions and the following disclaimer in the documentation and/or other materials provided
//   with the distribution.
//
// - Neither the name of Daniel Kollmann nor the names of its contributors may be used to endorse
//   or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Brainiac.Design.Nodes;
using Brainiac.Design.Properties;
using System.Xml;
using CATHODE;

namespace Brainiac.Design
{
	/// <summary>
	/// This control manages all the behaviours which are loaded and saved.
	/// </summary>
	internal partial class BehaviorTreeList : UserControl, BehaviorManagerInterface, CurrentAITypeProvider
	{
		public BehaviorTreeList()
		{
			InitializeComponent();

			BehaviorManager.Instance= this;
			AIType.SetProvider(this);

			// right-clicking a tree offers what the toolbar does, and selects it first so the menu acts on what was clicked
			ContextMenuStrip menu= new ContextMenuStrip();
			menu.Items.Add("New Behaviour Tree...", newBehaviorButton.Image, newBehaviorButton_Click);
			menu.Items.Add("Rename", null, (s, e) => { if(behaviourTreeView.SelectedNode !=null) behaviourTreeView.SelectedNode.BeginEdit(); });
			menu.Items.Add("Delete", deleteButton.Image, deleteButton_Click);
			behaviourTreeView.ContextMenuStrip= menu;
			behaviourTreeView.NodeMouseClick+= (s, e) => { if(e.Button ==MouseButtons.Right) behaviourTreeView.SelectedNode= e.Node; };
		}

		/// <summary>
		/// All the file managers which were loaded.
		/// </summary>
		private List<FileManagerInfo> _fileManagers= new List<FileManagerInfo>();

		/// <summary>
		/// All the exporters which were loaded.
		/// </summary>
		private List<ExporterInfo> _exporters= new List<ExporterInfo>();

		/// <summary>
		/// All the AI types which were registered.
		/// </summary>
		private List<AIType> _aiTypes= new List<AIType>();

		private AIType _currentAIType= null;

		/// <summary>
		/// The currently selected AI type.
		/// </summary>
		public AIType CurrentAIType
		{
			get { return _currentAIType; }
		}

		/// <summary>
		/// Loads all plugins form a directory.
		/// </summary>
		/// <param name="dll">plugin dll name.</param>
		internal void LoadPlugins(string dll)
		{
			// load file
			Assembly assembly= Assembly.LoadFile( Path.GetFullPath(dll) );

			// create an instance of the plugin class of the same name as the file
			string classname= Path.GetFileNameWithoutExtension(dll);
			Plugin plugin= (Plugin) assembly.CreateInstance(classname +'.'+ classname);

			if(plugin ==null)
				throw new Exception( string.Format(Properties.Resources.ExceptionCouldNotLoadPlugin, classname +'.'+ classname) );

			// register the plugin
			Plugin.AddLoadedPlugin(assembly);

			// add all file managers, exporters and AI types
			_fileManagers.AddRange(plugin.FileManagers);
			_exporters.AddRange(plugin.Exporters);
			_aiTypes.AddRange(plugin.AITypes);

			// create the tree nodes
			foreach(NodeGroup group in plugin.NodeGroups)
				group.Register(nodeTreeView.Nodes);
		}

		/// <summary>
		/// Return true when there are any file managers loaded.
		/// </summary>
		/// <returns></returns>
		internal bool HasFileManagers()
		{
			return _fileManagers.Count >0;
		}

		/// <summary>
		/// Returns true if there are any exporters loaded.
		/// </summary>
		/// <returns></returns>
		internal bool HasExporters()
		{
			return _exporters.Count >0;
		}

		private List<BehaviorNode> _newBehaviors= new List<BehaviorNode>();

		/// <summary>
		/// The list of all newly created behaviours which are not yet saved.
		/// </summary>
		public IList<BehaviorNode> NewBehaviors
		{
			get { return _newBehaviors.AsReadOnly(); }
		}

		private List<BehaviorNode> _loadedBehaviors= new List<BehaviorNode>();

		/// <summary>
		/// The list of all loaded behaviours.
		/// </summary>
		public IList<BehaviorNode> LoadedBehaviors
		{
			get { return _loadedBehaviors.AsReadOnly(); }
		}

		/// <summary>
		/// Returns the behaviour which is associated with a tree node.
		/// </summary>
		/// <param name="nodetag">The NodeTag of the tree node.</param>
		/// <param name="label">The label of the tree node.</param>
		/// <returns>Returns null if no matching behaviour could be found or loaded.</returns>
		internal BehaviorNode GetBehavior(NodeTag nodetag, string label)
		{
			// search the loaded behaviours if we can find one for this tree node
			foreach(BehaviorNode node in _loadedBehaviors)
			{
				if(node.FileManager.Filename ==nodetag.Filename)
					return node;
			}

			// search if we have any newly created behaviour with a matching name
			foreach(BehaviorNode node in _newBehaviors)
			{
				if(((Node)node).Label ==label)
					return node;
			}

			// not found, so we try to load the behaviour
			BehaviorNode behavior= LoadBehavior(nodetag.Filename);

			// if that was sucessful return the loaded behaviour
			if(behavior !=null)
				return behavior;

			MessageBox.Show("No such behavior: "+ label +" "+ nodetag.Filename, "File Error", MessageBoxButtons.OK);
			return null;
		}

		private string _behaviorFolder= string.Empty;

		/// <summary>
		/// The folder which is searched for behaviours.
		/// </summary>
		public string BehaviorFolder
		{
			get { return _behaviorFolder; }
			set { ChangeBehaviorFolder(value); }
		}

		/// <summary>
		/// Returns a behaviour group with a specific name. If none exists, it is created.
		/// </summary>
		/// <param name="list">The list which is earched for this behaviour tree node.</param>
		/// <param name="name">The name of the node you want to find or create.</param>
		/// <param name="root">The path of the directory this behaviour folder represents.</param>
		/// <returns></returns>
		private TreeNode GetBehaviorGroup(TreeNodeCollection list, string name, string root)
		{
			// search for an existing behaviour group
			foreach(TreeNode node in list)
			{
				if(node.Text ==name)
					return node;
			}

			// create a new group
			TreeNode newnode= new TreeNode(name, (int) NodeIcon.FolderClosed, (int) NodeIcon.FolderClosed);
			newnode.Tag= new NodeTag(NodeTagType.BehaviorFolder, null, root);
			list.Add(newnode);

			return newnode;
		}

		/// <summary>
		/// Creates a behaviour group for a given folder or file.
		/// </summary>
		/// <param name="filename">The file or folder you want to create the behaviour group for.</param>
		/// <param name="isFile">Determines if filename is a folder or a file.</param>
		/// <returns>Returns null if a tree node could not be created.</returns>
		private TreeNode GetBehaviorGroup(string filename, bool isFile)
		{
			// separate filename and behaviour folder
			string name= filename.Substring(_behaviorFolder.Length +1);
			string folder= _behaviorFolder;

			// get the different groups
			string[] groups= name.Split('\\');
			if(isFile && groups.Length <2)  // if this is a file with no subfolder, no group needs to be created
				return null;

			// get the group for the behaviours
			TreeNodeCollection list= GetBehaviorGroup(behaviourTreeView.Nodes, "Behaviours", _behaviorFolder).Nodes;

			TreeNode group= null;
			int count= isFile ? groups.Length -1 : groups.Length;  // if this is a file, skip the filename

			// create a tree node for each folder
			for(int i= 0; i <count; ++i)
			{
				// update the directory
				folder+= '\\'+ groups[i];

				// create the behaviour group
				group= GetBehaviorGroup(list, groups[i], folder);
				if(group ==null)
					return null;
				else list= group.Nodes;
			}

			return group;
		}

		/// <summary>
		/// Returns the tree node of a behaviour. For internal use only.
		/// </summary>
		/// <param name="node">The node you want to search for the node of a behaviour.</param>
		/// <param name="identifier">The label or filename of the behaviour you are looking for.</param>
		/// <param name="isFilename">Determines if the identifier is a filename or a label.</param>
		/// <returns>Returns null if no tree node for the behaviour could be found.</returns>
		private TreeNode GetBehaviorNode(TreeNode node, string identifier, bool isFilename)
		{
			NodeTag nodetag= (NodeTag) node.Tag;
			if(nodetag.Type ==NodeTagType.Behavior)
			{
				if(isFilename)
				{
					if(identifier ==nodetag.Filename)
						return node;
				}
				else
				{
					if(identifier ==node.Text)
						return node;
				}
			}

			foreach(TreeNode child in node.Nodes)
			{
				TreeNode res= GetBehaviorNode(child, identifier, isFilename);
				if(res !=null)
					return res;
			}

			return null;
		}

		/// <summary>
		/// Returns the tree node for a behaviour.
		/// </summary>
		/// <param name="identifier">The label or filename of the behaviour you are looking for.</param>
		/// <param name="isFilename">Determines if the identifier is a filename or a label.</param>
		/// <returns>Returns null if no tree node for the behaviour could be found.</returns>
		private TreeNode GetBehaviorNode(string identifier, bool isFilename)
		{
			if(identifier ==string.Empty)
				return null;

			TreeNode behaviors= GetBehaviorGroup(behaviourTreeView.Nodes, "Behaviours", _behaviorFolder);
			return GetBehaviorNode(behaviors, identifier, isFilename);
		}

		/// <summary>
		/// Generates a new list of the behaviours.
		/// </summary>
		internal void RebuildBehaviorList()
		{
			// check if we have a valid folder
			if(_behaviorFolder ==string.Empty)
				return;

			// create the folder if it does not exist
			if(!Directory.Exists(_behaviorFolder))
				Directory.CreateDirectory(_behaviorFolder);

			// get the group for the behaviour and clear it from the old ones.
			TreeNode behaviorTreeNode= GetBehaviorGroup(behaviourTreeView.Nodes, "Behaviours", _behaviorFolder);
			TreeNodeCollection behaviors= behaviorTreeNode.Nodes;
			behaviors.Clear();

			// add all the newly created behaviours
			foreach(BehaviorNode node in _newBehaviors)
			{
				// create the tree node
				TreeNode behaviorNode= new TreeNode(((Node)node).Label, (int) NodeIcon.BehaviorModified, (int) NodeIcon.BehaviorModified);
				behaviorNode.Tag= new NodeTag(NodeTagType.Behavior, node.GetType(), string.Empty);

				behaviors.Add(behaviorNode);
			}

			// search all folders for behaviours which must be added to the tree
			IList<string> foundFiles, foundFolders;
			FileManagers.FileManager.CollectBehaviors(_fileManagers, _behaviorFolder, out foundFiles, out foundFolders);

			for(int f= 0; f <foundFiles.Count; ++f)
			{
				// determine correct icon
				int iconIndex= GetBehavior(foundFiles[f]) ==null ? (int)NodeIcon.Behavior : (int)NodeIcon.BehaviorLoaded;

				// create the tree node
				TreeNode behaviorNode= new TreeNode(Path.GetFileNameWithoutExtension(foundFiles[f]), iconIndex, iconIndex);
				behaviorNode.Tag= new NodeTag(NodeTagType.Behavior, typeof(Behavior), foundFiles[f]);

				// add the tree node to the correct group. If none can be found add it to the root.
				TreeNode group= GetBehaviorGroup(foundFiles[f], true);
				if(group ==null)
					behaviors.Add(behaviorNode);
				else group.Nodes.Add(behaviorNode);
			}

			// sort the tree. We use a delegate to avoid a bug in .NET
			BeginInvoke(new SortTreeDelegate(SortTree));

			// expand the behaviours
			behaviorTreeNode.ExpandAll();
		}

		/// <summary>
		/// Adds all tree nodes in the pool and its children to a list.
		/// </summary>
		/// <param name="pool">The pool you want to search.</param>
		/// <param name="list">The list the tree nodes are added to.</param>
		private void AddChildNodes(TreeNodeCollection pool, List<TreeNode> list)
		{
			foreach(TreeNode node in pool)
			{
				list.Add(node);

				AddChildNodes(node.Nodes, list);
			}
		}

		/// <summary>
		/// Adds all tree nodes in the pool and its children of a specific type to a list.
		/// </summary>
		/// <param name="pool">The pool you want to search.</param>
		/// <param name="list">The list the tree nodes are added to.</param>
		/// <param name="type">The type of nodes you want to get.</param>
		private void AddChildNodes(TreeNodeCollection pool, List<TreeNode> list, NodeTagType type)
		{
			foreach(TreeNode node in pool)
			{
				if(((NodeTag)node.Tag).Type ==type)
					list.Add(node);

				AddChildNodes(node.Nodes, list, type);
			}
		}

		/// <summary>
		/// Generates a label with a unique name.
		/// </summary>
		/// <param name="label">The name you want to generate a unique one from.</param>
		/// <param name="start">The first number you want to be added to the name.</param>
		/// <param name="used">Returns the number being added to the name.</param>
		/// <returns>Returns the unique name.</returns>
		private string GetUniqueLabel(string label, int start, out int used)
		{
			int i= start;

			// gather all tree nodes
			List<TreeNode> nodes= new List<TreeNode>();
			AddChildNodes(behaviourTreeView.Nodes, nodes);
			AddChildNodes(nodeTreeView.Nodes, nodes);

			// if there is no tree node, simply output the first available name
			if(nodes.Count <1)
			{
				used= i;
				return string.Format("{0} {1}", label, i);
			}

			do
			{
				// generate the new label
				string newlabel= string.Format("{0} {1}", label, i);

				// check if there is any node with this name
				bool found= false;
				foreach(TreeNode node in nodes)
				{
					if(node.Text ==newlabel)
					{
						found= true;
						break;
					}
				}

				// if no node with the name exists, return it.
				if(!found)
				{
					used= i;
					return newlabel;
				}

				i++;
			}
			while(true);
		}

		public delegate BehaviorTreeViewDock ShowBehaviorEventDelegate(Nodes.BehaviorNode node);

		/// <summary>
		/// This event is triggered when double-clicking a behaviour in the node explorer.
		/// </summary>
		public event ShowBehaviorEventDelegate ShowBehavior;

		/// <summary>
		/// Returns a behaviour which has already been loaded.
		/// </summary>
		/// <param name="filename">Behaviour file to get the behaviour for.</param>
		/// <returns>Returns null if the behaviour is not loaded.</returns>
		public BehaviorNode GetBehavior(string filename)
		{
			foreach(BehaviorNode node in _loadedBehaviors)
			{
				if(node.FileManager.Filename ==filename)
					return node;
			}

			return null;
		}

		/// <summary>
		/// Loads the given behaviour. If the behaviour was already loaded, it is not loaded a second time.
		/// </summary>
		/// <param name="filename">Behaviour file to load.</param>
		/// <returns>Returns null if the behaviour was not already loaded and could not be loaded.</returns>
		public BehaviorNode LoadBehavior(string filename)
		{
			// check if the behaviour was already loaded.
			BehaviorNode behavior= GetBehavior(filename);
			if(behavior !=null)
				return behavior;

			FileManagers.FileManager filemanager= null;

			// search the file managers for the right one to load the given file
			foreach(FileManagerInfo info in _fileManagers)
			{
				// check if the file manager could handle this file extension
				if(filename.ToLowerInvariant().EndsWith(info.FileExtension))
				{
					try
					{
						// check if the file exists
						if(!File.Exists(filename))
							throw new Exception( string.Format(Properties.Resources.ExceptionNoSuchFile, filename) );

						// create a new file manager and load the behaviour
						filemanager= info.Create(filename, null);
						filemanager.Load();
						behavior= filemanager.Node;

						// register the WasSaved and WasModified events on the behaviour
						behavior.WasSaved+= new Behavior.WasSavedEventDelegate(behavior_WasSaved);
						((Node)behavior).WasModified+= new Node.WasModifiedEventDelegate(behavior_WasModified);

						// assign the label of the behaviour node
						((Node)behavior).Label= Path.GetFileNameWithoutExtension(filename);

						// add this behaviour to the loaded ones
						_loadedBehaviors.Add(behavior);

						// get the correct tree node
						TreeNode tnode= null;
						if(behavior.FileManager ==null)
						{
							tnode= GetBehaviorNode(((Node)behavior).Label, false);
						}
						else
						{
							tnode= GetBehaviorNode(behavior.FileManager.Filename, true);
						}

						// change the behaviours icon to the loaded icon
						if(tnode !=null)
						{
							tnode.ImageIndex= (int) NodeIcon.BehaviorLoaded;
							tnode.SelectedImageIndex= (int) NodeIcon.BehaviorLoaded;

							NodeTag tag= (NodeTag)tnode.Tag;
							tag.AssignLoadedBehavior(behavior);
						}
					}
					catch(Exception ex)
					{
						MessageBox.Show(ex.Message, "Load Error", MessageBoxButtons.OK);
					}
					break;
				}
			}

			return behavior;
		}

		/// <summary>
		/// Handles when a tree node in the node explorer is double-clicked.
		/// </summary>
		private void behaviorTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			NodeTag nodetag= (NodeTag) e.Node.Tag;

			// if the double-clicked node was not a behaviour, there is nothing we must do.
			if(nodetag.Type !=NodeTagType.Behavior)
				return;

			// reset any previously loaded behaviour
			FileManagers.FileManager.ResetLoadedBehavior();

			// get or load the behaviour
			BehaviorNode behavior= GetBehavior(nodetag, e.Node.Text);
			if(behavior ==null)
				behavior= LoadBehavior(nodetag.Filename);

			// trigger the ShowBehavior event
			if(behavior !=null && ShowBehavior !=null)
				ShowBehavior(behavior);
		}

		/// <summary>
		/// Handles when the new behaviour button is clicked. The tree is made on disk straight away, under the name asked
		/// for, and compiled into the game's trees so OpenCAGE's character attributes editor can offer it.
		/// </summary>
		private void newBehaviorButton_Click(object sender, EventArgs e)
		{
			// a rename still being typed would otherwise finish inside the dialog, under a list of trees taken before it
			EndLabelEdit();
			if(!CanCompile("create a tree"))
				return;

			List<string> trees= new List<string>();
			foreach(string file in BehaviorTreeFiles.TreeFiles(_behaviorFolder))
				trees.Add(Path.GetFileNameWithoutExtension(file));

			// start from the tree selected in the list, if there is one, else the minimal main tree
			string startFrom= "NoBehaviour";
			NodeTag selected= behaviourTreeView.SelectedNode ==null ? null : behaviourTreeView.SelectedNode.Tag as NodeTag;
			if(selected !=null && selected.Type ==NodeTagType.Behavior && selected.Filename !=string.Empty)
				startFrom= Path.GetFileNameWithoutExtension(selected.Filename);

			string name, source;
			using(NewBehaviorTreeDialog dialog= new NewBehaviorTreeDialog(_behaviorFolder, trees, startFrom))
			{
				if(dialog.ShowDialog(this) !=DialogResult.OK)
					return;
				name= dialog.TreeName;
				source= dialog.SourceTree;
			}

			string filename= Path.Combine(_behaviorFolder, name + ".xml");
			try
			{
				if(source !=null)
				{
					// a copy of the tree as last saved
					File.Copy(Path.Combine(_behaviorFolder, source + ".xml"), filename);
				}
				else
				{
					// an empty tree, written by the same file manager as every other
					Nodes.Behavior node= new Nodes.Behavior(name);
					FileManagerInfo xml= _fileManagers.Find(o => o.FileExtension ==".xml");
					node.FileManager= xml.Create(filename, node);
					node.FileManager.Save();
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Could not create the tree", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// the folder and the game's trees stay in step: a tree the game didn't get isn't kept either
			if(!CompileBehaviors())
			{
				File.Delete(filename);
				CompileBehaviors(false);
				return;
			}
			RebuildBehaviorList();

			// open it the way double-clicking does, without a reference to the last tree opened looking circular
			FileManagers.FileManager.ResetLoadedBehavior();
			BehaviorNode behavior= LoadBehavior(filename);
			if(behavior !=null && ShowBehavior !=null)
				ShowBehavior(behavior);
		}

		/// <summary>
		/// Written into the behaviour folder while trees there couldn't be compiled into the game's, so the next start
		/// doesn't replace them with the game's copy without asking. Not a tree: compiling ignores it.
		/// </summary>
		internal static string NotWrittenMarker
		{
			get { return Path.Combine(SharedData.pathToXMLs, "NOT_WRITTEN_TO_GAME.txt"); }
		}

		/// <summary>
		/// Compiles every tree into the game's _DIRECTORY_CONTENTS.BML, which is what the game and OpenCAGE read. If a tree
		/// is broken nothing is written, and (unless told not to) the user is told which.
		/// </summary>
		internal bool CompileBehaviors(bool report= true)
		{
			List<string> problems= BehaviorTreeFiles.Compile(SharedData.pathToXMLs, SharedData.pathToBML);
			try
			{
				if(problems.Count ==0)
					File.Delete(NotWrittenMarker);
				else
					File.WriteAllText(NotWrittenMarker, "These behaviour trees could not be written to the game's DATA/BINARY_BEHAVIOR/_DIRECTORY_CONTENTS.BML:\r\n\r\n" + string.Join("\r\n", problems.ToArray()) + "\r\n");
			}
			catch(Exception)
			{
				// the marker only protects unsaved work on the next start
			}
			if(problems.Count ==0)
				return true;

			if(report)
			{
				MessageBox.Show("The behaviour trees were not written to the game:\n\n" + string.Join("\n", problems.ToArray()) +
					"\n\nFix or remove " + (problems.Count ==1 ? "that file" : "those files") + " in " + Path.GetFullPath(SharedData.pathToXMLs) + ", then save again.",
					"Behaviour trees not written", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			return false;
		}

		/// <summary>
		/// Whether the trees compile now, before creating, renaming or deleting one: if they don't, nothing reaches the game,
		/// so the change isn't made and the user is told why.
		/// </summary>
		private bool CanCompile(string action)
		{
			List<string> problems= BehaviorTreeFiles.Check(SharedData.pathToXMLs);
			if(problems.Count ==0)
				return true;

			MessageBox.Show("Can't " + action + " until the behaviour trees can be written to the game:\n\n" + string.Join("\n", problems.ToArray()) +
				"\n\nFix or remove " + (problems.Count ==1 ? "that file" : "those files") + " in " + Path.GetFullPath(SharedData.pathToXMLs) + ".",
				"Behaviour trees not written", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}

		/// <summary>
		/// Finishes a rename being typed in the tree list, so it happens before (not inside) whatever runs next.
		/// </summary>
		private void EndLabelEdit()
		{
			if(behaviourTreeView.SelectedNode !=null && behaviourTreeView.SelectedNode.IsEditing)
				behaviourTreeView.SelectedNode.EndEdit(false);
		}

		/// <summary>
		/// Open trees with unsaved changes that reference the tree called <paramref name="name"/> - which the files on disk
		/// don't show yet.
		/// </summary>
		private List<string> UnsavedReferencesTo(string name)
		{
			List<string> users= new List<string>();
			foreach(BehaviorNode behavior in _loadedBehaviors)
			{
				if(behavior.IsModified && References((Node)behavior, name))
					users.Add("the unsaved tree " + ((Node)behavior).Label);
			}
			return users;
		}

		private static bool References(Node node, string name)
		{
			foreach(Node child in node.Children)
			{
				ReferencedBehavior reference= child as ReferencedBehavior;
				if(reference !=null)
				{
					if(string.Equals(Path.GetFileNameWithoutExtension(reference.ReferenceFilename ?? string.Empty), name, StringComparison.OrdinalIgnoreCase))
						return true;

					// what it shows beneath it is the referenced tree's own
					continue;
				}
				if(References(child, name))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Handles when the refresh list button is clicked.
		/// </summary>
		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RebuildBehaviorList();
		}

		public delegate void BehaviorRenamedEventDelegate(BehaviorNode node);

		/// <summary>
		/// This event is triggered when a behaviour was renamed.
		/// </summary>
		public event BehaviorRenamedEventDelegate BehaviorRenamed;

		public delegate void ClearBehaviorsEventDelegate(List<BehaviorNode> nodes, out bool[] result, out bool error);

		/// <summary>
		/// This event is triggered when all created and loaded behaviour trees must be closed.
		/// </summary>
		public event ClearBehaviorsEventDelegate ClearBehaviors;

		/// <summary>
		/// Changes the currently selected behaviour folder to another one.
		/// </summary>
		/// <param name="folder">The new behaviour folder.</param>
		private void ChangeBehaviorFolder(string folder)
		{
			// assign the new folder
			_behaviorFolder= folder ==string.Empty ? string.Empty : Path.GetFullPath(folder);
			FileManagers.FileManager.BehaviorFolder= _behaviorFolder;

			// check if we can clear all behaviours
			if(ClearBehaviors !=null)
			{
				// add all new behaviours to the list of behaviours which need to be saved
				List<BehaviorNode> behaviorsToSave= new List<BehaviorNode>();
				behaviorsToSave.AddRange(_newBehaviors);

				// add any modified behaviour to that list as well
				foreach(BehaviorNode node in _loadedBehaviors)
				{
					if(node.IsModified)
						behaviorsToSave.Add(node);
				}

				// clear all of the behaviours
				bool error;
				bool[] result;
				ClearBehaviors(behaviorsToSave, out result, out error);

				// if all behaviours could be saved or discarded, clear the loaded behaviours and rebuild the list for the new folder
				if(!error)
				{
					_loadedBehaviors.Clear();
					RebuildBehaviorList();
				}
			}
		}

		// this is a hack to allow sorting the treeView from inside AfterLabelEdit
		private delegate void SortTreeDelegate();
		private void SortTree()
		{
			behaviourTreeView.Sort();
			nodeTreeView.Sort();
        }

		/// <summary>
		/// Handles when a tree node is dragged from the node explorer
		/// </summary>
		private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			TreeNode node= (TreeNode) e.Item;
			NodeTag nodetag= (NodeTag) node.Tag;

			// we can only move behaviours which are saved, behaviour sub-folders and nodes
			if( nodetag.Type ==NodeTagType.Behavior && nodetag.Filename !=string.Empty ||
				nodetag.Type ==NodeTagType.BehaviorFolder && node.Parent !=null ||
				nodetag.Type ==NodeTagType.Node ||
				nodetag.Type ==NodeTagType.Event )
			{
				DoDragDrop(e.Item, DragDropEffects.Move);
			}
		}

		/// <summary>
		/// Handles when a tree node is dropped on the node explorer
		/// </summary>
		private void treeView_DragDrop(object sender, DragEventArgs e)
		{
			// check if a tree node was dropped
			if(e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
			{
				TreeView view= (TreeView) sender;

				// get the tree node the other node was dropped on
				Point pt= view.PointToClient(new Point(e.X, e.Y));
				TreeNode targetNode= view.GetNodeAt(pt);
				NodeTag targetNodeTag= (NodeTag) targetNode.Tag;

				TreeNode sourceNode= (TreeNode) e.Data.GetData("System.Windows.Forms.TreeNode");
				NodeTag sourceNodeTag= (NodeTag)sourceNode.Tag;

				// if the tree node was dragged in the same node explorer and
				// we drop a behaviour or a sub-folder
				// on another behaviour or folder, we continue
				if( targetNode.TreeView ==sourceNode.TreeView &&
					(sourceNodeTag.Type ==NodeTagType.Behavior || (sourceNodeTag.Type ==NodeTagType.BehaviorFolder && sourceNode.Parent !=null)) &&
					(targetNodeTag.Type ==NodeTagType.Behavior || targetNodeTag.Type ==NodeTagType.BehaviorFolder) )
				{
					// if we dropped on a behaviour, we use its parent instead
					if(targetNodeTag.Type ==NodeTagType.Behavior)
					{
						targetNode= targetNode.Parent;
						targetNodeTag= (NodeTag) targetNode.Tag;
					}

					// only trees directly in the behaviour folder are compiled into the game's: moving one into (or out of) a
					// subfolder would silently add it to or drop it from the game, whatever uses it
					if(!string.Equals(Path.GetFullPath(targetNodeTag.Filename).TrimEnd('\\'), Path.GetFullPath(_behaviorFolder).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ||
						!string.Equals(Path.GetFullPath(Path.GetDirectoryName(sourceNodeTag.Filename)).TrimEnd('\\'), Path.GetFullPath(_behaviorFolder).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
						return;

					try
					{
						// generate the new filename
						string targetfile= targetNodeTag.Filename +'\\'+ Path.GetFileName(sourceNodeTag.Filename);

						// check if the target file is different from the source file
						if(sourceNodeTag.Filename !=targetfile)
						{
							// if the dropped item was a behaviour
							if(sourceNodeTag.Type ==NodeTagType.Behavior)
							{
								// move the file
								File.Move(sourceNodeTag.Filename, targetfile);

								// and update its file manager
								BehaviorNode node= GetBehavior(sourceNodeTag.Filename);
								node.FileManager.Filename= targetfile;

								// update the tree node's filename
								sourceNodeTag.Filename= targetfile;

								// move the tree node in the explorer
								sourceNode.Remove();
								targetNode.Nodes.Add(sourceNode);
								targetNode.Expand();

								// sort the tree
								targetNode.TreeView.Sort();
							}
							else
							{
								// if it is a folder, move it
								Directory.Move(sourceNodeTag.Filename, targetfile);

								// update the filename of the already loaded behaviours
								foreach(BehaviorNode node in _loadedBehaviors)
								{
									if(node.FileManager.Filename.StartsWith(sourceNodeTag.Filename))
										node.FileManager.Filename= targetfile + node.FileManager.Filename.Substring(sourceNodeTag.Filename.Length);
								}

								// rebuild the behaviour list to update the tree nodes
								RebuildBehaviorList();
							}
						}
					}
					catch(Exception ex)
					{
						MessageBox.Show(ex.Message, "File Error", MessageBoxButtons.OK);

						RebuildBehaviorList();
					}
				}
			}
		}

		/// <summary>
		/// Handles when a tree node is dragged over the node explorer
		/// </summary>
		private void treeView_DragOver(object sender, DragEventArgs e)
		{
			e.Effect= DragDropEffects.None;

			// if it is a tree node
			if(e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
			{
				TreeView view= (TreeView) sender;

				// get the tree node we are over
				Point pt= view.PointToClient(new Point(e.X, e.Y));
				TreeNode targetNode= view.GetNodeAt(pt);
				if(targetNode !=null)
				{
					NodeTag targetNodeTag= (NodeTag) targetNode.Tag;

					TreeNode sourceNode= (TreeNode) e.Data.GetData("System.Windows.Forms.TreeNode");
					NodeTag sourceNodeTag= (NodeTag)sourceNode.Tag;

					// if the tree node was dragged in the same node explorer and
					// we drop a behaviour or a sub-folder
					// on another behaviour or folder, we continue
					if( targetNode.TreeView ==sourceNode.TreeView &&
						(sourceNodeTag.Type ==NodeTagType.Behavior || (sourceNodeTag.Type ==NodeTagType.BehaviorFolder && sourceNode.Parent !=null)) &&
						(targetNodeTag.Type ==NodeTagType.Behavior || targetNodeTag.Type ==NodeTagType.BehaviorFolder) )
					{
						// if the target is a behaviour, use its folder instead
						if(targetNodeTag.Type ==NodeTagType.Behavior)
							targetNode= targetNode.Parent;

						// update the selected node and expand it so you can drag an item into a collapsed sub folder
						targetNode.TreeView.SelectedNode= targetNode;
						targetNode.Expand();
						e.Effect= DragDropEffects.Move;
					}
				}
			}
		}

		/// <summary>
		/// Saves a given behaviour under the filename which is stored in the behaviour's file manager.
		/// If no file manager exists (new node), the user is asked to choose a name.
		/// </summary>
		/// <param name="node">The behaviour node which will be saved.</param>
		/// <param name="saveas">If true, the user will always be asked for a filename, even when a file manager is already present.</param>
		/// <returns>Returns the filename which the behaviour was saved in.</returns>
		public string SaveBehavior(BehaviorNode node, bool saveas)
		{
			if(ShowBehavior ==null)
				throw new Exception("Missing event handler ShowBehavior");

			// store which behaviour is currently shown
			BehaviorNode currNode= BehaviorTreeViewDock.LastFocused ==null ? null : BehaviorTreeViewDock.LastFocused.BehaviorTreeView.RootNode;

			// show the behaviour we want to save
			BehaviorTreeViewDock dock= ShowBehavior(node);

			// check if we need to show the save dialogue
			if(dock.BehaviorTreeView.RootNode.FileManager ==null || saveas)
			{
				// set the filename
				if(dock.BehaviorTreeView.RootNode.FileManager !=null && saveas)
				{
					saveFileDialog.InitialDirectory= Path.GetDirectoryName(dock.BehaviorTreeView.RootNode.FileManager.Filename);
					saveFileDialog.FileName= Path.GetFileNameWithoutExtension(dock.BehaviorTreeView.RootNode.FileManager.Filename);
				}
				else
				{
					saveFileDialog.InitialDirectory= _behaviorFolder;
					saveFileDialog.FileName= ((Node)dock.BehaviorTreeView.RootNode).Label;
				}

				// build the filter from all available file managers
				saveFileDialog.Filter= string.Empty;
				for(int i= 0; i <_fileManagers.Count; ++i)
				{
					saveFileDialog.Filter+= _fileManagers[i].Filter;

					if(i !=_fileManagers.Count -1)
						saveFileDialog.Filter+= '|';
				}

				// show the save dialogue
				if(saveFileDialog.ShowDialog() ==DialogResult.OK)
				{
					// the file is new if it has no file manager
					bool isNew= dock.BehaviorTreeView.RootNode.FileManager ==null;

					// create the selected file manager
					FileManagers.FileManager fm= _fileManagers[saveFileDialog.FilterIndex -1].Create(saveFileDialog.FileName, dock.BehaviorTreeView.RootNode);
					if(fm ==null)
						throw new Exception("Could not create file manager");

					// assign the new file manager and the new name
					dock.BehaviorTreeView.RootNode.FileManager= fm;

					// update the view so we get the new label
					dock.BehaviorTreeView.Invalidate();

					// save the behaviour
					dock.BehaviorTreeView.RootNode.FileManager.Save();

					// if the behaviour was new, ove it from the list of new behaviours to the loaded ones
					if(isNew)
					{
						if(_newBehaviors.Remove(dock.BehaviorTreeView.RootNode))
							_loadedBehaviors.Add(dock.BehaviorTreeView.RootNode);
					}

					// trigger the event that the node was renamed
					if(BehaviorRenamed !=null)
						BehaviorRenamed(dock.BehaviorTreeView.RootNode);

					// rebuild the behaviours in the node explorer
					RebuildBehaviorList();
				}
				else
				{
					// the user aborted
					return string.Empty;
				}
			}
			else
			{
				// simply save the behaviour using the existing file manager
				dock.BehaviorTreeView.RootNode.FileManager.Save();

				// update modified status
				dock.BehaviorTreeView.RootNode.FileManager= dock.BehaviorTreeView.RootNode.FileManager;
			}

			// if we were showing a different behaviour before, return to it
			if(currNode !=null)
				ShowBehavior(currNode);

			//Now the XML has been saved, compile them all into one. If that couldn't be done the save hasn't reached the game:
			//say so to callers (closing the editor stops, rather than leaving work the next start would replace)
			if(!CompileBehaviors())
				return string.Empty;

            return node.FileManager.Filename;
		}

		/// <summary>
		/// Handles when the save all menu item is clicked.
		/// </summary>
		private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if(ShowBehavior ==null)
				throw new Exception("Missing event handler ShowBehavior");

			// store the behaviour which is currently shown
			BehaviorNode currentNode= BehaviorTreeViewDock.LastFocused ==null ? null : BehaviorTreeViewDock.LastFocused.BehaviorTreeView.RootNode;

			// save all the newly created behaviours
			foreach(BehaviorNode node in _newBehaviors)
			{
				try { SaveBehavior(node, false); }
				catch(Exception ex) { MessageBox.Show(ex.Message, "Save Error", MessageBoxButtons.OK); }
			}

			// save all the modified behaviours
			foreach(BehaviorNode node in _loadedBehaviors)
			{
				if(node.IsModified)
				{
					try { SaveBehavior(node, false); }
					catch(Exception ex) { MessageBox.Show(ex.Message, "Save Error", MessageBoxButtons.OK); }
				}
			}

			// restore the previously shown behaviour
			if(currentNode !=null)
				ShowBehavior(currentNode);
		}

		/// <summary>
		/// Handles when the user tries to rename a tree node.
		/// </summary>
		private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			NodeTag nodetag= (NodeTag) e.Node.Tag;

			// we may not rename nodes, node folders and the root behaviour folder
			if( nodetag.Type ==NodeTagType.Node ||
				nodetag.Type ==NodeTagType.NodeFolder ||
				nodetag.Type ==NodeTagType.BehaviorFolder && e.Node.Parent ==null)
			{
				e.CancelEdit= true;
			}
			else
			{
				// we may not rename newly created behaviours as the label is used to identify them
				if(nodetag.Filename ==string.Empty)
					e.CancelEdit= true;
			}
		}

		/// <summary>
		/// Handles the rename process of a behaviour folder or a behaviour
		/// </summary>
		private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			// check if the new label is valid and did change
			if(e.Label ==null || e.Label ==e.Node.Text || e.Label.Length <1)
				return;

			// trim unrequired characters and check if the label is still valid
			string label= e.Label.Trim();
			if(label.Length <1)
				return;

			NodeTag nodetag= (NodeTag) e.Node.Tag;

			try
			{
				// create the new filename
				string targetfile= Path.GetDirectoryName(nodetag.Filename) +'\\'+ label + Path.GetExtension(nodetag.Filename);

				// check if we are renaming a behaviour or a folder
				if(nodetag.Type ==NodeTagType.Behavior)
				{
					// the name is what other trees, character classes and the game find the tree by: it must be a valid one,
					// and one in use can't change (the game finds a tree's file ignoring case, so only changing case is fine)
					string oldName= Path.GetFileNameWithoutExtension(nodetag.Filename);
					string problem= BehaviorTreeFiles.ValidateName(label, _behaviorFolder, nodetag.Filename);
					if(problem ==null && !string.Equals(oldName, label, StringComparison.OrdinalIgnoreCase))
					{
						List<string> users= BehaviorTreeFiles.Users(oldName, _behaviorFolder, SharedData.pathToAI);
						if(users.Count !=0)
							problem= oldName + " can't be renamed while it is used by " + string.Join(", ", users) + ".";
					}
					if(problem !=null)
					{
						e.CancelEdit= true;
						MessageBox.Show(problem, "Rename Behaviour Tree", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}
					if(!CanCompile("rename a tree"))
					{
						e.CancelEdit= true;
						return;
					}

					// mode the file
					string sourcefile= nodetag.Filename;
					File.Move(nodetag.Filename, targetfile);

					BehaviorNode node= GetBehavior(nodetag.Filename);
					if(node !=null)
					{
						// update the node's label and its file manager
						((Node)node).Label= label;
						node.FileManager.Filename= targetfile;

						// if the behaviour is shown it needs to be updated because of the label
						if(BehaviorTreeViewDock.LastFocused !=null)
							BehaviorTreeViewDock.LastFocused.Invalidate();

						// triggered the behaviour renamed event
						if(BehaviorRenamed !=null)
							BehaviorRenamed(node);
					}

					// the game's trees are compiled from the files, so it has the new name at once - or, if that can't be
					// written, the tree keeps its old name in both
					nodetag.Filename= targetfile;
					if(!CompileBehaviors())
					{
						File.Move(targetfile, sourcefile);
						nodetag.Filename= sourcefile;
						if(node !=null)
						{
							((Node)node).Label= Path.GetFileNameWithoutExtension(sourcefile);
							node.FileManager.Filename= sourcefile;
							if(BehaviorRenamed !=null)
								BehaviorRenamed(node);
						}
						CompileBehaviors(false);
						e.CancelEdit= true;
						return;
					}
				}
				else
				{
					// move the folder
					Directory.Move(nodetag.Filename, targetfile);

					// update the filename of all loaded behaviours from this folder
					foreach(BehaviorNode node in _loadedBehaviors)
					{
						if(node.FileManager.Filename.StartsWith(nodetag.Filename))
						{
							node.FileManager.Filename= targetfile + node.FileManager.Filename.Substring(nodetag.Filename.Length);
						}
					}

					// rebuild the list of behaviours
					RebuildBehaviorList();
				}

				// update the filename in the node tag
				nodetag.Filename= targetfile;

				//Invoke(new SortTreeDelegate(SortTree));
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "File Error", MessageBoxButtons.OK);

				RebuildBehaviorList();
			}
		}

		private int _lastNewGroup= 1;

		/// <summary>
		/// Handles when the new group button is clicked
		/// </summary>
		private void createGroupButton_Click(object sender, EventArgs e)
		{
			// if no tree node is selected we do not know where to create the new folder
			if(behaviourTreeView.SelectedNode ==null)
				return;

			// we can only create folders for behaviours and folders
			NodeTag nodetag= (NodeTag) behaviourTreeView.SelectedNode.Tag;
			if(nodetag.Type !=NodeTagType.Behavior && nodetag.Type !=NodeTagType.BehaviorFolder)
				return;

			try
			{
				// if the selected tree node is a behaviour we use its folder
				TreeNode folder= nodetag.Type ==NodeTagType.BehaviorFolder ? behaviourTreeView.SelectedNode : behaviourTreeView.SelectedNode.Parent;
				nodetag= (NodeTag) folder.Tag;

				// create a unique node name the the name of the folder
				string label= GetUniqueLabel("New Folder", _lastNewGroup, out _lastNewGroup);
				string dir= nodetag.Filename +'\\'+ label;

				// create the new folder
				Directory.CreateDirectory(dir);

				// create the tree node for the folder
				TreeNode newnode= new TreeNode(label, (int) NodeIcon.FolderClosed, (int) NodeIcon.FolderClosed);
				newnode.Tag= new NodeTag(NodeTagType.BehaviorFolder, null, dir);

				// add the folder and expand its parent
				folder.Nodes.Add(newnode);
				folder.Expand();

				// selected the new node
				behaviourTreeView.SelectedNode= newnode;

				// allow the user to define a custom name
				newnode.BeginEdit();
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "File Error", MessageBoxButtons.OK);

				// if there was an error we have to rebuild the list of available behaviours and folders
				RebuildBehaviorList();
			}
		}

		/// <summary>
		/// Handles when the key is pressed
		/// </summary>
		private void treeView_KeyDown(object sender, KeyEventArgs e)
		{
			// the node palette shares this handler, but these keys act on the selected tree: only take them from the tree list
			if(sender !=behaviourTreeView)
				return;

			// if the F2 key is pressed and a node is selected which is not currently edited, edit the nodes label
			if(e.KeyCode ==Keys.F2 && behaviourTreeView.SelectedNode !=null && !behaviourTreeView.SelectedNode.IsEditing)
			{
				behaviourTreeView.SelectedNode.BeginEdit();
			}
				// delete the current tree node
			else if(e.KeyCode ==Keys.Delete)
			{
				deleteButton_Click(sender, null);
			}
		}

		/// <summary>
		/// Handles when the delete button is clicked.
		/// </summary>
		private void deleteButton_Click(object sender, EventArgs e)
		{
			if(ClearBehaviors ==null)
				throw new Exception("Missing event handler ClearBehaviors");

			// a rename still being typed would otherwise finish inside the confirmation below
			EndLabelEdit();

			// if no tree node is selected we have nothing to delete
			if(behaviourTreeView.SelectedNode ==null)
				return;

			// we may only delete behaviours: the folder is the game's
			NodeTag nodetag= (NodeTag) behaviourTreeView.SelectedNode.Tag;
			if(nodetag.Type !=NodeTagType.Behavior)
				return;

			// a tree in use can't go: the trees, character classes and levels using it would break, and so would the game
			if(nodetag.Filename !=string.Empty)
			{
				string name= Path.GetFileNameWithoutExtension(nodetag.Filename);
				List<string> users= BehaviorTreeFiles.Users(name, _behaviorFolder, SharedData.pathToAI);
				users.AddRange(UnsavedReferencesTo(name));
				if(users.Count ==0 && !CanCompile("delete a tree"))
					return;
				if(users.Count !=0)
				{
					MessageBox.Show(name + " can't be deleted while it is used by " + string.Join(", ", users) + ".", "Delete Behaviour Tree", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				if(MessageBox.Show("Delete the behaviour tree " + name + "?\n\nIt is removed from the game's trees. Revert Configs in OpenCAGE puts back the trees the game shipped with.",
					"Delete Behaviour Tree", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=DialogResult.Yes)
					return;
			}

			// the list of the behaviours deleted
			List<BehaviorNode> behaviors= new List<BehaviorNode>();

			try
			{
				// check if we delete a bhevaiour
				if(nodetag.Type ==NodeTagType.Behavior)
				{
					BehaviorNode node= GetBehavior(nodetag, behaviourTreeView.SelectedNode.Text);
					if(node !=null)
						behaviors.Add(node);
				}
				else
				{
					// add all behaviours which are loaded and in the folder we want to delete to the behaviour list
					foreach(BehaviorNode node in _loadedBehaviors)
					{
						if(node.FileManager.Filename.StartsWith(nodetag.Filename))
							behaviors.Add(node);
					}
				}

				// close all behaviours which we want to delete
				bool error;
				bool[] result;
				ClearBehaviors(behaviors, out result, out error);

				// check if there was no problem
				for(int i= 0; i <behaviors.Count; ++i)
				{
					// if the behaviour could be closed
					if(result[i])
					{
						// remove the behaviour from the correct list
						if(behaviors[i].FileManager ==null || behaviors[i].FileManager.Filename ==string.Empty)
						{
							_newBehaviors.Remove(behaviors[i]);
						}
						else
						{
							_loadedBehaviors.Remove(behaviors[i]);
						}
					}
				}

				// if there was no error remove the tree node and delete the selected path
				if(!error)
				{
					behaviourTreeView.SelectedNode.Remove();

					if(nodetag.Filename !=string.Empty)
					{
						Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(nodetag.Filename, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

						// the game's trees are compiled from the files, so it loses the tree at once
						CompileBehaviors();
					}
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "File Error", MessageBoxButtons.OK);

				// if there was an error rebuild the list of available behaviours and folders
				RebuildBehaviorList();
			}
		}

		/// <summary>
		/// Handles when a behaviour was modified.
		/// </summary>
		/// <param name="node">The node that was modified.</param>
		private void behavior_WasModified(Node node)
		{
			// check if the node modified was a behaviour
			if(!(node is BehaviorNode))
				return;

			BehaviorNode behavior= (BehaviorNode) node;

			// get the correct tree node
			TreeNode tnode= null;
			if(behavior.FileManager ==null)
			{
				tnode= GetBehaviorNode(((Node)behavior).Label, false);
			}
			else
			{
				tnode= GetBehaviorNode(behavior.FileManager.Filename, true);
			}

			// change the behaviours icon to the modified icon
			if(tnode !=null)
			{
				tnode.ImageIndex= (int) NodeIcon.BehaviorModified;
				tnode.SelectedImageIndex= (int) NodeIcon.BehaviorModified;
			}
		}

		/// <summary>
		/// Handles when a behaviour was saved.
		/// </summary>
		/// <param name="node">The node that was modified.</param>
		private void behavior_WasSaved(BehaviorNode node)
		{
			// if the file manager is null the file could not be saved
			if(node.FileManager ==null || node.FileManager.Filename ==string.Empty)
				return;

			// update the behaviours icon to the not-modified one
			TreeNode tnode= GetBehaviorNode(node.FileManager.Filename, true);
			if(tnode !=null)
			{
				tnode.ImageIndex= (int) NodeIcon.BehaviorLoaded;
				tnode.SelectedImageIndex= (int) NodeIcon.BehaviorLoaded;
			}
		}

		/// <summary>
		/// Copies the tree nodes from one treeview to another one. Used for the export dialogue. Internal use only.
		/// </summary>
		/// <param name="source">The nodes you want to copy.</param>
		/// <param name="target">The tree you want to copy the nodes to.</param>
		private void CopyTreeNodes(TreeNodeCollection source, TreeNodeCollection target)
		{
			foreach(TreeNode node in source)
			{
				NodeTag nodetag= (NodeTag) node.Tag;

				// duplicate the tree node with the export dialogue specific icons
				TreeNode newnode= new TreeNode(node.Text, nodetag.Type ==NodeTagType.BehaviorFolder ? 0 : 1, nodetag.Type ==NodeTagType.BehaviorFolder ? 0 : 1);
				newnode.Tag= node.Tag;

				// add the node
				target.Add(newnode);

				// copy its children
				CopyTreeNodes(node.Nodes, newnode.Nodes);
			}
		}

		/// <summary>
		/// Exports behaviours from the export dialogue. Internal use only.
		/// </summary>
		/// <param name="pool">The pool of tree nodes whose behaviours you want to export.</param>
		/// <param name="folder">The folder the behaviours are exported to.</param>
		/// <param name="exportNoGroups">Defines if the groups are exported as well.</param>
		/// <param name="exporter">The exporter which is used.</param>
		private void ExportBehavior(TreeNodeCollection pool, string folder, bool exportNoGroups, ExporterInfo exporter)
		{
			Exporters.Exporter exp= null;
			bool firstRun= true;

			// export the behaviour for each tree node
			foreach(TreeNode tnode in pool)
			{
				// reset any previously loaded behaviour
				FileManagers.FileManager.ResetLoadedBehavior();

				NodeTag nodetag= (NodeTag) tnode.Tag;

				// if the tree node is selected and a behaviour
				if(nodetag.Type ==NodeTagType.Behavior && tnode.Checked)
				{
					// get or load the behaviour we want to export
					BehaviorNode node= GetBehavior(nodetag, tnode.Text);
					if(node ==null)
						node= LoadBehavior(nodetag.Filename);

					// generate the new filename and the exporter
					exp= exporter.Create(node, folder, exportNoGroups ? tnode.Text : tnode.FullPath);

					// if we are exporting the first time run PreExport()
					if(firstRun)
					{
						firstRun= false;

						if(!exp.PreExport(false))
							return;
					}

					// export behaviour
					exp.Export();
				}

				// export the child tree nodes
				ExportBehavior(tnode.Nodes, folder, exportNoGroups, exporter);
			}

			// when we finishes exporting we call the PostExport() method
			if(exp !=null)
				exp.PostExport(false);
		}

		/// <summary>
		/// Check the tree nodes according to the behaviours which are ought to be exported.
		/// </summary>
		/// <param name="pool">The pool we are checking.</param>
		/// <param name="node">The behaviour we want to export. Use null if all behaviours may be exported.</param>
		private void CheckExportTreeNodes(TreeNodeCollection pool, BehaviorNode node)
		{
			foreach(TreeNode tnode in pool)
			{
				// if no behaviour is given enable all behaviours to be exported.
				if(node ==null)
				{
					tnode.Checked= true;
				}
				else
				{
					// if the behaviour is new one compare the label
					if(node.FileManager ==null)
					{
						tnode.Checked= tnode.Text ==((Node)node).Label;
					}
					else
					{
						// otherwise compare the filename
						NodeTag nodetag= (NodeTag) tnode.Tag;
						tnode.Checked= nodetag.Filename ==node.FileManager.Filename;
					}
				}

				// check the child nodes
				CheckExportTreeNodes(tnode.Nodes, node);
			}
		}

		/// <summary>
		/// Handles when double-clicking an error in the check for errors dialogue.
		/// </summary>
		/// <param name="node">The node you want to show.</param>
		/// <returns>Returns the NodeViewData which will be shown.</returns>
		internal NodeViewData ShowNode(Node node)
		{
			if(ShowBehavior !=null)
			{
				// show the nodes behaviour
				ShowBehavior(node.Behavior);

				// centre the node
				if(BehaviorTreeViewDock.LastFocused !=null)
				{
					NodeViewData nvd= BehaviorTreeViewDock.LastFocused.BehaviorTreeView.CenterNode(node);

					// switch to the editor
					BehaviorTreeViewDock.LastFocused.Focus();

					return nvd;
				}
			}

			return null;
		}
    }
}
