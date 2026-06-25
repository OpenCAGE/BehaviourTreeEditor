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

using Brainiac.Design.Attributes;
using Brainiac.Design.Nodes;
using LegendPlugin.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace LegendPlugin.Nodes
{
    public class ActionDebugMenuLinkItem : Search_Atomic
    {
        public ActionDebugMenuLinkItem()
            : base("DebugMenuLinkItem", "Will always succeed - sets the debug menu item to the given value (not functional) in retail).")
        {
        }

        protected String _inGameMenuText;
        [DesignerString("In_Game_Menu_Text", "What to display in the in-game debug menus.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public String In_Game_Menu_Text
        {
            get { return _inGameMenuText; }
            set { _inGameMenuText = value; }
        }

        protected bool _defaultBehaviour;
        [DesignerBoolean("DefaultBehaviour", "What should it set the menu item back to?", "CategoryBasic", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public bool DefaultBehaviour
        {
            get { return _defaultBehaviour; }
            set { _defaultBehaviour = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ActionDebugMenuLinkItem action = (ActionDebugMenuLinkItem)newnode;

            action._inGameMenuText = _inGameMenuText;
            action._defaultBehaviour = _defaultBehaviour;
        }
    }
}
