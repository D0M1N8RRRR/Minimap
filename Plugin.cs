using System;
using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minimap
{
    [BepInPlugin("minimap.grzybeksigma", "Minimap Mod", "1.0.1")]
    // this code is not stolen graze or what ever ur name is

    public class Mod : BaseUnityPlugin
    {
        private bool _initialized;
        private bool _showMinimap = true;
        private bool _showFullMap = false;
        private int _minimapCorner = 0;

        private GameObject _minimapCamObj;
        private Camera _minimapCam;
        private RenderTexture _minimapRenderTexture;

        private Vector3 _fullMapOffset = Vector3.zero;
        private float _fullMapZoom = 10f;
        private float _zoomSpeed = 20f;
        private float _panSpeed = 20f;
        private float _minZoom = 5f;
        private float _maxZoom = 50f;

        private float _minimapUpdateInterval = 0.1f;
        private float _minimapTimer = 0f;

        void Awake()
        {
            GorillaTagger.OnPlayerSpawned(() =>
            {
                _initialized = true;
                SetupMinimap();
            });
        }

        void Update()
        {
            if (!_initialized) return;

            HandleInput();
            UpdateCameraPosition();

            _minimapTimer += Time.deltaTime;
            if (_minimapTimer >= _minimapUpdateInterval)
            {
                _minimapTimer = 0f;
                _minimapCam.Render();
            }
        }

        private void HandleInput()
        {
            if (Keyboard.current.eKey.wasPressedThisFrame && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed))
            {
                _showMinimap = !_showMinimap;
                _minimapCamObj.SetActive(_showMinimap || _showFullMap);
            }

            if (Keyboard.current.qKey.wasPressedThisFrame && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed))
                _minimapCorner = (_minimapCorner + 1) % 4;

            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                _showFullMap = !_showFullMap;
                _minimapCamObj.SetActive(_showFullMap);
                if (!_showFullMap)
                {
                    _fullMapOffset = Vector3.zero;
                    _fullMapZoom = 10f;
                }
            }

            if (_showFullMap)
            {
                Vector3 inputDirection = Vector3.zero;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputDirection.z += 1;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputDirection.z -= 1;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputDirection.x += 1;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputDirection.x -= 1;
                _fullMapOffset += inputDirection.normalized * _panSpeed * Time.deltaTime;

                if (Keyboard.current.zKey.isPressed) _fullMapZoom = Mathf.Max(_minZoom, _fullMapZoom - _zoomSpeed * Time.deltaTime);
                if (Keyboard.current.xKey.isPressed) _fullMapZoom = Mathf.Min(_maxZoom, _fullMapZoom + _zoomSpeed * Time.deltaTime);
            }
        }

        private void UpdateCameraPosition()
        {
            GameObject localGorilla = GameObject.Find("Local Gorilla Player");
            if (localGorilla == null) return;

            Vector3 basePosition = localGorilla.transform.position + new Vector3(0, 20, 0);

            if (_showFullMap)
            {
                _minimapCamObj.transform.position = basePosition + _fullMapOffset;
                _minimapCam.orthographicSize = _fullMapZoom;
            }
            else
            {
                _minimapCamObj.transform.position = basePosition;
                _minimapCam.orthographicSize = 10f;
            }
            _minimapCamObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        private void SetupMinimap()
        {
            _minimapCamObj = new GameObject("MinimapCamera");
            _minimapCam = _minimapCamObj.AddComponent<Camera>();
            _minimapCam.orthographic = true;
            _minimapCam.orthographicSize = 10;
            _minimapCam.depth = 1;
            _minimapCam.cullingMask = LayerMask.GetMask("Default", "Minimap");

            _minimapRenderTexture = new RenderTexture(512, 512, 16);
            _minimapRenderTexture.filterMode = FilterMode.Bilinear;
            _minimapCam.targetTexture = _minimapRenderTexture;

            _minimapCamObj.SetActive(_showMinimap);
        }

        private Rect GetMinimapRect()
        {
            int size = 300;
            int margin = 10;
            float x = 0, y = 0;
            switch (_minimapCorner)
            {
                case 0: x = Screen.width - size - margin; y = margin; break;
                case 1: x = Screen.width - size - margin; y = Screen.height - size - margin; break;
                case 2: x = margin; y = Screen.height - size - margin; break;
                case 3: x = margin; y = margin; break;
            }
            return new Rect(x, y, size, size);
        }

        void OnGUI()
        {
            if (_showMinimap && _minimapRenderTexture != null)
            {
                GUI.DrawTexture(_showFullMap ? new Rect(0, 0, Screen.width, Screen.height) : GetMinimapRect(), _minimapRenderTexture, ScaleMode.ScaleToFit, false);
            }
        }
    }
}
