
namespace ARTestGame {

    using System.Collections;
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

    public class CharacterGenerator : MonoBehaviour {

        [SerializeField]
        private GameObject _characterVRMPrefab;
        [SerializeField]
        private RuntimeAnimatorController _animatiorController;
        [SerializeField]
        private float _characterScale = 0.1f;
        private CharacterManager _characterManager;


        private void Start() {
            var charaMgrGameObject = new GameObject("ARCoreTestGameManager");
            _characterManager = charaMgrGameObject.AddComponent<CharacterManager>();

        }

        public void FirstPlaneGenerated(DetectedPlane dp) {
            //var chara = Instantiate(_characterVRMPrefab, dp.CenterPose.position,
            //                        dp.CenterPose.rotation, _characterManager.transform);
            //chara.transform.localScale = Vector3.one * _characterScale;
            //_characterManager.Initialize(chara, _animatiorController);
            StartCoroutine("WaitForTouchToCharacterInstantiate");
        }

        IEnumerator WaitForTouchToCharacterInstantiate() {
            while (true) {
                Touch touch;
                print(Input.touchCount);
                if (Input.touchCount == 1 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began) {
                    TrackableHit hit;
                    TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                                                      TrackableHitFlags.FeaturePointWithSurfaceNormal;
                    if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit)) {
                        if (hit.Trackable is DetectedPlane) {
                            var chara = Instantiate(_characterVRMPrefab, hit.Pose.position, hit.Pose.rotation,
                                                    _characterManager.transform);
                            chara.transform.localScale = Vector3.one * _characterScale;
                            _characterManager.Initialize(chara, _animatiorController);
                            break;
                        }
                    }
                }
                yield return null;
            }
        }
    }
}
