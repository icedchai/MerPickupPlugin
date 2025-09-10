using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using MerPickupPlugin.Serializables;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace MerPickupPlugin.Managers
{
    public class PickupComponent : MonoBehaviour
    {
        public static Dictionary<Player, Transform> targets = new Dictionary<Player, Transform>();

        public static Dictionary<Player, PickupComponent> pickupComponents = new Dictionary<Player, PickupComponent>();

        public SchematicObject schematic;

        public SerializablePickup serializablePickup;

        public Transform? target;

        private Player owner;

        public Player? Owner
        {
            get => owner;
            set
            {
                owner = value;
                if (value != null)
                {
                    PreviousOwner = value;
                }
            }
        }

        public Player? PreviousOwner { get; private set; }

        public Rigidbody? rb;
        
        public void Init(Rigidbody rb, SerializablePickup serializablePickup)
        {
            this.serializablePickup = serializablePickup;
            this.rb = rb;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = serializablePickup.Mass;
            rb.centerOfMass = serializablePickup.CenterOfMass;
        }

        public void RemoveOwner()
        {
            if (Owner != null && pickupComponents.TryGetValue(Owner, out _))
            {
                pickupComponents.Remove(Owner);
                Owner.EnableEffect<Slowness>(0);
            }
            Owner = null;
            target = null;
            if (rb != null)
            {
                rb.useGravity = true;
            }
        }

        public void SetOwner(Player? player)
        {
            if (player == null)
            {
                return;
            }

            if (player == Owner)
            {
                RemoveOwner();
                return;
            }

            if (Owner == null)
            {
                if (pickupComponents.TryGetValue(player, out var otherPickup))
                {
                    otherPickup.RemoveOwner();
                }
                Owner = player;
                pickupComponents.Add(player, this);
                Owner.EnableEffect<Slowness>(serializablePickup.MovementPenalty);
                if (rb != null)
                {
                    rb.useGravity = false;
                }
                // transform.parent = owner.GameObject.transform;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Player.TryGet(collision.collider.gameObject, out Player? player) && player.IsNoclipEnabled == false && rb != null)
            {
                float minDamageWeight;
                if (Plugin.Singleton.Config == null)
                {
                    minDamageWeight = new Config().MinDamageWeight;
                }
                else
                {
                    minDamageWeight = Plugin.Singleton.Config.MinDamageWeight;
                }
                if (rb.mass < minDamageWeight)
                {
                    return;
                }

                //thanks to 'ThatGuy' on discord for help w/ math
                float vel = Mathf.Max(Vector3.Dot(collision.contacts[0].normal, Owner == null ? collision.relativeVelocity : collision.relativeVelocity * 0.05f), 0);
                float reducedMass = (player.Weight() * rb.mass) / (player.Weight() + rb.mass);
                float joules = (reducedMass / 2) * ((float)Math.Pow(vel, 2));

                // Logger.Info($"Impact delivered {joules} joules, velocity: {vel}, reducedMass: {reducedMass}, normal: {collision.contacts[0].normal}, player:{player.LogName}, schem:{schematic.Name}");
                // Logger.Info($"relative velocity: {collision.relativeVelocity}, calced velocity: {vel}, uncapped: {Vector3.Dot(collision.contacts[0].normal, collision.relativeVelocity)}");
                // Logger.Info($"{Vector3.Dot(collision.contacts[0].normal, collision.relativeVelocity)}, {Vector3.Dot(collision.contacts[0].normal * -1, collision.relativeVelocity)}, {Vector3.Dot(collision.contacts[0].normal, collision.relativeVelocity * -1)}");

                string name = Plugin.Singleton.Config != null && Plugin.Singleton.Config.SchematicNameToPropName.TryGetValue(schematic.Name, out name) ? name : schematic.Name;
                if (joules > 80)
                {
                    player.Damage(joules * 0.3f, $"Blunt force trauma: experienced {joules} joules from a {name} going at {vel} m/s. Last known holder of this object is {(PreviousOwner == null ? "unknown" : PreviousOwner.Nickname)}");
                }
            }
        }

        private byte LastTeleported = 0;
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out TeleportObject teleporter) && LastTeleported == 0 && rb != null)
            {
                TeleportObject? target = teleporter.GetRandomTarget();

                if (target != null)
                {
                    rb.MovePosition(target.gameObject.transform.position + target.transform.forward * 1.5f);
                    rb.MoveRotation(target.gameObject.transform.rotation * transform.rotation);
                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(target.gameObject.transform.forward * 4);
                    LastTeleported = 1;
                }
            }
        }


        private void FixedUpdate()
        {
            if (LastTeleported != 0)
            {
                LastTeleported++;
            }

            if (Owner == null)
            {
                RemoveOwner();
                return;
            }

            if (Owner.ReferenceHub == null || !Owner.IsAlive)
            {
                RemoveOwner();
                return;
            }

            if (rb == null)
            {
                rb = schematic.gameObject.AddComponent<Rigidbody>();
            }

            if (target == null)
            {
                if (!targets.TryGetValue(Owner, out target))
                {
                    target = new GameObject("target").transform;
/*
                    var prim = LabApi.Features.Wrappers.PrimitiveObjectToy.Create(null, false);
                    prim.Type = PrimitiveType.Sphere;
                    prim.Scale = Vector3.one * 0.3f;
                    prim.Flags = PrimitiveFlags.Visible;
                    prim.Spawn();
                    target = prim.GameObject.transform;
                    */
                    var targetComponent = target.gameObject.AddComponent<PickupTargetComponent>();
                    target.position = Owner.Camera.position;
                    targetComponent.Init(Owner.Camera);
                    targets.Add(Owner, target);
                }
            }

            if (Vector3.Distance(target.position, transform.position) > serializablePickup.MaxDistance)
            {
                RemoveOwner();
                return;
            }

            if (Owner.CurrentItem != null)
            {
                Owner.CurrentItem = null;
            }

            Vector3 dir = ((target.position + target.forward * serializablePickup.PickupDistance 
                + (target.transform.right * serializablePickup.PickupPosition.x +
                target.transform.up * serializablePickup.PickupPosition.y +
                target.transform.forward * serializablePickup.PickupPosition.z)
                ) - transform.position) * serializablePickup.ObjectLift;


            //stolen code
            Quaternion targetOrientation = target.rotation * Quaternion.Euler(serializablePickup.PickupRotation);
            Quaternion rotationChange = targetOrientation * Quaternion.Inverse(rb.rotation);

            rotationChange.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f)
                angle -= 360f;

            if (Mathf.Approximately(angle, 0))
            {
                rb.angularVelocity = Vector3.zero;
                return;
            }
            angle *= Mathf.Deg2Rad;
            var targetAngularVelocity = axis * angle / Time.deltaTime;


            float catchUp = serializablePickup.RotationCorrectionCatchup;
            targetAngularVelocity *= catchUp;

            rb.AddTorque(targetAngularVelocity - rb.angularVelocity, ForceMode.VelocityChange);
            //stolen code ends here
            rb.linearVelocity = dir;
        }
    }
}
