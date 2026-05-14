using UnityEngine;
using System.Collections;

namespace VirtualChemLab
{
    [RequireComponent(typeof(LiquidContainer))]
    public class PouringSystem : MonoBehaviour
    {
        [Header("Drag Settings")]
        public float dragHeight = 0f;
        public float dragFollowSpeed = 15f;

        [Header("Pour Detection")]
        public float pourProximity = 0.8f;
        public float pourAngle = 110f;
        public float tiltSpeed = 3f;
        public float pourRatePerSecond = 0.05f;

        [Header("Highlight")]
        public Renderer flaskRenderer;
        public Color highlightColor = new Color(1f, 1f, 0.3f, 1f);

        private LiquidContainer _container;
        private ReactionManager _reactionMgr;
        private Camera _cam;

        private bool _isDragging;
        private bool _isPouring;
        private Vector3 _dragOffset;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private LiquidContainer _targetContainer;
        private MaterialPropertyBlock _propBlock;
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        void Awake()
        {
            _container = GetComponent<LiquidContainer>();
            _cam = Camera.main;
            _propBlock = new MaterialPropertyBlock();
        }

        void Start()
        {
            _reactionMgr = ReactionManager.Instance;
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
        }

        void OnMouseDown()
        {
            if (_container.IsEmpty) return;

            _isDragging = true;
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;

            Vector3 screenPos = _cam.WorldToScreenPoint(transform.position);
            _dragOffset = transform.position - GetMouseWorldPos(screenPos.z);
        }

        void OnMouseDrag()
        {
            if (!_isDragging) return;

            Vector3 screenPos = _cam.WorldToScreenPoint(transform.position);
            Vector3 targetPos = GetMouseWorldPos(screenPos.z) + _dragOffset;
            targetPos.y = dragHeight;
            targetPos.z = _originalPosition.z;

            transform.position = Vector3.Lerp(
                transform.position, targetPos, Time.deltaTime * dragFollowSpeed
            );

            LiquidContainer nearContainer = FindNearestContainer();

            if (nearContainer != null && nearContainer != _container)
            {
                if (_targetContainer != nearContainer)
                {
                    ClearHighlight(_targetContainer);
                    _targetContainer = nearContainer;
                    SetHighlight(_targetContainer);
                }

                if (!_isPouring)
                    StartCoroutine(AutoTiltAndPour());
            }
            else
            {
                if (_targetContainer != null)
                {
                    ClearHighlight(_targetContainer);
                    _targetContainer = null;
                }
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, _originalRotation, Time.deltaTime * tiltSpeed
                );
            }
        }

        void OnMouseUp()
        {
            _isDragging = false;

            ClearHighlight(_targetContainer);
            _targetContainer = null;

            StartCoroutine(ReturnToOrigin());
        }

        private IEnumerator AutoTiltAndPour()
        {
            _isPouring = true;

            Quaternion pourRot = _originalRotation * Quaternion.Euler(0f, 0f, pourAngle);

            float elapsed = 0f;
            while (elapsed < 1f / tiltSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed * tiltSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, pourRot, t);
                yield return null;
            }

            yield return new WaitUntil(() =>
            {
                float angle = Quaternion.Angle(transform.rotation, _originalRotation);
                return angle > pourAngle * 0.65f;
            });

            _container.StartPourParticles();

            float totalPoured = 0f;
            while (_isDragging && _targetContainer != null && !_container.IsEmpty)
            {
                float delta = pourRatePerSecond * Time.deltaTime;
                float poured = _container.Pour(delta);
                totalPoured += poured;

                _reactionMgr?.OnLiquidPoured(_container, _targetContainer, poured);

                yield return null;
            }

            _container.StopPourParticles();
            _isPouring = false;
        }

        private IEnumerator ReturnToOrigin()
        {
            _container.StopPourParticles();
            _isPouring = false;

            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.5f);
                transform.position = Vector3.Lerp(startPos, _originalPosition, t);
                transform.rotation = Quaternion.Slerp(startRot, _originalRotation, t);
                yield return null;
            }

            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
        }

        private Vector3 GetMouseWorldPos(float z)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = z;
            return _cam.ScreenToWorldPoint(mousePos);
        }

        private LiquidContainer FindNearestContainer()
        {
            LiquidContainer nearest = null;
            float minDist = pourProximity;

            foreach (var lc in FindObjectsByType<LiquidContainer>(FindObjectsSortMode.None))
            {
                if (lc == _container) continue;
                float d = Vector3.Distance(transform.position, lc.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = lc;
                }
            }
            return nearest;
        }

        private void SetHighlight(LiquidContainer target)
        {
            if (target == null) return;
            Renderer r = target.GetComponentInChildren<Renderer>();
            if (r == null) return;
            r.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(EmissionColorID, highlightColor);
            r.SetPropertyBlock(_propBlock);
        }

        private void ClearHighlight(LiquidContainer target)
        {
            if (target == null) return;
            Renderer r = target.GetComponentInChildren<Renderer>();
            if (r == null) return;
            r.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(EmissionColorID, Color.black);
            r.SetPropertyBlock(_propBlock);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, pourProximity);
        }
    }
}