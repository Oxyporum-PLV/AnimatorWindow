using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class PlayModeStateChanged
{
    public static PlayModeStateChange State;
        static PlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        public static void LogPlayModeState(PlayModeStateChange state)
        {
            State = state;
        }
}
