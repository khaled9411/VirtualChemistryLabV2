using UnityEngine;

namespace VirtualChemLab
{
    public class FlameTestInteractable : MonoBehaviour
    {
        public enum InteractableType { AcidBeaker, SaltDish, BunsenFlame }

        [Header("Interactable Type")]
        public InteractableType objectType;

        [Header("Salt (only used when objectType == SaltDish)")]
        public string saltId;

        [Header("Controller Reference")]
        public FlameTestController controller;

        private void Awake()
        {
            if (controller == null)
                controller = FindAnyObjectByType<FlameTestController>();
        }

        public void Interact()
        {
            if (controller == null)
            {
                Debug.LogWarning("[FlameTestInteractable] No FlameTestController found in scene.");
                return;
            }

            switch (objectType)
            {
                case InteractableType.AcidBeaker:
                    controller.CleanStick();
                    break;

                case InteractableType.SaltDish:
                    controller.LoadSalt(saltId);
                    break;

                case InteractableType.BunsenFlame:
                    controller.TestFlame();
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("WireTip"))
                Interact();
        }
    }
}