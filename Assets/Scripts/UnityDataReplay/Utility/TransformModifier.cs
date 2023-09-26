using UnityEngine;

namespace UnityDataReplay
{
    public class TransformModifier : MonoBehaviour
    {
        [Header("Link transform components")]
        // position link
        public bool linkPosition;
        [ConditionalField(nameof(linkPosition))] public Transform parentPosition;
        public bool positionOffset;
        [ConditionalField(nameof(positionOffset))] public Vector3 posOffset = Vector3.zero;

        // rotation link
        public bool linkRotation;
        [ConditionalField(nameof(linkRotation))] public Transform parentRotation;
        public bool rotationOffset; // <<<<<<<<<< not sure how this works
        [ConditionalField(nameof(rotationOffset))] public Quaternion rotOffset = Quaternion.identity;

        // scale link
        public bool linkScale;
        [ConditionalField(nameof(linkScale))] public Transform parentScale;
        public bool scaleOffset;
        [ConditionalField(nameof(scaleOffset))] public Vector3 sOffset = Vector3.zero;

        [Header("Fixing transform components")]
        // Simple Fix position
        public bool fixPosition;
        [ConditionalField(nameof(fixPosition))] public bool xPosFix = true;
        [ConditionalField(nameof(fixPosition))] public bool yPosFix = true;
        [ConditionalField(nameof(fixPosition))] public bool zPosFix = true;

        private Vector3 _startPosition;
        // Simple Fix rotation
        public bool fixRotation;
        [ConditionalField(nameof(fixRotation))] public bool xRotFix = true;
        [ConditionalField(nameof(fixRotation))] public bool yRotFix = true;
        [ConditionalField(nameof(fixRotation))] public bool zRotFix = true;
        private Quaternion _startRotation;


        private void Awake()
        {
            var t = transform;
            _startPosition = t.position;
            _startRotation = t.rotation;
        }

        private void LateUpdate()
        {
            if (linkPosition)
            {
                transform.position = parentPosition.position;

                if (positionOffset)
                    transform.position += posOffset;
            }

            if (linkRotation)
            {
                transform.rotation = parentRotation.rotation;

                if (rotationOffset)
                    transform.rotation *= rotOffset;
            }

            if (linkScale)
            {
                transform.localScale = parentScale.localScale;

                if (scaleOffset)
                    transform.localScale += sOffset;
            }

            if (fixPosition) transform.position = FixPosition(transform.position);
            if (fixRotation) transform.rotation = FixRotation(transform.rotation);

        }

        private Vector3 FixPosition(Vector3 position)
        {
            if (xPosFix) position.x = _startPosition.x;
            if (yPosFix) position.y = _startPosition.y;
            if (zPosFix) position.z = _startPosition.z;
            return position;
        }
        
        private Quaternion FixRotation(Quaternion rotation)
        {
            var r = rotation.eulerAngles;
            var s = _startRotation.eulerAngles;
            if (xRotFix) r.x = s.x;
            if (yRotFix) r.y = s.y;
            if (zRotFix) r.z = s.z;
            return Quaternion.Euler(r);
        }
    }
}
