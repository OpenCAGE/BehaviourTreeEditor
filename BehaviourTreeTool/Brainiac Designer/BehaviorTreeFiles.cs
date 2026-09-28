using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using CATHODE;

namespace Brainiac.Design
{
    /// <summary>
    /// The game's behaviour trees on disk: DATA/BEHAVIOR holds one .xml per tree while the editor runs, and
    /// DATA/BINARY_BEHAVIOR/_DIRECTORY_CONTENTS.BML - what the game and OpenCAGE read - is compiled from them.
    /// </summary>
    public static class BehaviorTreeFiles
    {
        /// <summary>
        /// The longest name a new or renamed tree may have. The game copies a tree's name (and ".bml") into a 100 byte
        /// buffer; retail's longest is 36 characters.
        /// </summary>
        public const int MaxNameLength = 64;

        /// <summary>
        /// The trees the game picks itself - the player's - whatever any character class says.
        /// </summary>
        public static readonly string[] EngineTrees = { "PlayerBehaviour", "NoBehaviour" };

        /// <summary>
        /// Why <paramref name="name"/> can't be a tree's name in <paramref name="folder"/>, or null if it can. A tree's name is
        /// its file name, its entry in the game's directory and what other trees and character classes refer to it by, so it
        /// keeps to the letters, digits and underscores retail uses, and differs from every other tree's by more than case.
        /// <paramref name="currentFile"/> is the tree being renamed, if any.
        /// </summary>
        public static string ValidateName(string name, string folder, string currentFile = null)
        {
            if (string.IsNullOrEmpty(name))
                return "Give the tree a name.";
            if (name.Length > MaxNameLength)
                return "A tree's name can be at most " + MaxNameLength + " characters.";
            if (!Regex.IsMatch(name, "^[A-Za-z0-9_]+$"))
                return "A tree's name can only use letters, digits and underscores.";
            //Windows opens a device, not a file, for these (whatever the extension)
            if (Regex.IsMatch(name, "^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])$", RegexOptions.IgnoreCase))
                return name + " is a name Windows keeps for a device: choose another.";
            foreach (string file in TreeFiles(folder))
            {
                if (currentFile != null && string.Equals(Path.GetFullPath(file), Path.GetFullPath(currentFile), StringComparison.OrdinalIgnoreCase))
                    continue;
                if (string.Equals(Path.GetFileNameWithoutExtension(file), name, StringComparison.OrdinalIgnoreCase))
                    return "There is already a tree called " + Path.GetFileNameWithoutExtension(file) + ".";
            }
            return null;
        }

