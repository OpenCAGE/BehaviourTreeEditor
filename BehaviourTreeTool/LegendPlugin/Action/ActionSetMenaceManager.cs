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

//using System;
using System.Collections.Generic;
using System.Text;
using Brainiac.Design.Nodes;
using Brainiac.Design.Attributes;
using LegendPlugin.Properties;

namespace LegendPlugin.Nodes
{
    public class ActionSetMenaceManager : Action
    {
        private bool _isAggressive = false;
        private bool _reset = false;
        private string _fraction = "";

        [DesignerBoolean("Aggressive menace", "AggressiveMenace", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public bool AggressiveMenace
        {
            get { return _isAggressive; }
            set { _isAggressive = value; }
        }

        [DesignerBoolean("Reset menace gauge", "ResetMenaceGauge", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public bool ResetMenaceGauge
        {
            get { return _reset; }
            set { _reset = value; }
        }

        [DesignerString("Progression fraction", "ProgressionFraction", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public string ProgressionFraction
        {
            get { return _fraction; }
            set { _fraction = value; }
        }

        public ActionSetMenaceManager() : base("SetMenaceManager", "Directly set values on the menace gauge manager system - unused in the final game.")
        {
            
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ActionSetMenaceManager cond = (ActionSetMenaceManager)newnode;
            cond._isAggressive = _isAggressive;
            cond._reset = _reset;
            cond._fraction = _fraction;
        }
    }
}
