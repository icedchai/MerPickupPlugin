using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MerPickupPlugin.Serializables
{
    [Serializable]
    public class SerializablePickup
    {
        public float Mass { get; set; } = 1f;

        public bool PlayerCollidable { get; set; } = true;

        public float MinThrowStrength { get; set; } = 1f;

        public float MaxThrowStrength { get; set; } = 3f;

        public float ThrowStrengthInterval { get; set; } = 1 / 30f;

        public float ObjectLift { get; set; } = 15f;

        public float RotationCorrectionCatchup { get; set; } = 0.1f;
        
        public byte MovementPenalty { get; set; } = 0;

        public float PickupDistance { get; set; } = 1.5f;

        public Vector3 PickupRotation { get; set; } = Vector3.zero;

        public Vector3 PickupPosition { get; set; } = Vector3.zero;

        public Vector3 CenterOfMass { get; set; } = Vector3.zero;

        public float MaxDistance { get; set; } = 9f;

        public string CollisionMessage { get; set; } = "You feel an object hitting you";
    }
}
