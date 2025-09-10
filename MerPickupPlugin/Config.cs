using MerPickupPlugin.Serializables;

namespace MerPickupPlugin
{
    public class Config
    {
        public float MinDamageWeight { get; set; } = 9;
        public int ThrowSsId { get; set; } = 90;

        public Dictionary<string, string> SchematicNameToPropName { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> SchematicToPickup { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, SerializablePickup> Pickups { get; set; } = new Dictionary<string, SerializablePickup>() { { "toy", new SerializablePickup() } };
    }
}
