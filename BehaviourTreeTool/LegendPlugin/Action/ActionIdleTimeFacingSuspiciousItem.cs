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

namespace LegendPlugin.Nodes
{
    public class ActionIdleTimeFacingSuspiciousItem : Normal_Atomic
    {
        public ActionIdleTimeFacingSuspiciousItem()
            : base("IdleTimeFacingSuspiciousItem", "Idle time facing suspicious item.")
        {
        }

        protected float _time = 5.0f;
        [DesignerFloat("Time", "Duration in seconds.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, -1.0f, 1000, 0.01f, 2, "UnitsCount")]
        public float Time
        {
            get { return _time; }
            set { _time = value; }
        }

        protected float _noiseTime = 0.0f;
        [DesignerFloat("NoiseTime", "Random variation in seconds applied to the base duration.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, 0.0f, 1000, 0.01f, 2, "UnitsCount")]
        public float NoiseTime
        {
            get { return _noiseTime; }
            set { _noiseTime = value; }
        }

        protected bool _waitForTeamMembersRouting = false;
        [DesignerBoolean("WaitForTeamMembersRouting", "If set then this is used in the WaitForTeamMembersRouting branch.", "CategoryBasic", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public bool WaitForTeamMembersRouting
        {
            get { return _waitForTeamMembersRouting; }
            set { _waitForTeamMembersRouting = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ActionIdleTimeFacingSuspiciousItem action = (ActionIdleTimeFacingSuspiciousItem)newnode;
            action._time = _time;
            action._noiseTime = _noiseTime;
            action._waitForTeamMembersRouting = _waitForTeamMembersRouting;
        }
    }
}
