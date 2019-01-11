
namespace ARTestGame {

    using System.Collections;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.Events;
    using GoogleARCore;

    [System.Serializable]
    public class FirstPlaneGeneratedEvent : UnityEvent<DetectedPlane> { };

    // Build and update a localized navmesh from the sources marked by NavMeshSourceTag
    [DefaultExecutionOrder(-102)]
    public class PlaneGenerator : MonoBehaviour {
        [SerializeField]
        private GameObject _detectedPlanePrefab;
        [SerializeField]
        private GameObject _searchingForPlaneUI;
        [SerializeField]
        private FirstPlaneGeneratedEvent _firstPlaneGeneratedEvents;

        private NavMeshData _navMesh;
        private NavMeshDataInstance _navMeshInstance;

        private List<DetectedPlaneVisualizer> _planeVizs = new List<DetectedPlaneVisualizer>();
        private NavMeshBuildSettings _defaultBuildSettings;

        public void Start() {
            _searchingForPlaneUI.SetActive(true);
            _defaultBuildSettings = NavMesh.GetSettingsByID(0);

            _firstPlaneGeneratedEvents.AddListener((DetectedPlane dp) => {
                _searchingForPlaneUI.SetActive(false);
            });
            _firstPlaneGeneratedEvents.AddListener((DetectedPlane dp) => {
                StartCoroutine("UpdateNavMeshAsync");
            });
        }

        void OnEnable() {
            _navMesh = new NavMeshData();
            _navMeshInstance = NavMesh.AddNavMeshData(_navMesh);
        }

        void OnDisable() {
            // Unload navmesh and clear handle
            _navMeshInstance.Remove();
        }

        public void Update() {
            if (Session.Status != SessionStatus.Tracking) {
                return;
            }
            DetectNewPlanes();
        }

        IEnumerator UpdateNavMeshAsync() {
            while (true) {
                if (_planeVizs.Count < 1) {
                    yield return new AsyncOperation();
                }
                var navMeshBuildSources = _planeVizs.Select(planeViz => (NavMeshBuildSource)planeViz.NavMeshSource).ToList();
                var navMeshBuildBounds = CalcNavMeshBuildBounds();
                yield return NavMeshBuilder.UpdateNavMeshDataAsync(_navMesh, _defaultBuildSettings, navMeshBuildSources, navMeshBuildBounds);
                print("NavMeshBuilder updated");
                print(_navMesh.sourceBounds.center);
            }
        }

        private Bounds CalcNavMeshBuildBounds() {
            var boundaryVerteces = _planeVizs.Aggregate(new Vector3[] { }, (sumArray, planeViz) => sumArray.Concat(planeViz.MeshBoundaryVertices).ToArray());
            var center = boundaryVerteces.Aggregate(Vector3.zero, (sum, vertex) => sum + vertex);
            var size = new Vector3(boundaryVerteces.Select(vertex => Mathf.Abs(vertex.x - center.x)).ToArray().Max(),
                                   boundaryVerteces.Select(vertex => Mathf.Abs(vertex.y - center.y)).ToArray().Max(),
                                   boundaryVerteces.Select(vertex => Mathf.Abs(vertex.z - center.z)).ToArray().Max()) * 2.0f;
            return new Bounds(center, size);
        }

        private void DetectNewPlanes() {
            List<DetectedPlane> newPlanes = new List<DetectedPlane>();
            Session.GetTrackables<DetectedPlane>(newPlanes, TrackableQueryFilter.New);

            if (newPlanes.Count == 0) {
                return;
            }

            if (_planeVizs.Count == 0 && newPlanes.Count > 0) {
                _firstPlaneGeneratedEvents.Invoke(newPlanes[0]);
            }

            foreach (var plane in newPlanes) {
                var planeObject = Instantiate(_detectedPlanePrefab, Vector3.zero, Quaternion.identity, this.transform);
                _planeVizs.Add(planeObject.GetComponent<DetectedPlaneVisualizer>().Initialize(plane));
            }
            //UpdateNavMesh();
        }


        //private void UpdateUI() {
        //    List<DetectedPlane> allPlanes = new List<DetectedPlane>();
        //    Session.GetTrackables<DetectedPlane>(allPlanes);
        //    bool showSearchingUI = true;
        //    foreach (var plane in allPlanes) {
        //        if (plane.TrackingState == TrackingState.Tracking) {
        //            showSearchingUI = false;
        //            break;
        //        }
        //    }
        //    SearchingForPlaneUI.SetActive(showSearchingUI);
        //}

        void OnDrawGizmosSelected() {

            if (_navMesh) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_navMesh.sourceBounds.center, _navMesh.sourceBounds.size);
            }

            //Gizmos.color = Color.yellow;
            //var bounds = QuantizedBounds();
            //Gizmos.DrawWireCube(bounds.center, bounds.size);

            //Gizmos.color = Color.green;
            //var center = m_Tracked ? m_Tracked.position : transform.position;
            //Gizmos.DrawWireCube(center, m_Size);
        }
    }
}
