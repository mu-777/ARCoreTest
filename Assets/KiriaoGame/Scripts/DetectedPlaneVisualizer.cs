
namespace ARTestGame {

    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;
    using GoogleARCore;

    [DefaultExecutionOrder(-200)]
    public class DetectedPlaneVisualizer : MonoBehaviour {

        private DetectedPlane _detectedPlane;
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;


        public List<Vector3> MeshBoundaryVertices {
            get;
            private set;
        }

        public NavMeshBuildSource NavMeshSource {
            get;
            private set;
        }

        void Awake() {
            _mesh = new Mesh();
            _meshFilter = this.GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            MeshBoundaryVertices = new List<Vector3>();
        }

        void Start() {
        }

        public DetectedPlaneVisualizer Initialize(DetectedPlane plane) {
            _detectedPlane = plane;
            _meshRenderer.material.SetColor("_GridColor", Color.white);
            _meshRenderer.material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));
            Update();
            return this;
        }

        void Update() {
            if (_detectedPlane == null) {
                return;
            } else if (_detectedPlane.SubsumedBy != null) {
                Destroy(this.gameObject);
                return;
            } else if (_detectedPlane.TrackingState != TrackingState.Tracking) {
                _meshRenderer.enabled = false;
                return;
            }

            _meshRenderer.enabled = true;
            if (_UpdateMeshIfNeeded()) {
                _UpdateNavMesh();
            }
        }
        

        private void _UpdateNavMesh() {
            var navMeshSource = new NavMeshBuildSource();
            navMeshSource.shape = NavMeshBuildSourceShape.Mesh;
            navMeshSource.sourceObject = _meshFilter.sharedMesh;
            navMeshSource.transform = Matrix4x4.TRS(_detectedPlane.CenterPose.position,
                                                    _detectedPlane.CenterPose.rotation,
                                                    Vector3.one);
            navMeshSource.area = 0;
            NavMeshSource = navMeshSource;
            print("NavMeshSource update");
        }

        private bool _UpdateMeshIfNeeded() {
            var meshVertices = new List<Vector3>();
            var meshIndices = new List<int>();
            var newMeshBoundaryVertices = new List<Vector3>();

            _detectedPlane.GetBoundaryPolygon(newMeshBoundaryVertices);
            if (AreVerticesListsEqual(MeshBoundaryVertices, newMeshBoundaryVertices)) {
                return false;
            }
            MeshBoundaryVertices = newMeshBoundaryVertices;

            var planeCenter = _detectedPlane.CenterPose.position;
            var planeNormal = _detectedPlane.CenterPose.rotation * Vector3.up;

            _meshRenderer.material.SetVector("_PlaneNormal", planeNormal);

            meshVertices.Add(planeCenter);
            meshVertices.AddRange(newMeshBoundaryVertices);
            for (var i = 0; i < newMeshBoundaryVertices.Count - 1; i++) {
                meshIndices.AddRange(new int[] { 0, i + 1, i + 2 });
            }
            meshIndices.AddRange(new int[] { 0, newMeshBoundaryVertices.Count, 1});

            _mesh.Clear();
            _mesh.SetVertices(meshVertices);
            _mesh.SetTriangles(meshIndices, 0);
            _meshFilter.mesh = _mesh;
            _meshCollider.sharedMesh = _mesh;
            return true;
        }

        private bool AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList) {
            if (firstList.Count != secondList.Count) {
                return false;
            }
            for (int i = 0; i < firstList.Count; i++) {
                if (firstList[i] != secondList[i]) {
                    return false;
                }
            }
            return true;
        }

    }
}

