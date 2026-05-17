using UnityEngine;

namespace VirtualChemLab
{
    public class DraggableStick : MonoBehaviour
    {
        private Vector3 _mOffset;
        private float _mZCoord;

        void OnMouseDown()
        {
            _mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
            _mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
        }

        void OnMouseDrag()
        {
            transform.position = GetMouseAsWorldPoint() + _mOffset;
        }

        private Vector3 GetMouseAsWorldPoint()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = _mZCoord;
            return Camera.main.ScreenToWorldPoint(mousePoint);
        }
    }
}