using UnityEngine;

namespace MmorpgPrototype
{
    // Movimiento simple de mascota: sigue al jugador manteniendo distancia,
    // con un leve balanceo. Solo presentacion; los bonos viven en PetService.
    public sealed class PetController : MonoBehaviour
    {
        public Transform Target;
        public float FollowDistance = 1.7f;
        public float MoveSpeed = 6.5f;

        private float bobSeed;

        private void Awake()
        {
            bobSeed = Random.Range(0f, 10f);
        }

        private void Update()
        {
            if (Target == null)
            {
                return;
            }

            var toTarget = Target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude > FollowDistance)
            {
                var destination = Target.position - toTarget.normalized * FollowDistance;
                destination.y = transform.position.y;
                transform.position = Vector3.MoveTowards(transform.position, destination, MoveSpeed * Time.deltaTime);

                if (toTarget.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget.normalized, Vector3.up), 8f * Time.deltaTime);
                }
            }

            var position = transform.position;
            position.y = 0.45f + Mathf.Sin(Time.time * 3f + bobSeed) * 0.08f;
            transform.position = position;
        }
    }
}
