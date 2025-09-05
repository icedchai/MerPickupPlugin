using MerPickupPlugin.Serializables;

namespace MerPickupPlugin
{
    public class Config
    {
        public float MinDamageWeight { get; set; } = 9;
        public int ThrowSsId { get; set; } = 90;
        public Dictionary<string, SerializablePickup> Pickups { get; set; } = new Dictionary<string, SerializablePickup>() { { "Conk", new SerializablePickup() } };
    }
}
