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

using System.Collections.Generic;
using System.Text;
using Brainiac.Design.Nodes;
using Brainiac.Design.Attributes;
using LegendPlugin.Properties;
using CATHODE.Enums;

namespace LegendPlugin.Nodes
{
    public class ActionSetGaugeAmount : Search_Atomic
    {
        public ActionSetGaugeAmount()
            : base("SetGaugeAmount", "Set gauge amount.")
        {
        }

        protected LOGIC_CHARACTER_GAUGE_TYPE _gaugeType;
        [DesignerEnum("GaugeType", "The type of gauge we are setting.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, null)]
        public LOGIC_CHARACTER_GAUGE_TYPE GaugeType
        {
            get { return _gaugeType; }
            set { _gaugeType = value; }
        }

        protected GAUGE_AMOUNT_TYPE _gaugeAmountType;
        [DesignerEnum("GaugeAmountType", "The activation we must be greater than or equal to.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, null)]
        public GAUGE_AMOUNT_TYPE GaugeAmountType
        {
            get { return _gaugeAmountType; }
            set { _gaugeAmountType = value; }
        }

        protected bool _setAtleastTo;
        [DesignerBoolean("SetAtleastTo", "The gauge will be set to at least this value if it is already higher than it will remain unchanged.", "CategoryBasic", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public bool SetAtleastTo
        {
            get { return _setAtleastTo; }
            set { _setAtleastTo = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ActionSetGaugeAmount cond = (ActionSetGaugeAmount)newnode;
            cond._gaugeType = _gaugeType;
            cond._gaugeAmountType = _gaugeAmountType;
            cond._setAtleastTo = _setAtleastTo;
        }
    }
}
