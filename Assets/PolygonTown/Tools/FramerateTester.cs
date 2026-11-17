using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Hidden debug overlay to change the target framerate at runtime.
/// Used to check whether movement is framerate independent.
/// Spawns itself automatically when entering Play Mode – no need to add it to a scene.
/// Toggle with F8.
/// </summary>
public class FramerateTester : MonoBehaviour
{
    private static readonly int[] Presets = { 10, 15, 30, 60, 144, 240 };

    private bool _visible;
    private int _targetFps = -1;
    private float _smoothedDeltaTime;
    private float _sliderValue = 60f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Spawn()
    {
        if (FindAnyObjectByType<FramerateTester>() != null) return;

        var go = new GameObject(nameof(FramerateTester)) { hideFlags = HideFlags.HideInHierarchy };
        DontDestroyOnLoad(go);
        go.AddComponent<FramerateTester>();
    }

    private void Update()
    {
        _smoothedDeltaTime = Mathf.Lerp(_smoothedDeltaTime, Time.unscaledDeltaTime, 0.1f);

        if (TogglePressed()) _visible = !_visible;
    }

    private static bool TogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.F8);
#else
        return false;
#endif
    }

    private void SetFramerate(int fps)
    {
        _targetFps = fps;
        // targetFrameRate is ignored while VSync is active.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

    private void OnGUI()
    {
        if (!_visible) return;

        GUILayout.BeginArea(new Rect(10, 10, 260, 230), GUI.skin.box);
        GUILayout.Label("<b>Framerate Tester</b>  (F8 to hide)", new GUIStyle(GUI.skin.label) { richText = true });

        float fps = _smoothedDeltaTime > 0f ? 1f / _smoothedDeltaTime : 0f;
        GUILayout.Label($"Current: {fps:0} FPS   |   Target: {(_targetFps > 0 ? _targetFps.ToString() : "Unlimited")}");
        GUILayout.Label($"Time since start: {Time.time:0.00} s");

        GUILayout.BeginHorizontal();
        for (int i = 0; i < Presets.Length; i++)
        {
            if (i == 3)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
            if (GUILayout.Button(Presets[i].ToString())) SetFramerate(Presets[i]);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        _sliderValue = GUILayout.HorizontalSlider(_sliderValue, 5f, 300f);
        if (GUILayout.Button($"Set {Mathf.RoundToInt(_sliderValue)}", GUILayout.Width(70)))
            SetFramerate(Mathf.RoundToInt(_sliderValue));
        GUILayout.EndHorizontal();

        GUILayout.Space(4);
        if (GUILayout.Button("Unlimited")) SetFramerate(-1);

        GUILayout.EndArea();
    }
}
