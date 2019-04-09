using System.Collections.Generic;
using UnityEngine;
using Liangddyy;
using System.Linq;
using UnityEditor;
using UnityEngine.Rendering;

namespace Liangddyy
{
    public class CurveEditorWindow : EditorWindow
    {
        [MenuItem("Tools/CurveUtil")]
        public static void InitWindow()
        {
            EditorWindow window = (CurveEditorWindow) EditorWindow.GetWindow(typeof(CurveEditorWindow));
            window.Show();
        }

        private static EaserDataWrapper _easer;

        private float DOT_SIZE = 2;

        private Rect _selectorRect;
        private Rect _canvasRect;

        private Vector2 _scrollPos;
        private Vector2 _mousePos;

        private bool _mouseClick = false;

        //private bool _mouseDrag = false;
        private bool _clearGuiTarget = false;
        private bool _repaint;
        private Texture2D _texture;
        private float _dotValue;

        private float _lastTime;

//    private const int SELECTOR_WIDTH = 250;
//    private const int CURVE_CANVAS_SIZE = 400;

        private const int max = 300;
        private float width;

        private int row; // 行
        private int column;

        private void OnGUI()
        {
            Setup();
            UpdateMouse();
            ClearGuiTarget();

            CalculateColmnRow();

            OnDraw(new Rect(0, 0, position.width - 3f, position.height - 3f));

            if (_repaint)
            {
                Repaint();
                _repaint = false;
            }
        }

        private void OnDraw(Rect rect)
        {
            _scrollPos = GUI.BeginScrollView(rect, _scrollPos,
                new Rect(0, 0, position.width, max * row));

            int tmpRow = 0, tmpColumn = 0; // 留一个格子 写配置
            DrawConfig(new Rect(tmpColumn * max, tmpRow * max, max, max));
            tmpColumn++;

            if (_easer.data.eases.Any())
            {
                for (var i = 0; i < _easer.data.eases.Length; i++)
                {
                    var tt = new Rect(tmpColumn * max, tmpRow * max, max, max);
                    DrawAnimationCurve(EaserEditorUtils.InsetRect(tt, 45), i);
                    drawCurve(tt, i);
                    tmpColumn++;
                    if (tmpColumn >= column)
                    {
                        tmpColumn = 0;
                        tmpRow++;
                    }
                }
            }

            GUI.EndScrollView();
        }

        private string _addNameTmp = "";
        private int _addSelectIndexTmp;

        private void DrawConfig(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginVertical();
            GUILayout.Space(20);
//        GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("Name：");
            _addNameTmp = EditorGUILayout.TextField(_addNameTmp);
            bool isAdd = GUILayout.Button("New Curve");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Select：");
            _addSelectIndexTmp = EditorGUILayout.Popup(_addSelectIndexTmp,
                _easer.data.eases.Select(x => x.name).ToArray(),
                GUILayout.Width(rect.width / 3));
            bool isCopy = GUILayout.Button("Copy");
            bool isDelete = GUILayout.Button("Delete");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
//        GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("1. 避免混乱，内置的曲线不允许编辑。");

            EditorGUILayout.LabelField("2. 如需编辑内置曲线，请Copy后操作。");
                
            GUILayout.EndVertical();
            GUILayout.EndArea();

            if (isAdd)
            {
                if (!string.IsNullOrEmpty(_addNameTmp))
                {
                    Add(_addNameTmp);
                }
            }

            if (isCopy)
            {
                List<EaserEaseObject> eases = new List<EaserEaseObject>(_easer.data.eases);
                var tmp = new EaserEaseObject(name);
                tmp.name = eases[_addSelectIndexTmp].name + "_Copy";
                tmp.curve = eases[_addSelectIndexTmp].curve;
                eases.Add(tmp);

                _easer.data.eases = eases.ToArray();
                _addSelectIndexTmp = _easer.data.eases.Length - 1;
                Debug.Log("已成功复制.");
            }

            if (isDelete)
            {
                if (_addSelectIndexTmp >= _easer.constTotal)
                    Remove(_addSelectIndexTmp);
            }
        }

        private void Add(string name)
        {
//        _easer.current = _easer.data.eases.Length;
            List<EaserEaseObject> eases = new List<EaserEaseObject>(_easer.data.eases);
            eases.Add(new EaserEaseObject(name));
            _easer.data.eases = eases.ToArray();
            _addSelectIndexTmp = _easer.data.eases.Length - 1;
            Debug.Log("添加成功.");
        }

        private void Remove(int index)
        {
            if (index < 0 || index >= _easer.data.eases.Length)
                return;

            List<EaserEaseObject> eases = new List<EaserEaseObject>(_easer.data.eases);
            eases.RemoveAt(index);
//            eases.RemoveAt(_easer.current);
            _easer.data.eases = eases.ToArray();
//            _easer.current -= 1;
//            if (_easer.current < 0 && _easer.data.eases.Length > 0) { _easer.current = 0; }
            _addSelectIndexTmp = 0;
        }


