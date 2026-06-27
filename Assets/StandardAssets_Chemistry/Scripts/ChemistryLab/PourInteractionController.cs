using UnityEngine;
using System.Collections;

namespace VirtualChemLab
{

    [RequireComponent(typeof(LiquidContainer))]
    [RequireComponent(typeof(DynamicPourPoint))]
    public class PourInteractionController : MonoBehaviour
    {

        [Header("Flow")]
        public float maxFlowRate = 0.15f;
        public float minTransferForReaction = 0.02f;

        [Header("Detection")]
        public LayerMask containerMask = 0;
        public float raycastDistance = 1.5f;
        public float detectionRadius = 0.04f;

        [Header("Particles")]
        public bool alignParticlesToPourDirection = true;
        public float particleSpeedScale = 1.5f;

        private LiquidContainer _source;
        private DynamicPourPoint _pourPoint;

        private LiquidContainer _currentTarget;
        private float _accumulatedTransfer;
        private bool _isPouring;

        void Awake()
        {
            _source = GetComponent<LiquidContainer>();
            _pourPoint = GetComponent<DynamicPourPoint>();
        }

        void Update()
        {
            if (_source.IsEmpty) return;

            bool shouldPour = _pourPoint.IsPouring;

            if (shouldPour && !_isPouring) OnPourStart();
            if (!shouldPour && _isPouring) OnPourEnd();

            if (_isPouring)
                HandlePouring(Time.deltaTime);
        }

        private void OnPourStart()
        {
            _isPouring = true;
            _accumulatedTransfer = 0f;
            _source.StartPourParticles();
            Debug.Log("[PourInteraction] Pour started.");
        }

        private void OnPourEnd()
        {
            _isPouring = false;
            _source.StopPourParticles();

            if (_currentTarget != null && _accumulatedTransfer >= minTransferForReaction)
                TriggerReaction(_currentTarget, _accumulatedTransfer);

            _currentTarget = null;
            _accumulatedTransfer = 0f;
            Debug.Log("[PourInteraction] Pour ended.");
        }

        private void HandlePouring(float dt)
        {
            AlignParticles();

            LiquidContainer target = FindTargetBeaker();
            if (target != _currentTarget)
            {
                if (_currentTarget != null && _accumulatedTransfer >= minTransferForReaction)
                    TriggerReaction(_currentTarget, _accumulatedTransfer);

                _accumulatedTransfer = 0f;
                _currentTarget = target;
            }

            float strength = _pourPoint.GetPourStrength();
            float flowThisFrame = maxFlowRate * strength * dt;
            float poured = _source.Pour(flowThisFrame);

            if (_currentTarget != null)
            {
                _currentTarget.FillWithoutReaction(_source.ChemicalId, poured);
                _accumulatedTransfer += poured;
            }
        }

        private void AlignParticles()
        {
            if (!alignParticlesToPourDirection) return;
            if (_source.pourParticles == null) return;

            var main = _source.pourParticles.main;
            var velocity = _source.pourParticles.velocityOverLifetime;

            float speed = particleSpeedScale * _pourPoint.GetPourStrength();
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed);

            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            Vector3 dir = _pourPoint.PourDirection * speed;
            velocity.x = new ParticleSystem.MinMaxCurve(dir.x);
            velocity.y = new ParticleSystem.MinMaxCurve(dir.y);
            velocity.z = new ParticleSystem.MinMaxCurve(dir.z);
        }

        private LiquidContainer FindTargetBeaker()
        {
            Vector3 origin = _pourPoint.PourPosition;
            Vector3 direction = _pourPoint.PourDirection;

            RaycastHit[] hits = Physics.SphereCastAll(
                origin, detectionRadius, direction,
                raycastDistance, containerMask,
                QueryTriggerInteraction.Collide);

            LiquidContainer best = null;
            float bestDist = Mathf.Infinity;

            foreach (var hit in hits)
            {
                if (hit.transform.IsChildOf(transform) ||
                    hit.transform == transform) continue;

                var lc = hit.collider.GetComponentInParent<LiquidContainer>();
                if (lc == null) continue;
                if (!lc.isBeaker) continue;

                if (hit.distance < bestDist)
                {
                    bestDist = hit.distance;
                    best = lc;
                }
            }

            return best;
        }

        private void TriggerReaction(LiquidContainer target, float amount)
        {
            ReactionResult result;

            ChemicalDatabase.TryGetReaction(
                _source.ChemicalId, target.ChemicalId, out result);

            if (result != null)
            {
                target.PlayReaction(result, amount);
                Debug.Log($"[PourInteraction] Reaction triggered: " +
                          $"{_source.ChemicalId} + {target.ChemicalId} " +
                          $"to {result.productName}  (volume {amount:F3})");
            }
            else
            {
                Debug.Log($"[PourInteraction] No reaction defined for " +
                          $"{_source.ChemicalId} + {target.ChemicalId}.");
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (_pourPoint == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_pourPoint.PourPosition, detectionRadius);
            Gizmos.DrawLine(_pourPoint.PourPosition,
                            _pourPoint.PourPosition +
                            _pourPoint.PourDirection * raycastDistance);
        }
#endif
    }
}