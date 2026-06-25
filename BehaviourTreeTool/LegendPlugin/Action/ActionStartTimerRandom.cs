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
    public class ActionStartTimerRandom : Search_Atomic
    {
        public ActionStartTimerRandom()
            : base("StartTimerRandom", "Starts or restarts a timer, duration is random between the given min and max values.")
        {
        }

        protected float _minTime = -1.0f;
        [DesignerFloat("MinTime", "Minimum duration in seconds.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, -1.0f, 1000.0f, 0.01f, 2, "UnitsCount")]
        public float MinTime
        {
            get { return _minTime; }
            set { _minTime = value; }
        }

        protected float _maxTime = -1.0f;
        [DesignerFloat("MaxTime", "Maximum duration in seconds.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, -1.0f, 1000.0f, 0.01f, 2, "UnitsCount")]
        public float MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = value; }
        }

        protected LOGIC_CHARACTER_TIMER_TYPE _timerType;
        [DesignerEnum("TimerType", "The type of timer we are starting.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, null)]
        public LOGIC_CHARACTER_TIMER_TYPE TimerType
        {
            get { return _timerType; }
            set { _timerType = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ActionStartTimerRandom action = (ActionStartTimerRandom)newnode;
            action._timerType = _timerType;
            action._minTime = _minTime;
            action._maxTime = _maxTime;
        }
    }
}
