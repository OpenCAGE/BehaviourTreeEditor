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

/*
 * 
 * LegendPlugin was created by Matt Filer
 * www.mattfiler.co.uk
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace LegendPlugin
{
    //note - for cathode enums i should add a thing that removes the last entry if it's UNKNOWN_<enum_name>

    public enum SPEED_BOUNDARY
    {
        Slowest,
        Slow,
        Fast,
        Fastest
    }

    public enum GROUP_AWARENESS_STATE
    {
        UNAWARE,
        SUSPICIOUS,
        SEARCHING_AREA,
        SEARCHING_LAST_SENSED,
        AWARE,
    }

    public enum AWARENESS_STATE
    {
        DEAD,
        STUNNED,
        UNAWARE,
        SUSPICIOUS,
        SEARCHING_AREA,
        SEARCHING_LAST_SENSED,
        AWARE,
    }

    public enum WITHDRAW_STATE
    {
        NOT_WITHDRAWING,
        NEEDS_TO_WITHDRAW,
        WITHDRAWING,
    }

    public enum SEQUENCE_CHILD_STATE_TYPE
    {
        CHILD_DEFAULT,
        IGNORE_CHILD_FAIL,
    }

    public enum SHUTDOWN_SPEED_TYPE
    {
        SST_GRACEFULL,
        SST_EXPEDIENT,
        SST_CRITICAL,
    }

    public enum ThresholdQualifier
    {
        TRACE_THRESHOLD,
        LOWER_THRESHOLD,
        ACTIVATED_THRESHOLD,
        UPPER_THRESHOLD,
    }

    public enum BACKSTAGE_BEHAVIOUR
    {
        BACKSTAGE_ONLY,
        ALLOW_KILLTRAP,
    }

    public enum STEP_TYPE
    {
        FORWARD,
        BACK
    }

    public enum ROLE_TYPE
    {
        IDLE,
        DESPAWN,
        SYSTEMATIC_SEARCH,
        SYSTEMATIC_SEARCH_SUSPICIOUS_ITEM,
        STALK,
        HIDING_PLAYER,
        FOLLOW,
        SUSPECT_RESPONSE_MOVE_TO,
        PANIC
    }

    public enum SENSORY_TYPE
    {
        VISUAL,
        HEARD_COMBAT,
        HEARD_MOVEMENT,
        DAMAGED,
        TOUCHED,
        AFFECTED_BY_FLAME_THROWER,
        SEE_FLASH_LIGHT,
        COMBINED,
    }

    public enum MOTIVATION_TYPE
    {
        CINEMATIC_MOTIVATION,
	    ATTACK_MOTIVATION,
	    AIM_MOTIVATION,
	    DESPAWN_MOTIVATION,
	    FOLLOW_MOTIVATION,
	    STANDARD_MOTIVATION,
	    SEARCH_SYSTEMATIC_MOTIVATION,
	    STALK_MOTIVATION,
	    BACKSTAGE_STALK_MOTIVATION,
	    SHOT_MOTIVATION,
	    SUSPECT_TARGET_RESPONSE_MOTIVATION,
	    THREAT_AWARE_MOTIVATION,
	    BACKSTAGE_AMBUSH_MOTIVATION,
	    IDLE_JOB_MOTIVATION,
	    USE_COVER_MOTIVATION,
	    ASSAULT_MOTIVATION,
	    MELEE_MOTIVATION,
	    RETREAT_MOTIVATION,
	    CLOSE_ON_TARGET_MOTIVATION,
	    MUTUAL_MELEE_ONLY_MOTIVATION,
	    VENT_MELEE_MOTIVATION,
	    MULTIPLAYER_MOTIVATION,
	    REACT_TO_WEAPON_MOTIVATION,
	    SUSPICIOUS_ITEM_MOTIVATION,
	    AGGRESSION_ESCALATION_MOTIVATION,
	    STUN_DAMAGE_MOTIVATION,
	    BREAKOUT_MOTIVATION,
	    PLAYER_HIDE_MOTIVATION,
	    OBSERVE_TARGET_MOTIVATION,
	    ADVANCING_MOTIVATION,
	    AMBUSH_MOTIVATION,
	    PANIC_MOTIVATION,
	    DEBUG_FORCE_CHARACTER_IDLE_MOTIVATION,
	    BACKSTAGE_ALIEN_RESPONSE_MOTIVATION,
	    ESCALATION_PREVENTS_SEARCH_MOTIVATION,
    }

    public enum CHARACTER_TYPE
    {
        OWNER,
        TARGET,
    };

    public enum VENT_CHARACTER_TYPE
    {
        OWNER,
        TARGET,
        OWNER_AND_TARGET,
    }

    public enum SOUND_ARGUMENT
    {
        ALIEN_AFFECTED_BY_FLAME_THROWER,
        ALIEN_ATTACK,
        ALIEN_BREATH_SLOW_LOOP,
        ALIEN_CHARGE_TO_ATTACK,
        ALIEN_DAMAGED_BY_FLAME_THROWER,
        ALIEN_DAMAGED_BY_ORDNANCE,
        ALIEN_SEARCHING,
        ALIEN_SEARCHING_FAIL,
        ALIEN_STALKING,
        ALIEN_STARTS_SEARCHING,
        ALIEN_SUSPECTS_TARGET,
    }

    public enum ATTACK_TYPE
    {
        ANY,
        MELEE,
        GRAB,
        VENT,
        TRAP
    };

    public enum ATTACK_TYPE_NO_TRAP
    {
        ANY,
        MELEE,
        GRAB,
        VENT,
    };

    public enum TIME_THRESHOLD
    {
        TM_0,
        TM_1,
        TM_2,
        TM_3,
        TM_4,
        TM_5,
        TM_10,
        TM_15,
        TM_20,
        TM_25,
        TM_30,
        TM_35,
        TM_40,
        TM_45,
        TM_50,
        TM_55,
        TM_60,
        TM_70,
        TM_80,
        TM_90,
        TM_100,
        TM_110,
        TM_120,
    }

    public enum DISTANCE_THRESHOLD
    {
        DT_0,
        DT_1,
        DT_2,
        DT_3,
        DT_4,
        DT_5,
        DT_6,
        DT_7,
        DT_8,
        DT_9,
        DT_10,
        DT_12,
        DT_14,
        DT_16,
        DT_18,
        DT_20,
        DT_25,
        DT_30,
        DT_35,
        DT_40,
        DT_45,
        DT_50,
    }

    public enum GAUGE_AMOUNT_TYPE
    {
        GAUGE_NONE,
        GAUGE_TRACE,
        GAUGE_LOWER,
        GAUGE_ACTIVATED,
        GAUGE_UPPER,
        GAUGE_FULL,
    }

    public enum COMBAT_AREA_TYPE
    {
        COMBAT_AREA_DEFEND,
        COMBAT_AREA_PURSUIT,
    }

    public enum OBJECTIVE_TYPE
    {
        OBJECTIVE_TYPE_MOVE,
        OBJECTIVE_TYPE_START_POS,
        OBJECTIVE_TYPE_COVER,
        OBJECTIVE_TYPE_SAFE_POINT,
    }

    public enum OBJECTIVE_TYPE_NO_COVER
    {
        OBJECTIVE_TYPE_MOVE,
        OBJECTIVE_TYPE_START_POS,
        OBJECTIVE_TYPE_SAFE_POINT,
    }

    public enum NPC_WEAPON_TYPE
    {
        WEAPON_TYPE_ANY,
        WEAPON_TYPE_PROJECTILE,
        WEAPON_TYPE_MELEE,
    }

    public enum WEAPON_EQUIP
    {
        SHOULD_EQUIP,
        SHOULD_UNEQUIP
    }

    public enum WEAPON_STANCE
    {
        GUN_RAISED,
        GUN_LOWERED
    }

    public enum WEAPON_RANGE_THRESHOLD
    {
        WRT_TOO_CLOSE,
        WRT_EFFECTIVE_RANGE,
        WRT_MAX_RANGE,
        WRT_TOO_FAR,
        WRT_PREFERRED_RANGE
    }

    public enum WEAPON_RANGE_THRESHOLD_NO_EFFECTIVE
    {
        WRT_TOO_CLOSE,
        WRT_MAX_RANGE,
        WRT_TOO_FAR,
        WRT_PREFERRED_RANGE
    }

    public enum SHOT_TERMINATOR_TYPE
    {
        Continuous,
        Shot_1,
        Shot_2,
        Shot_3,
        Shot_4,
        Random_between_1_and_4,
        Random_between_1_and_ClipCount,
    }

    public enum MOVE_DIRECTION
    {
        Back, 
        Left, 
        Right, 
        Forward
    }

    public enum HEALTH_STATE_TYPE
    {
        FULL,
        DAMAGED,
        INJURED,
        DOWN_BUT_NOT_OUT,
    };
}