        private void CalculateColmnRow()
        {
            column = position.width / max >= 1f ? ((int) position.width) / max : 1;
            row = (_easer.data.eases.Length + 1) / column;
            if ((_easer.data.eases.Length + 1) % 2 == 1)
                row++; // 奇数还要加 一列。
        }

        private void ClearGuiTarget()
        {
            GUI.SetNextControlName("unfocus");
            EditorGUI.TextArea(new Rect(-100, -100, 1, 1), "");
            if (_clearGuiTarget) GUI.FocusControl("unfocus");
            _clearGuiTarget = false;
        }

        private void UpdateMouse()
        {
            _mousePos = Event.current.mousePosition;
            _mouseClick = false;
            //_mouseDrag = false;

            if (Event.current.isMouse && Event.current.button == 0)
            {
                if (Event.current.type == EventType.MouseDown)
                {
//                Debug.Log("点击事件检测");
                    _clearGuiTarget = true;
                    _mouseClick = true;
                    _repaint = true;
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    //_mouseDrag = true;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    //_mouseDrag = false;
                }
            }
        }

        private void Setup()
        {
            if (_easer == null)
            {
                Load();
            }

            if (_easer.data == null)
            {
                _easer.data = new EaserData();
            }

            if (_easer.data.eases == null)
            {
                _easer.data.eases = new EaserEaseObject[0];
            }
        }

        private void DrawAnimationCurve(Rect rect, int index)
        {
            if (index < _easer.constTotal) // 内置部分不允许修改。
            {
                var curve = new AnimationCurve(_easer.data.eases[index].curve.keys);
                EditorGUI.CurveField(rect, curve, Color.red, new Rect(0, 0, 1, 1));
            }
            else
                _easer.data.eases[index].curve = EditorGUI.CurveField(rect, _easer.data.eases[index].curve, Color.red,
                    new Rect(0, 0, 1, 1));
        }

        private void drawCurve(Rect rect, int index)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(1, 1);
                _texture.filterMode = FilterMode.Point;
                _texture.SetPixel(0, 0, new Color(1, 1, 1, 1));
                _texture.Apply();
            }

