using System;
using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minimap
{
    [BepInPlugin("minimap.grzybeksigma", "Minimap Mod", "1.1.6")]
    public class Mod : BaseUnityPlugin
    {

        private bool _initialized;
        private bool _showMinimap;
        private int _minimapCorner = 0;

        private GameObject _minimapCamObj;
        private Camera _minimapCam;
        private RenderTexture _minimapRenderTexture;

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

            if (Keyboard.current.eKey.wasPressedThisFrame &&
                (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed))
            {
                _showMinimap = !_showMinimap;
            }

            if (Keyboard.current.qKey.wasPressedThisFrame &&
                (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed))
            {
                _minimapCorner = (_minimapCorner + 1) % 4;
            }

            GameObject localGorilla = GameObject.Find("Local Gorilla Player");
            if (localGorilla != null)
            {
                _minimapCamObj.transform.position = localGorilla.transform.position + new Vector3(0, 20, 0);
                _minimapCamObj.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }

        private void SetupMinimap()
        {
            _minimapCamObj = new GameObject("MinimapCamera");
            _minimapCam = _minimapCamObj.AddComponent<Camera>();
            _minimapCam.orthographic = true;
            _minimapCam.orthographicSize = 10;
            _minimapCam.depth = 1;
            _minimapCam.cullingMask = LayerMask.GetMask("Default");

            _minimapRenderTexture = new RenderTexture(256, 256, 16);
            _minimapCam.targetTexture = _minimapRenderTexture;
        }

        private Rect GetMinimapRect()
        {
            int size = 350;
            int margin = 10;
            float x = 0, y = 0;
            switch (_minimapCorner)
            {
                case 0:
                    x = Screen.width - size - margin;
                    y = margin;
                    break;
                case 1:
                    x = Screen.width - size - margin;
                    y = Screen.height - size - margin;
                    break;
                case 2:
                    x = margin;
                    y = Screen.height - size - margin;
                    break;
                case 3:
                    x = margin;
                    y = margin;
                    break;
            }
            return new Rect(x, y, size, size);
        }

        void OnGUI()
        {
            if (_showMinimap && _minimapRenderTexture != null)
            {
                GUI.DrawTexture(GetMinimapRect(), _minimapRenderTexture, ScaleMode.ScaleToFit, false);
            }
        }
    }
}
