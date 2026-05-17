using UnityEngine;
using System.Collections;

namespace VirtualChemLab
{
    public class BunsenBurner : MonoBehaviour
    {
        [Header("Snap Zone")]
        public Transform snapPoint;             // Where the beaker sits
        public float snapRadius = 0.4f;         // Drop detection radius
        public float snapSpeed = 8f;

        [Header("Flame Visuals")]
        public ParticleSystem flameParticles;
        public Light flameLight;
        public float flameLightIntensity = 1.2f;
        public Gradient flameLightColorOverTemp;

        [Header("State")]
        public bool isLit = true;

        private LiquidContainer _dockedContainer;
        private HeatingSystem _dockedHeating;

        public bool HasContainer => _dockedContainer != null;

        void Start()
        {
            SetFlame(isLit);
        }

        void Update()
        {
            if (flameLight && _dockedHeating != null)
            {
                flameLight.intensity = Mathf.Lerp(
                    0.4f, flameLightIntensity,
                    _dockedHeating.NormalizedTemp
                );
            }
        }

        public bool TryDock(LiquidContainer container)
        {
            if (_dockedContainer != null || !container.isBeaker) return false;

            _dockedContainer = container;
            _dockedHeating = container.GetComponent<HeatingSystem>();
            if (_dockedHeating == null)
                _dockedHeating = container.gameObject.AddComponent<HeatingSystem>();

            StartCoroutine(SnapToPoint(container.transform));
            _dockedHeating.isOnFlame = isLit;

            Debug.Log($"[BunsenBurner] Docked: {container.ChemicalId}");
            return true;
        }

        public void Undock()
        {
            if (_dockedContainer == null) return;

            if (_dockedHeating != null)
            {
                _dockedHeating.isOnFlame = false;
                _dockedHeating.ResetReactionState();
            }

            _dockedContainer = null;
            _dockedHeating = null;
        }

        private IEnumerator SnapToPoint(Transform target)
        {
            Vector3 dest = snapPoint != null ? snapPoint.position : transform.position;
            Quaternion destRot = snapPoint != null ? snapPoint.rotation : transform.rotation;

            while (Vector3.Distance(target.position, dest) > 0.005f)
            {
                target.position = Vector3.Lerp(target.position, dest, Time.deltaTime * snapSpeed);
                target.rotation = Quaternion.Slerp(target.rotation, destRot, Time.deltaTime * snapSpeed);
                yield return null;
            }

            target.position = dest;
            target.rotation = destRot;
        }

        public void SetFlame(bool lit)
        {
            isLit = lit;
            if (flameParticles)
            {
                if (lit) flameParticles.Play();
                else flameParticles.Stop();
            }
            if (flameLight) flameLight.enabled = lit;

            if (_dockedHeating != null)
                _dockedHeating.isOnFlame = lit;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
            Vector3 center = snapPoint ? snapPoint.position : transform.position;
            Gizmos.DrawWireSphere(center, snapRadius);
        }
    }
}