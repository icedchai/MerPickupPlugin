using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
using MerPickupPlugin.Managers;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace MerPickupPlugin
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "MER Pickup Plugin";

        public override string Description => "Pick up MER objects.";

        public override string Author => "icedchqi";

        public override Version Version => new Version(1, 0, 1);

        public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

        public PickupManager PickupManager { get; internal set; }

        public static Plugin Singleton { get; private set; }

        public override void Disable()
        {
            Singleton = null;
            PickupManager.Unsubscribe();
            PickupManager = null;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnSSValueRecieved;
        }

        public override void Enable()
        {
            Singleton = this;

            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnSSValueRecieved;

            SSGroupHeader header = new("MER Pickups");
            SSKeybindSetting thrower = new SSKeybindSetting((Config.ThrowSsId), $"Throw Held Object", UnityEngine.KeyCode.Mouse1, allowSpectatorTrigger: false);
            ServerSpecificSettingsSync.DefinedSettings ??= new ServerSpecificSettingBase[0];
            ServerSpecificSettingsSync.DefinedSettings = ServerSpecificSettingsSync.DefinedSettings.Append(header).Append(thrower).ToArray();
            ServerSpecificSettingsSync.SendToAll();
            PickupManager = new PickupManager();
            PickupManager.Subscribe();

            // Timing.CallDelayed(5f, () => MEROptimizer.Plugin.merOptimizer.excludedNames.AddRange(Config.SchematicToPickup.Keys));
        }

        private Dictionary<Player, float> throwStrengths = new Dictionary<Player, float>();

        private IEnumerator<float> increaseThrowStrength(Player player, float strength, float maxStrength, float strengthIncrease)
        {
            while (strength < maxStrength)
            {
                strength += strengthIncrease;
                if (!throwStrengths.ContainsKey(player))
                {
                    throwStrengths.Add(player, strength);
                }
                throwStrengths[player] = strength;
                yield return Timing.WaitForOneFrame;
            }
            strength = maxStrength;
        }

        private void OnSSValueRecieved(ReferenceHub p, ServerSpecificSettingBase b)
        {
            if (b is SSKeybindSetting keybind)
            {
                if (keybind.SettingId == Config.ThrowSsId)
                {
                    Player player = Player.Get(p);

                    if (keybind.SyncIsPressed)
                    {
                        if (PickupComponent.pickupComponents.TryGetValue(player, out PickupComponent pickupComponent) && pickupComponent != null)
                        {
                            Timing.RunCoroutine(increaseThrowStrength(player, pickupComponent.serializablePickup.MinThrowStrength, pickupComponent.serializablePickup.MaxThrowStrength, pickupComponent.serializablePickup.ThrowStrengthInterval));
                        }
                    }
                    else
                    {
                        if (player == null)
                        {
                            return;
                        }
                        if (PickupComponent.pickupComponents.TryGetValue(player, out PickupComponent pickupComponent) && pickupComponent != null)
                        {
                            pickupComponent.RemoveOwner();
                            pickupComponent.rb?.AddForce(player.Camera.forward * (throwStrengths.TryGetValue(player, out float strength) ? strength : 1), UnityEngine.ForceMode.Impulse);
                        }
                    }
                }
            }
        }
    }
}
