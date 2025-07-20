using UnityEngine;
using System.Collections.Generic;

namespace Framework.GameConsole
{
    public partial class GameConsole
    {
        public bool IsShowFps { get; set; }

        private void OnGUI_Entry()
        {
            GUISkin cachedGuiSkin = GUI.skin;
            Matrix4x4 cachedGuiMatrix = GUI.matrix;
            BeginUIResizing();
                
            var cachedColor = GUI.color;
            var rect = GetRect(0, 87, 120, 50);
            if (IsShowFps || IsOpenWindow)
            {
                var cachedLabelAlignment = GUI.skin.label.alignment;
                var cachedLabelFontSize = GUI.skin.label.fontSize;
                    
                var fps = (int) (1f / Time.unscaledDeltaTime);
                var color = fps < 20 ? Color.red : fps < 40 ? Color.yellow : Color.green;
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = color;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.skin.label.fontSize = 42;
                GUI.Label(rect, $"{fps}");
                    
                GUI.skin.label.alignment = cachedLabelAlignment;
                GUI.skin.label.fontSize = cachedLabelFontSize;
            }
            
            GUI.color = new Color(0, 0, 0, 0f);
            if (GUI.Button(rect, ""))
            {
                if(IsOpenWindow)
                    CloseWindow();
                else
                    OpenWindow();
            }
            GUI.color = cachedColor;
                
            EndUIResizing();
            GUI.skin = cachedGuiSkin;
            GUI.matrix = cachedGuiMatrix;
        }
        
        /// <summary> 计算了被缩放后的位置 </summary>
        public static Rect GetRect(float x, float y, float width, float height)
        {
            return new Rect(x - Instance.OffsetX, y - Instance.OffsetY, width, height);
        }
        public float OffsetX => _offset.x / _guiScaleFactor;
        public float OffsetY => _offset.y / _guiScaleFactor;
        private float _guiScaleFactor = -1.0f;
        private Vector3 _offset = Vector3.zero;
        private List<Matrix4x4> _stack = new();
        private void BeginUIResizing()
        {
            Vector2 nativeSize = _nativeResolution;
	
            _stack.Add (GUI.matrix);
            Matrix4x4 m = new Matrix4x4();
            var w = (float)Screen.width;
            var h = (float)Screen.height;
            var aspect = w / h;
            _offset = Vector3.zero;
            if(aspect < (nativeSize.x/nativeSize.y)) 
            { 
                //screen is taller
                _guiScaleFactor = (Screen.width/nativeSize.x);
                _offset.y += (Screen.height-(nativeSize.y*_guiScaleFactor))*0.5f;
            } 
            else 
            { 
                // screen is wider
                _guiScaleFactor = (Screen.height/nativeSize.y);
                _offset.x += (Screen.width-(nativeSize.x*_guiScaleFactor))*0.5f;
            }
	
            m.SetTRS(_offset,Quaternion.identity,Vector3.one*_guiScaleFactor);
            GUI.matrix *= m;	
        }

        private void EndUIResizing()
        {
            GUI.matrix = _stack[^1];
            _stack.RemoveAt (_stack.Count - 1);
        }
    }
}