        /// <summary>
        /// Every tree file in the folder, in the order the game's directory lists them (names compared upper-cased, as the
        /// file system sorts them). Only top-level .xml files are trees: anything else there is ignored.
        /// </summary>
        public static List<string> TreeFiles(string folder)
        {
            List<string> files = new List<string>();
            if (!Directory.Exists(folder))
                return files;
            foreach (string file in Directory.GetFiles(folder, "*.xml", SearchOption.TopDirectoryOnly))
            {
                //GetFiles("*.xml") also matches longer extensions ("x.xmlbak"); hidden and system files are not trees either
                if (!string.Equals(Path.GetExtension(file), ".xml", StringComparison.OrdinalIgnoreCase))
                    continue;
                if ((File.GetAttributes(file) & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                    continue;
                files.Add(file);
            }
            files.Sort((a, b) => string.Compare(Path.GetFileName(a), Path.GetFileName(b), StringComparison.OrdinalIgnoreCase));
            return files;
        }

        /// <summary>
        /// What would stop the trees in <paramref name="folder"/> compiling, without writing anything (empty if nothing).
        /// </summary>
        public static List<string> Check(string folder)
        {
            List<string> problems;
            Build(folder, out problems);
            return problems;
        }

        /// <summary>
        /// Compile every tree in <paramref name="folder"/> into the game's directory at <paramref name="bmlPath"/>. Nothing is
        /// written if a tree can't be read or has no root node (the game loads each level's trees by position, so one that
        /// fails to load hands every tree after it to the wrong characters), or if it references a tree that isn't there
        /// (the game silently drops that branch - or the whole tree, if the reference is its top node).
        /// Returns the problems (empty on success).
        /// </summary>
        public static List<string> Compile(string folder, string bmlPath)
        {
            List<string> problems;
            XmlDocument directory = Build(folder, out problems);
            if (problems.Count != 0)
                return problems;

            BML bml = new BML(bmlPath);
            bml.Content = directory;
            if (!bml.Save())
                problems.Add("Could not write " + bmlPath + " (is it read-only, or open in another program?).");
            return problems;
        }

        private static XmlDocument Build(string folder, out List<string> problems)
        {
            problems = new List<string>();
            XmlDocument directory = new XmlDocument();
            directory.AppendChild(directory.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = directory.CreateElement("DIR");
            directory.AppendChild(root);

            List<string> files = TreeFiles(folder);
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in files)
                names.Add(Path.GetFileNameWithoutExtension(file));

            foreach (string file in files)
            {
                XmlDocument tree = new XmlDocument();
                try
                {
                    tree.Load(file);
                }
                catch (Exception ex)
                {
                    problems.Add(Path.GetFileName(file) + ": " + ex.Message);
                    continue;
                }
                XmlElement behavior = tree.DocumentElement;
                if (behavior == null || behavior.Name != "Behavior" || behavior["Node"] == null)
                {
                    problems.Add(Path.GetFileName(file) + ": it has no <Behavior><Node> root.");
                    continue;
                }
                //The game finds a referenced tree the way it finds any other ("x.xml" -> "x.bml", ignoring case)
                foreach (XmlNode node in behavior.SelectNodes(".//Node[@ReferenceFilename]"))
                {
                    string reference = Path.GetFileNameWithoutExtension(((XmlElement)node).GetAttribute("ReferenceFilename"));
                    if (!names.Contains(reference))
                        problems.Add(Path.GetFileName(file) + ": it references " + reference + ", which isn't a tree.");
                }
                XmlElement entry = directory.CreateElement("File");
                entry.SetAttribute("name", Path.GetFileNameWithoutExtension(file) + ".bml");
                entry.AppendChild(directory.ImportNode(behavior, true));
                root.AppendChild(entry);
            }
            if (problems.Count == 0 && root.ChildNodes.Count == 0)
                problems.Add("There are no trees in " + folder + ".");
            return directory;
        }

        /// <summary>
        /// What uses the tree called <paramref name="name"/>: the game itself (the player's trees), other trees that
        /// reference it, and character classes that run it. A tree in use can't be renamed or deleted without breaking them.
        /// </summary>
        public static List<string> Users(string name, string folder, string pathToAI)
        {
            List<string> users = new List<string>();
            foreach (string engineTree in EngineTrees)
            {
                if (string.Equals(engineTree, name, StringComparison.OrdinalIgnoreCase))
                    users.Add("the game itself (it gives " + engineTree + " to players)");
            }

            //Trees refer to each other by file name ("x.xml"), found ignoring case
            foreach (string file in TreeFiles(folder))
            {
                string treeName = Path.GetFileNameWithoutExtension(file);
                if (string.Equals(treeName, name, StringComparison.OrdinalIgnoreCase))
                    continue;
                try
                {
                    XmlDocument tree = new XmlDocument();
                    tree.Load(file);
                    foreach (XmlNode node in tree.SelectNodes("//Node[@ReferenceFilename]"))
                    {
                        string reference = Path.GetFileNameWithoutExtension(((XmlElement)node).GetAttribute("ReferenceFilename"));
                        if (string.Equals(reference, name, StringComparison.OrdinalIgnoreCase))
                        {
                            users.Add("the tree " + treeName);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    //A tree that can't be read can't be checked; Compile reports it
                }
            }

            //A class runs the tree its attribute config names (the game matches that name exactly, so compare ignoring case
            //to catch near misses too)
            string attributes = Path.Combine(pathToAI, "DATA", "CHR_INFO", "ATTRIBUTES");
            if (Directory.Exists(attributes))
            {
                foreach (string file in Directory.GetFiles(attributes, "*.BML"))
                {
                    BML bml = new BML(file);
                    XmlElement tree = bml.Loaded ? bml.Content?["Attribute"]?["Behavior"]?["Behavior_Tree"] : null;
                    if (tree != null && string.Equals(tree.InnerText, name, StringComparison.OrdinalIgnoreCase))
                        users.Add("the character class " + Path.GetFileNameWithoutExtension(file));
                }
            }

            //Each level lists the trees it loads (WORLD/BEHAVIOR_TREE.DB), and OpenCAGE only brings a level's list up to date
            //when the level is saved: a tree a list still names would fail to load there, handing every tree listed after it
            //to the wrong characters
            List<string> levels = LevelsListing(name, pathToAI);
            if (levels.Count != 0)
                users.Add(levels.Count + " level" + (levels.Count == 1 ? "" : "s") + " (" + string.Join(", ", levels.GetRange(0, Math.Min(5, levels.Count))) +
                    (levels.Count > 5 ? " and more" : "") + ") - once no class uses it, saving " + (levels.Count == 1 ? "that level" : "those levels") + " in OpenCAGE takes it off " + (levels.Count == 1 ? "its list" : "their lists"));
            return users;
        }

        /// <summary>
        /// The levels (by name under DATA/ENV) whose behaviour tree list names <paramref name="name"/>, ignoring case. A
        /// BSPNostromo level is read from its _PATCH folder, as the game does.
        /// </summary>
        private static List<string> LevelsListing(string name, string pathToAI)
        {
            List<string> levels = new List<string>();
            string env = Path.Combine(pathToAI, "DATA", "ENV");
            if (!Directory.Exists(env))
                return levels;
            foreach (string world in Directory.GetDirectories(env, "WORLD", SearchOption.AllDirectories))
            {
                string path = Path.Combine(world, "BEHAVIOR_TREE.DB");
                if (!File.Exists(path))
                    continue;
                string level = Path.GetDirectoryName(world).Substring(env.Length).TrimStart('\\', '/').Replace('\\', '/');
                bool nostromo = level.StartsWith("PRODUCTION/DLC/BSPNOSTROMO", StringComparison.OrdinalIgnoreCase);
                if (nostromo && !level.EndsWith("_PATCH", StringComparison.OrdinalIgnoreCase) && Directory.Exists(Path.Combine(env, level + "_PATCH", "WORLD")))
                    continue;
                if (nostromo && level.EndsWith("_PATCH", StringComparison.OrdinalIgnoreCase))
                    level = level.Substring(0, level.Length - "_PATCH".Length);
                foreach (string entry in new BehaviorTreeDB(path).Entries)
                {
                    if (string.Equals(entry, name, StringComparison.OrdinalIgnoreCase))
                    {
                        levels.Add(level);
                        break;
                    }
                }
            }
            return levels;
        }
    }
}
