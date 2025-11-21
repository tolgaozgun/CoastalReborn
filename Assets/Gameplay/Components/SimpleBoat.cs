using UnityEngine;
using Gameplay.Interfaces;

namespace Gameplay.Components
{
    /// <summary>
    /// Simple boat component for creating basic boat prefabs.
    /// </summary>
    public class SimpleBoat : MonoBehaviour
    {
        [Header("Boat Configuration")]
        [SerializeField] private ShipType shipType = ShipType.CivilianSmall;
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float rotationSpeed = 60f;

        private Rigidbody rigidbody;
        private Vector3 targetPosition;
        private bool hasTarget = false;

        public ShipType ShipType => shipType;
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
                ConfigureRigidbody();
            }
        }

        private void ConfigureRigidbody()
        {
            rigidbody.mass = 1000f;
            rigidbody.linearDamping = 1f;
            rigidbody.angularDamping = 2f;
            rigidbody.useGravity = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Update()
        {
            if (hasTarget)
            {
                MoveTowardsTarget();
            }
        }

        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
            hasTarget = true;
        }

        public void ClearTarget()
        {
            hasTarget = false;
        }

        private void MoveTowardsTarget()
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > 1f)
            {
                // Rotate towards target
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Move forward
                rigidbody.AddForce(transform.forward * moveSpeed, ForceMode.Force);
            }
            else
            {
                ClearTarget();
            }
        }

        public void StopMovement()
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            ClearTarget();
        }
    }
}