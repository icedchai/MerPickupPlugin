using AdminToys;
using LabApi.Features.Wrappers;
using ProjectMER.Events.Arguments;
using RelativePositioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace MerPickupPlugin.Managers
{
    public class PickupManager
    {
        public void Subscribe()
        {
            ProjectMER.Events.Handlers.Schematic.SchematicSpawned += OnSchematicSpawned;
        }

        public void Unsubscribe()
        {
            ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= OnSchematicSpawned;
        }

        private void OnSchematicSpawned(SchematicSpawnedEventArgs e)
        {
            if (Plugin.Singleton.Config != null && Plugin.Singleton.Config.SchematicToPickup.TryGetValue(e.Name, out string pickupName) && Plugin.Singleton.Config.Pickups.TryGetValue(pickupName, out var serializablePickup))
            {
                var comp = e.Schematic.gameObject.AddComponent<PickupComponent>();

                // var pickup = Pickup.Create(ItemType.Painkillers, e.Schematic.Position + new Vector3(0, 0.1f, 0));

                var rb = comp.gameObject.AddComponent<Rigidbody>();

                comp.schematic = e.Schematic;
                comp.Init(rb, serializablePickup);

                if (!serializablePickup.PlayerCollidable)
                {
                    comp.gameObject.layer = 9;
                }

                foreach (var p in e.Schematic.AdminToyBases)
                {
                    if (p is PrimitiveObjectToy primToy)
                    {
                        if (!serializablePickup.PlayerCollidable)
                        {
                            if (primToy.NetworkPrimitiveFlags.HasFlag(PrimitiveFlags.Collidable))
                            {
                                Collider collider;
                                switch (primToy.PrimitiveType)
                                {
                                    case PrimitiveType.Cube:
                                        collider = primToy.gameObject.AddComponent<BoxCollider>();
                                        break;
                                    case PrimitiveType.Quad:
                                        collider = primToy.gameObject.AddComponent<BoxCollider>();
                                        collider.transform.localScale = new Vector3(1, 1, 0.01f);
                                        break;
                                    case PrimitiveType.Sphere:
                                        collider = primToy.gameObject.AddComponent<SphereCollider>();
                                        break;
                                    case PrimitiveType.Capsule:
                                        collider = primToy.gameObject.AddComponent<CapsuleCollider>();
                                        break;
                                    case PrimitiveType.Cylinder:
                                        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                                        cylinder.transform.parent = primToy.transform;
                                        cylinder.transform.localScale = Vector3.one;
                                        break;
                                    case PrimitiveType.Plane:
                                        collider = primToy.gameObject.AddComponent<BoxCollider>();
                                        collider.transform.localEulerAngles = new Vector3(10, 10, 0.01f);
                                        break;
                                }
                            }
                            primToy.NetworkPrimitiveFlags &= ~PrimitiveFlags.Collidable;
                        }

                        InteractableToy interactableToy = SpawnInteractableToy(primToy);
                        if (interactableToy == null)
                        {
                            continue;
                        }
                        interactableToy.OnInteracted += p => comp.SetOwner(p);
                    }
                }
            }
        }


        private InteractableToy? SpawnInteractableToy(AdminToys.PrimitiveObjectToy primitiveObjectToy)
        {
            if (!primitiveObjectToy.NetworkPrimitiveFlags.HasFlag(PrimitiveFlags.Collidable))
            {
                return null;
            }
            InteractableToy interactableToy = InteractableToy.Create(primitiveObjectToy.transform, false);
            switch (primitiveObjectToy.PrimitiveType)
            {
                case PrimitiveType.Plane:
                    interactableToy.Shape = InvisibleInteractableToy.ColliderShape.Box;
                    interactableToy.Transform.localScale = new Vector3(interactableToy.Transform.localScale.x * 10, 0.01f, interactableToy.Transform.localScale.z * 10);
                    break;
                case PrimitiveType.Quad:
                    interactableToy.Shape = InvisibleInteractableToy.ColliderShape.Box;
                    interactableToy.Transform.localScale = new Vector3(interactableToy.Transform.localScale.x, interactableToy.Transform.localScale.y, 0.01f);
                    break;
                case PrimitiveType.Cube:
                    interactableToy.Shape = InvisibleInteractableToy.ColliderShape.Box;
                    break;
                case PrimitiveType.Sphere:
                    interactableToy.Shape = InvisibleInteractableToy.ColliderShape.Sphere;
                    break;
                case PrimitiveType.Capsule:
                    interactableToy.Shape = InvisibleInteractableToy.ColliderShape.Capsule;
                    break;
                default:
                    interactableToy.Destroy();
                    return null;
            }

            interactableToy.Transform.localScale = Vector3.one * 1.05f;
            interactableToy.Spawn();
            return interactableToy;
        }
    }
}
