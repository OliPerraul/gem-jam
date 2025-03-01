﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Cirrus.Circuit.World.Objects
{

    public class Ladder : BaseObject
    {
        public override ObjectType Type => ObjectType.Ladder;

        public override void Start()
        {
            base.Start();
        }

        public override void Awake()
        {
            base.Awake();
        }

        public override ReturnType GetMoveResults(
            Move move, 
            out IEnumerable<MoveResult> results,
            bool isRecursiveCall = false,
            bool lockResults = true)
        {
            results = null;
            return ReturnType.Failed;
        }

        public override ReturnType GetEnterResults(
            Move move, 
            out EnterResult result, 
            out IEnumerable<MoveResult> moveResults)
        {
            moveResults = new MoveResult[0];
            result = new EnterResult
            {
                Position = move.Position,
                Destination = move.Position + Vector3Int.up,
                Step = Vector3Int.up,
                Entered = null,
                Moved = null, // TODO arriving at the ledge
                Offset = Vector3Int.zero,
                PitchAngle = 0,
                MoveType = MoveType.Climbing
            };

            if (!LevelSession.Instance.Get(
                _levelPosition + Vector3Int.up, out BaseObject _
                ))
            {
                result.Destination = _levelPosition + Vector3Int.up;                
            }

            return ReturnType.Succeeded_Next;

            //return base.GetEnterResults(move, out result, out moveResults);
        }
    }
}