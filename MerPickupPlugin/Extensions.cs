using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MerPickupPlugin
{
    public static class Extensions
    {
        public static float Weight(this Player player)
        {
            float weight = 70;
            float overallVolumeDiff = player.Scale.x * player.Scale.y * player.Scale.z;
            weight *= Mathf.Pow(overallVolumeDiff, 3);
            return weight;
        }
    }
}
