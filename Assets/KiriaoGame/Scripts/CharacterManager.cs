

namespace ARTestGame {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;
    using GoogleARCore;

//#if UNITY_EDITOR
//    // NOTE:
//    // - InstantPreviewInput does not support `deltaPosition`.
//    // - InstantPreviewInput does not support input from
//    //   multiple simultaneous screen touches.
//    // - InstantPreviewInput might miss frames. A steady stream
//    //   of touch events across frames while holding your finger
//    //   on the screen is not guaranteed.
//    // - InstantPreviewInput does not generate Unity UI event system
//    //   events from device touches. Use mouse/keyboard in the editor
//    //   instead.
//    using Input = GoogleARCore.InstantPreviewInput;
//#endif

    public class CharacterManager : MonoBehaviour {

        private class CharacterController {
            private enum AnimatorPoseState {
                Stay = 0, Walk = 1, Run = 2, Jump = 3
            }

            private NavMeshAgent _navMeshAgent;
            private Animator _animator;
            private AnimatorPoseState _currentState;

            private float _characterVelocity = 0.05f;

            public CharacterController(NavMeshAgent characterNavMeshAgent, Animator characterAnimator) {
                _navMeshAgent = characterNavMeshAgent;
                _navMeshAgent.speed = 0.01f;
                _navMeshAgent.angularSpeed = 90.0f;
                _navMeshAgent.acceleration = 4.0f;
                _navMeshAgent.autoBraking = true;
                _navMeshAgent.radius = 0.5f;
                _navMeshAgent.height = 1f;

                _animator = characterAnimator;
                _currentState = AnimatorPoseState.Stay;
            }

            public bool PlacingOnNavMesh() {
                print(_navMeshAgent.isOnNavMesh);
                _navMeshAgent.enabled = false;

                var tf = this._navMeshAgent.gameObject.transform;
                //TrackableHit trackableHit;
                //if (Frame.Raycast(tf.position, -tf.up, out trackableHit) && trackableHit.Trackable is DetectedPlane) {
                //    print("placed! @ Frame");
                //    tf.position = trackableHit.Pose.position;
                //    _navMeshAgent.enabled = true;
                //    return true;
                //}
                RaycastHit raycastHit;
                if (Physics.Raycast(tf.position, -tf.up, out raycastHit)) {
                    print("placed! @ Raycast");
                    _navMeshAgent.Warp(raycastHit.point);
                    _navMeshAgent.enabled = true;
                    return true;
                }
                print("placing...");
                return false;
            }

            public bool NavMeshRaycast(Vector3 targetPosition, out NavMeshHit hit) {
                return _navMeshAgent.Raycast(targetPosition, out hit);
            }

            public void InitialMotion() {
                var tf = this._navMeshAgent.gameObject.transform;
                tf.Translate(tf.up * 0.1f);
            }

            public void Stay() {
                this.Move(0f, AnimatorPoseState.Stay);
            }

            public void Run(float forwardRate) {
                this.Move(forwardRate, AnimatorPoseState.Run);
            }

            public void Walk(float forwardRate) {
                this.Move(forwardRate, AnimatorPoseState.Walk);
            }

            public void Jump() {
                this.ChangeAnimStateIfNeeded(AnimatorPoseState.Jump);
            }

            private void Move(float forwardRate, AnimatorPoseState animState) {
                if (_navMeshAgent == null) {
                    print("_navMeshAgent is null");
                }

                print(_navMeshAgent.pathStatus != NavMeshPathStatus.PathInvalid);
                print(_navMeshAgent.isOnNavMesh);


                //this.ChangeAnimStateIfNeeded(animState);
                if (forwardRate == 0f) {
                    return;
                }
                var offset = new Vector3(0f, 0f, forwardRate * _characterVelocity * Time.deltaTime);
                //_navMeshAgent.gameObject.transform.Translate(offset);
                _navMeshAgent.enabled = true;
                _navMeshAgent.Move(offset);
            }

            private void ChangeAnimStateIfNeeded(AnimatorPoseState newPoseState) {
                //if (newPoseState == _currentState) {
                //    return;
                //}
                _animator.SetInteger("PoseState", (int)newPoseState);
                _currentState = newPoseState;
            }
        }

        private CharacterController _charaController;
        private bool _isCharacterInitialized = false;

        public void Initialize(GameObject characterGameObject, RuntimeAnimatorController animatiorController) {
            print("Initialized");
            var characterNavMeshAgent = characterGameObject.AddComponent<NavMeshAgent>();
            var characterAnimator = characterGameObject.GetComponent<Animator>();
            characterAnimator.runtimeAnimatorController = animatiorController;
            _charaController = new CharacterController(characterNavMeshAgent, characterAnimator);
            _charaController.InitialMotion();
            StartCoroutine("PlacingOnNavMesh");
            //_isCharacterInitialized = true;
        }

        private IEnumerator PlacingOnNavMesh() {
            print("PlacingOnNavMesh Coroutine");

            while (!_charaController.PlacingOnNavMesh()) {
                print("Fail PlacingOnNavMesh");
                yield return new WaitForSeconds(0.1f);
            }
            _isCharacterInitialized = true;
        }

        void Update() {
            if (!_isCharacterInitialized) {
                return;
            }

#if UNITY_EDITOR
            var forwardInput = Input.GetKey(KeyCode.UpArrow) ? 1.0f : Input.GetKey(KeyCode.DownArrow) ? -1.0f : 0f;
#else
            var forwardInput = Input.GetAxis("Vertical");
#endif
            if (Mathf.Abs(forwardInput) < 0.01f) {
                _charaController.Stay();
            } else if (Mathf.Abs(forwardInput) > 0.7f) {
                _charaController.Run(forwardInput);
            } else {
                _charaController.Walk(forwardInput);
            }
        }

    }
}
