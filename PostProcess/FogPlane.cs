using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Script.renderfeatures
{
    public class FogPlane : MonoBehaviour
    {
        public FogMaskTexFeature fogMaskTexFeature;
        public FogRenderFeature fogRenderFeature;
        public UniversalRendererData forward;
        public bool fogFlag = false;
        private GameObject grid;

        public static bool isBlur = false;
        public Camera HeightCamera;
        public List<List<Matrix4x4>> maskMessage;


        public enum MaskStatus
        {
            start,
            done,
            update
        }

        public Mesh _mesh;
        public bool test = false;
        public MaskStatus status = MaskStatus.done;
        public RenderTexture output;
        private HashSet<int> _loadGridSet;


        public GameObject Grass;
        private Dictionary<int, List<MeshRenderer>> _grassByRange;

        void Start()
        {
            _loadGridSet = new HashSet<int>();
            _grassByRange = new Dictionary<int, List<MeshRenderer>>();
            for (int i = 0; i < Grass.transform.childCount; i++)
            {
                var grass = Grass.transform.GetChild(i);
                var pos = grass.position;
                var meshRenderer = grass.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    int gridIndex = (int) ((pos.x + 123) / 6) * 100 + (int) (pos.z - 129) / 6;
                    if (_grassByRange.ContainsKey(gridIndex))
                    {
                        _grassByRange[gridIndex].Add(meshRenderer);
                    }
                    else
                    {
                        _grassByRange.Add(gridIndex, new List<MeshRenderer> {meshRenderer});
                    }

                    meshRenderer.enabled = false;
                }
            }

            output = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
            maskMessage = new List<List<Matrix4x4>>();
            foreach (var feature in forward.rendererFeatures)
            {
                if (feature.GetType() == typeof(FogMaskTexFeature))
                {
                    fogMaskTexFeature = (FogMaskTexFeature) feature;
                    fogMaskTexFeature.fogMaskPass._fogPlane = this;
                    break;
                }
            }

            grid = GameObject.Find("grid");
            status = MaskStatus.update;
            UpdateMask();
        }

        private void Update()
        {
            if (isBlur)
            {
                if (fogRenderFeature.isActive)
                    fogRenderFeature.SetActive(false);
                return;
            }

            if (status == MaskStatus.update || test)
            {
                UpdateMask();
            }

            if (status == MaskStatus.done)
            {
                HeightCamera.enabled = false;
                fogMaskTexFeature.SetActive(false);
            }

            if (fogFlag != fogRenderFeature.isActive)
            {
                fogRenderFeature.SetActive(fogFlag);
            }
        }

        private void UpdateMask()
        {
            status = MaskStatus.start;
            HeightCamera.enabled = true;
            HeightCamera.targetTexture = output;
            fogMaskTexFeature.SetActive(true);
            Matrix4x4 view = HeightCamera.worldToCameraMatrix;
            Matrix4x4 pro = GL.GetGPUProjectionMatrix(HeightCamera.projectionMatrix, false);
            fogRenderFeature.settings._HeightViewMatrix = view;
            fogRenderFeature.settings._Projection = pro;
            if (grid && test)
            {
                maskMessage.Clear();
                var matrix4X4s = new List<Matrix4x4>();
                for (int i = 0; i < grid.transform.childCount; i++)
                {
                    var child = grid.transform.GetChild(i);
                    matrix4X4s.Add(child.localToWorldMatrix);
                }

                maskMessage.Add(matrix4X4s);
            }
        }

        public void UpdateMessage(List<Vector3> posList)
        {
            maskMessage.Clear();
            _loadGridSet.Clear();
       
            List<Matrix4x4> matrix4X4s = new List<Matrix4x4>();
            int count = 0;
            foreach (var pos in posList)
            {
                int gridIndex = (int) ((pos.x + 123) / 6) * 100 + (int) (pos.z - 129) / 6;
                _loadGridSet.Add(gridIndex);
                count++;
                var matr = new Matrix4x4();
                matr.SetTRS(pos + new Vector3(0, 20, 0), Quaternion.Euler(0, 0, 0), Vector3.one);
                matrix4X4s.Add(matr);
                if (count == 1023)
                {
                    count = 0;
                    maskMessage.Add(matrix4X4s);
                    matrix4X4s = new List<Matrix4x4>();
                }
            }

            if (matrix4X4s.Count != 0)
                maskMessage.Add(matrix4X4s);
            UpdateGrass();
            status = MaskStatus.update;
        }

        private void UpdateGrass()
        {
            foreach (var index in _loadGridSet)
            {
                if (_grassByRange.TryGetValue(index, out List<MeshRenderer> list))
                {
                    foreach (var renderer in list)
                    {
                        renderer.enabled = true;
                    }
                }
            }
        }

        private void OnDisable()
        {
            fogRenderFeature.SetActive(false);
        }

        private void OnDestroy()
        {
            fogMaskTexFeature.SetActive(false);
            fogRenderFeature.SetActive(false);
        }
    }
}