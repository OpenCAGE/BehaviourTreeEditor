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
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LegendPlugin.Nodes
{
    public abstract class ActionMoveBase : Normal_Atomic
    {
        public ActionMoveBase(string label, string description)
            : base(label, description)
        {

        }

        protected SPEED_BOUNDARY _speed;
        [DesignerEnum("MovementSpeedType", "The speed at which we move to the target.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, null)]
        public SPEED_BOUNDARY MovementSpeedType
        {
            get { return _speed; }
            set { _speed = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ActionMoveBase action = (ActionMoveBase)newnode;
            action._speed = _speed;
        }
    }

    public abstract class ActionMoveAndShootBase : ActionMoveBase
    {
        public ActionMoveAndShootBase(string label, string description)
            : base(label, description)
        {

        }

        protected bool _shouldAim = false;
        [DesignerBoolean("ShouldAim", "Should the atomic succeed when we are close to the target.", "CategoryBasic", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public bool ShouldAim
        {
            get { return _shouldAim; }
            set { _shouldAim = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ActionMoveAndShootBase action = (ActionMoveAndShootBase)newnode;
            action._shouldAim = _shouldAim;
        }

    }
}
