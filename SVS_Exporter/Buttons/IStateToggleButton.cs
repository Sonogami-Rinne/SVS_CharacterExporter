using UnityEngine;

namespace SVSExporter
{
    public interface IStateToggleButton
    {
        GUIContent Content { get; }
        void OnClick();
    }
}