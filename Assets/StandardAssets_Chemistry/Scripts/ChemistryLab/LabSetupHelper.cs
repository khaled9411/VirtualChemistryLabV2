using UnityEngine;

namespace VirtualChemLab
{
    public class LabSetupHelper : MonoBehaviour
    {
        [System.Serializable]
        public class ContainerSlot
        {
            public string chemicalId = "HCl";
            public Transform spawnPoint;
            public GameObject cylinderPrefab;
            [Range(0.1f, 1f)]
            public float initialFill = 0.6f;
        }

        [Header("Containers to Spawn")]
        public ContainerSlot[] containers;

        [Header("Auto-find ReactionManager")]
        public ReactionManager reactionManager;

        void Start()
        {
            if (reactionManager == null)
                reactionManager = FindAnyObjectByType<ReactionManager>();

            foreach (var slot in containers)
            {
                if (slot.cylinderPrefab == null || slot.spawnPoint == null) continue;

                GameObject go = Instantiate(
                    slot.cylinderPrefab,
                    slot.spawnPoint.position,
                    slot.spawnPoint.rotation
                );

                LiquidContainer lc = go.GetComponent<LiquidContainer>();
                if (lc == null) lc = go.AddComponent<LiquidContainer>();

                lc.chemicalId = slot.chemicalId;
                lc.fillLevel = slot.initialFill;

                PouringSystem ps = go.GetComponent<PouringSystem>();
                if (ps == null) ps = go.AddComponent<PouringSystem>();
            }
        }
    }
}