using UnityEngine;

namespace VirtualChemLab
{
    [RequireComponent(typeof(LiquidContainer))]
    public class DynamicPourPoint : MonoBehaviour
    {

        [Header("Container Geometry")]
        public float rimRadius = 0.03f;
        public float rimHeight = 0.12f;
        public float lipOffset = 0.005f;

        [Header("Tilt Detection")]
        public float tiltThreshold = 25f;
        public float maxTiltAngle = 90f;

        [Header("Debug")]
        public bool drawGizmos = true;

        private LiquidContainer _container;
        private Transform _pourTransform;

        void Awake()
        {
            _container = GetComponent<LiquidContainer>();
            Transform existing = transform.Find("_DynamicPourPoint");
            if (existing != null)
            {
                _pourTransform = existing;
            }
            else
            {
                _pourTransform = new GameObject("_DynamicPourPoint").transform;
                _pourTransform.SetParent(transform, false);
            }

            _container.pourPoint = _pourTransform;
        }

        void LateUpdate()
        {
            UpdatePourPoint();
        }

        public void UpdatePourPoint()
        {
            Vector3 rimCentreWorld = transform.TransformPoint(
                new Vector3(0f, rimHeight, 0f));

            Vector3 containerUp = transform.up;
            float tiltAngle = Vector3.Angle(containerUp, Vector3.up);

            if (tiltAngle < 0.1f)
            {
                _pourTransform.position = rimCentreWorld;
                _pourTransform.rotation = Quaternion.LookRotation(
                    Vector3.down, transform.forward);
                return;
            }

            Vector3 tiltAxis = Vector3.Cross(containerUp, Vector3.up).normalized;
            Vector3 leanDir = Vector3.Cross(tiltAxis, containerUp).normalized;

            Vector3 pourWorld = rimCentreWorld + leanDir * rimRadius
                                               + leanDir * lipOffset;

            pourWorld.y = rimCentreWorld.y
                          - Mathf.Sin(tiltAngle * Mathf.Deg2Rad) * rimRadius;

            Quaternion pourRot = Quaternion.LookRotation(
                Vector3.down,
                -leanDir
            );

            _pourTransform.position = pourWorld;
            _pourTransform.rotation = pourRot;
        }


        public float GetPourStrength()
        {
            float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
            return Mathf.InverseLerp(tiltThreshold, maxTiltAngle, tiltAngle);
        }

        public bool IsPouring => GetPourStrength() > 0f;

        public Vector3 PourPosition => _pourTransform.position;
        public Vector3 PourDirection
        {
            get
            {
                Vector3 containerUp = transform.up;
                Vector3 tiltAxis = Vector3.Cross(containerUp, Vector3.up).normalized;
                Vector3 leanDir = Vector3.Cross(tiltAxis, containerUp).normalized;

                // Blend between pure-down (upright) and leaning-forward (tilted)
                float strength = GetPourStrength();
                return Vector3.Lerp(Vector3.down,
                                    (Vector3.down + leanDir * 0.6f).normalized,
                                    strength).normalized;
            }
        }


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            Vector3 rimCentre = transform.TransformPoint(
                new Vector3(0f, rimHeight, 0f));
            Gizmos.color = Color.cyan;
            DrawCircle(rimCentre, transform.up, rimRadius, 32);

            if (_pourTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_pourTransform.position, 0.005f);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(_pourTransform.position,
                                _pourTransform.position + PourDirection * 0.05f);
            }

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            DrawCircle(rimCentre, transform.up, rimRadius * 0.3f, 16);
        }

        private static void DrawCircle(Vector3 centre, Vector3 normal,
                                        float radius, int segments)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
            Vector3 prev = centre + rot * new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                Vector3 p = centre + rot * new Vector3(
                    Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
#endif
    }
}