            if (EditorGUIUtility.isProSkin)
            {
                GUI.color = new Color(0.2f, 0.2f, 0.2f, 1);
            }
            else
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 1);
            }

            Rect bgRect = rect;
            bgRect.yMin += 1;
            bgRect.xMin += 1;
            GUI.DrawTexture(bgRect, _texture);
            EaserEditorUtils.DrawOutlineBox(bgRect, Color.black);

            if (index < _easer.constTotal)
            {
                EditorGUI.TextField(new Rect(rect.x + 5, rect.y + 5, 150, 20), "Ease." + _easer.data.eases[index].name);
            }
            else
            {
                _easer.data.eases[index].name = EditorGUI.TextField(new Rect(rect.x + 5, rect.y + 5, 150, 20),
                    _easer.data.eases[index].name);
            }

            rect = EaserEditorUtils.InsetRect(rect, 45);
            if (EditorGUIUtility.isProSkin)
            {
                EaserEditorUtils.DrawShadowBox(rect);
            }

            Color lineColor = new Color(0.15f, 0.15f, 0.15f, 1);
            Color offLineColor = new Color(0.15f, 0.15f, 0.15f, 0.3f);
            EaserEditorUtils.DrawGrid(rect, lineColor, offLineColor);

            AnimationCurve curve = _easer.data.eases[index].curve;
            //GUI.color = Color.red;
            //EditorGUIUtility.DrawCurveSwatch(rect, curve, null, Color.red, new Color(0, 0, 0, 0), new Rect(0, 0, 1, 1));
            //GUI.color = GUI.contentColor;

            float inc = 0.01f;
            for (float i = 0; i < 1; i += inc)
            {
                float val = curve.Evaluate(i);
                float next = curve.Evaluate(i + inc);

                Vector2 start = new Vector2(rect.x + (i * rect.width), (rect.y + rect.height) - (val * rect.height));
                Vector2 end = new Vector2(rect.x + ((i + inc) * rect.width),
                    (rect.y + rect.height) - (next * rect.height));
                if (EditorGUIUtility.isProSkin)
                {
                    Handles.color = Color.white;
                }
                else
                {
                    Handles.color = Color.white;
                }

                Handles.DrawLine(start, end);
            }

            // 动画
            if (rect.Contains(_mousePos + _scrollPos))
            {
                float deltaTime = Time.realtimeSinceStartup - _lastTime;
                float value = curve.Evaluate(_dotValue);
                _dotValue += deltaTime * 0.5f;
                if (_dotValue > 1) _dotValue = 0;
                _lastTime = Time.realtimeSinceStartup;

                float x = rect.x + (_dotValue * rect.width);
                float y = (rect.y + rect.height) - (value * rect.height);
                Vector2 pos = new Vector2(x - (DOT_SIZE * 0.5f), y - (DOT_SIZE * 0.5f));
                //Vector2 xPos = new Vector2(x - (DOT_SIZE * 0.5f), (rect.yMax + 10) - (DOT_SIZE * 0.5f));
                Vector2 yPos = new Vector2((rect.xMax + 10) - (DOT_SIZE * 0.5f), y - (DOT_SIZE * 0.5f));

                //Handles.color = offLineColor;
                //Handles.DrawLine(new Vector2(x, y), new Vector3(x, xPos.y));
                //Handles.DrawLine(new Vector2(x, y), new Vector3(yPos.x, y));

                Handles.color = lineColor;
                Handles.DrawLine(new Vector2(rect.xMax + 10, rect.y), new Vector2(rect.xMax + 10, rect.yMax));

                Rect dotRect = new Rect(pos.x, pos.y, DOT_SIZE, DOT_SIZE);
                //Rect dotRectX = new Rect(xPos.x, xPos.y, DOT_SIZE, DOT_SIZE);
                Rect dotRectY = new Rect(yPos.x, yPos.y, DOT_SIZE, DOT_SIZE);

                if (EditorGUIUtility.isProSkin)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = new Color(0.6f, 0, 0, 1);
                }

                GUI.DrawTexture(EaserEditorUtils.OutsetRect(dotRect, 1), _texture);
                //GUI.DrawTexture(EaserEditorUtils.OutsetRect(dotRectX, 1), _texture);
                GUI.DrawTexture(EaserEditorUtils.OutsetRect(dotRectY, 1), _texture);

                if (EditorGUIUtility.isProSkin)
                {
                    GUI.color = new Color(0.6f, 0, 0, 1);
                }
                else
                {
                    GUI.color = Color.red;
                }

                GUI.DrawTexture(dotRect, _texture);
                //GUI.DrawTexture(dotRectX, _texture);
                GUI.DrawTexture(dotRectY, _texture);
            }

            _repaint = true;

            /*
            EaseUtility.EaseType ease = EaseUtility.EaseType.easeInCirc;
            for (float i = 0; i < 1; i += inc)
            {
                float val = EaseUtility.Ease(ease, 0, 1, i);
                float next = EaseUtility.Ease(ease, 0, 1, i + inc);
    
                Vector2 start = new Vector2(rect.x + (i * rect.width), (rect.y + rect.height) - (val * rect.height));
                Vector2 end = new Vector2(rect.x + ((i + inc) * rect.width), (rect.y + rect.height) - (next * rect.height));
                Handles.color = Color.green;
                Handles.DrawLine(start, end);
            }
            */
        }

        public static void Load()
        {
            Debug.Log(Easer.DATA_FILENAME.Split('.')[0]);
            _easer = Resources.Load<EaserDataWrapper>(Easer.DATA_FILENAME.Split('.')[0]);
            if (_easer == null)
            {
                Debug.LogWarning("Easer data does not exist, creating new.");
                New();
            }

            if (Selection.activeObject == null)
            {
                Selection.activeObject = _easer;
            }
        }

        public static void New()
        {
            _easer = ScriptableObject.CreateInstance<EaserDataWrapper>();
            Save();
        }

        public static void Save()
        {
            if (_easer == null)
            {
                Debug.LogWarning("Easer data has not been loaded, not saving.");
                return;
            }

            // Check/Create directories
            string[] directories = Easer.DATA_PATH.Split('/');
            string currentPath = "";
            for (int i = 0; i < directories.Length; i++)
            {
                string dir = directories[i];
                Debug.Log(dir);
                string checkDir = Application.dataPath + "/" + currentPath + dir;
                string parentDir = "Assets" + ((i > 0) ? '/' + currentPath.Remove(currentPath.Length - 1) : "");
                if (!System.IO.Directory.Exists(checkDir))
                {
                    AssetDatabase.CreateFolder(parentDir, dir);
                }

                currentPath += dir + '/';
            }

            currentPath += Easer.DATA_FILENAME;

            Debug.Log(currentPath);
            if (!System.IO.File.Exists(Application.dataPath + "/" + currentPath))
            {
                AssetDatabase.CreateAsset(_easer, "Assets/" + currentPath);
            }

            EditorUtility.SetDirty(_easer);
            AssetDatabase.Refresh();
        }
    }

    internal class EaserEditorUtils
    {
        public static Rect OutsetRect(Rect rect, int outset)
        {
            Rect output = rect;
            output.xMin -= outset;
            output.xMax += outset;
            output.yMin -= outset;
            output.yMax += outset;
            return output;
        }

        public static Rect InsetRect(Rect rect, int inset)
        {
            return OutsetRect(rect, -inset);
        }

        private static void DrawColoredBox(Color color, Rect rect)
        {
            DrawColoredBox(color, rect, 1);
        }

        private static void DrawColoredBox(Color color, Rect rect, int strength)
        {
            if (strength < 1)
            {
                strength = 1;
            }

            Color oldcolor = GUI.color;
            GUI.color = color;
            for (int i = 0; i < strength; i++)
            {
                GUI.Box(rect, "");
            }

            GUI.color = oldcolor;
        }

        public static void DrawHighlightBox(Rect rect)
        {
            DrawHighlightBox(rect, 1);
        }

        public static void DrawHighlightBox(Rect rect, int strength)
        {
            DrawColoredBox(Color.white, rect, strength);
        }

        public static void DrawShadowBox(Rect rect)
        {
            DrawShadowBox(rect, 1);
        }

        public static void DrawShadowBox(Rect rect, int strength)
        {
            DrawColoredBox(Color.black, rect, strength);
        }

        public static void DrawOutsetBox(Rect rect)
        {
            DrawOutsetBox(rect, 1, 2);
        }

        public static void DrawOutsetBox(Rect rect, int highlight, int shadow)
        {
            if (EditorGUIUtility.isProSkin)
            {
                DrawShadowBox(OutsetRect(rect, 1), shadow);
                DrawHighlightBox(rect, highlight);
            }
            else
            {
                DrawHighlightBox(rect, highlight);
            }
        }

        public static void DrawInsetBox(Rect rect)
        {
            DrawInsetBox(rect, 1, 1);
        }

        public static void DrawInsetBox(Rect rect, int shadow, int highlight)
        {
            if (EditorGUIUtility.isProSkin)
            {
                DrawHighlightBox(rect, highlight);
                DrawShadowBox(InsetRect(rect, 1), 3 + shadow);
            }
            else
            {
                DrawHighlightBox(rect, highlight);
            }
        }

        public static void DrawOutlineBox(Rect rect, Color color)
        {
            Color oldColor = Handles.color;
            Handles.color = color;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.xMax, rect.y));
            Handles.DrawLine(new Vector2(rect.xMax, rect.y), new Vector2(rect.xMax, rect.yMax));
            Handles.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax));
            Handles.DrawLine(new Vector2(rect.x, rect.yMax), new Vector2(rect.x, rect.y));
            Handles.color = oldColor;
        }

        public static void DrawGrid(Rect rect, Color color)
        {
            DrawGrid(rect, color, new Color(color.r, color.g, color.b, color.a * 0.25f));
        }

        public static void DrawGrid(Rect rect, Color mainColor, Color secondaryColor)
        {
            Color oldColor = Handles.color;
            Handles.color = secondaryColor;

            int divs = 16;
            for (int i = 0; i < divs; i++)
            {
                Vector2 xStart = new Vector2(rect.x + (rect.width / divs * i), rect.y);
                Vector2 xEnd = new Vector2(rect.x + (rect.width / divs * i), rect.yMax);
                Handles.DrawLine(xStart, xEnd);

                Vector2 yStart = new Vector2(rect.x, rect.y + (rect.height / divs * i));
                Vector2 yEnd = new Vector2(rect.xMax, rect.y + (rect.height / divs * i));
                Handles.DrawLine(yStart, yEnd);
            }

            Handles.color = mainColor;
            Handles.DrawLine(new Vector2(rect.x + (rect.width * 0.5f), rect.y),
                new Vector2(rect.x + (rect.width * 0.5f), rect.yMax));
            Handles.DrawLine(new Vector2(rect.x, rect.y + (rect.height * 0.5f)),
                new Vector2(rect.xMax, rect.y + (rect.height * 0.5f)));
            DrawOutlineBox(rect, mainColor);

            Handles.color = oldColor;
        }

        public static Rect TitleText(Rect container, string text)
        {
            Color oldColor = GUI.color;

            Rect rect = new Rect(container.x + 5, container.y + 5, container.width - 10, 20);

            EaserEditorUtils.DrawOutsetBox(rect, 2, 3);

            Rect textRect = rect;
            textRect.xMin += 5;
            textRect.yMin += 3;
            GUI.color = new Color(0, 0, 0, 0.35f);
            if (EditorGUIUtility.isProSkin)
            {
                GUI.Label(textRect, text);
            }

            textRect.xMin -= 1;
            textRect.yMin -= 1;
            GUI.color = Color.white;
            GUI.Label(textRect, text);

            GUI.color = oldColor;

            return rect;
        }
    }
}