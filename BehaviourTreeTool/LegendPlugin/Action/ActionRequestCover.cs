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
    public class ActionRequestCover : Search_Atomic
    {
        public ActionRequestCover()
            : base("RequestCover", "Request a search around the cover the sensed target is in.")
        {
        }

        protected NPC_COVER_REQUEST_TYPE _requestType;
        [DesignerEnum("RequestType", "The type of cover.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, null)]
        public NPC_COVER_REQUEST_TYPE RequestType
        {
            get { return _requestType; }
            set { _requestType = value; }
        }

        protected float _radius = 20.0f;
        [DesignerFloat("Radius of search", "Radius in metres to search for cover.", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, 0, 100, 0.5f, 2, "m")]
        public float Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        protected bool _shouldInvalidateCover = true;
        [DesignerBoolean("InvalidateCurrentCover", "Should the atomic invalidate current cover.", "CategoryBasic", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public bool InvalidateCurrentCover
        {
            get { return _shouldInvalidateCover; }
            set { _shouldInvalidateCover = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            ActionRequestCover action = (ActionRequestCover)newnode;
            action._requestType = _requestType;
            action._radius = _radius;
            action._shouldInvalidateCover = _shouldInvalidateCover;
        }
    }
}
