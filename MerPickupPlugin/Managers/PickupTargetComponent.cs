using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MerPickupPlugin.Managers
{
    public class PickupTargetComponent : MonoBehaviour
    {
        public Transform camera;

        public void Init(Transform camera)
        {
            this.camera = camera;
        }

        private void Update()
        {
            if (camera == null)
            {
                Destroy(this);
                return;
            }
            transform.position = camera.position;
            transform.rotation = camera.rotation;
            return;
            Vector3 camRot = camera.eulerAngles;
            float vertRot = camRot.x;
            if (vertRot > 90)
            {
                vertRot -= 270;
            }
            else
            {
                vertRot += 90;
            }

            vertRot = Mathf.Clamp(vertRot, 50, 120);

            transform.rotation = Quaternion.Euler(vertRot - 90, camRot.y, camRot.z);
        }
    }
}
