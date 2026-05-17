using UnityEngine;
using System.Collections;

namespace VirtualChemLab
{
    [RequireComponent(typeof(LiquidContainer))]
    public class HeatingSystem : MonoBehaviour
    {
        [Header("Temperature")]
        public float currentTempC = 22f;
        public float heatingRatePerSecond = 15f;   // °C/s while on flame
        public float coolingRatePerSecond = 8f;    // °C/s while off flame
        public float ambientTempC = 22f;
        public float maxTempC = 300f;

        [Header("State")]
        public bool isOnFlame = false;

        private LiquidContainer _container;
        private bool _reactionTriggered = false;
        private bool _reactionRunning = false;
        private Coroutine _heatCoroutine;

        public float NormalizedTemp => currentTempC / maxTempC;

        void Awake()
        {
            _container = GetComponent<LiquidContainer>();
        }

        void Update()
        {
            if (isOnFlame)
            {
                currentTempC = Mathf.MoveTowards(
                    currentTempC,
                    maxTempC,
                    heatingRatePerSecond * Time.deltaTime
                );
            }
            else
            {
                currentTempC = Mathf.MoveTowards(
                    currentTempC,
                    ambientTempC,
                    coolingRatePerSecond * Time.deltaTime
                );
            }

            if (isOnFlame && !_reactionTriggered && !_reactionRunning)
                CheckForThermalReaction();
        }

        private void CheckForThermalReaction()
        {
            if (_container.IsEmpty) return;

            string chemId = _container.ChemicalId;

            if (ThermalReactionDatabase.TryGetThermalReaction(chemId, out ThermalReactionResult result))
            {
                if (currentTempC >= result.activationTempC)
                {
                    _reactionTriggered = true;
                    StartCoroutine(RunThermalReaction(result));
                }
            }
        }

        private IEnumerator RunThermalReaction(ThermalReactionResult result)
        {
            _reactionRunning = true;

            ReactionManager.Instance?.OnThermalReaction(result, _container);

            Debug.Log($"[HeatingSystem] Thermal reaction started: {result.logMessage}");

            float elapsed = 0f;
            float duration = result.reactionDurationSec;

            while (elapsed < duration)
            {
                if (!isOnFlame)
                {
                    yield return new WaitUntil(() => isOnFlame);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Reaction complete
            _container.PlayReaction(result, 0f);

            Debug.Log($"[HeatingSystem] Thermal reaction complete: {result.productName}");

            _reactionRunning = false;
        }

        public void ResetReactionState()
        {
            _reactionTriggered = false;
            _reactionRunning = false;
        }
    }
}