namespace ProtocolEMR.Systems
{
    /// <summary>
    /// Interface for objects that can be highlighted/unhighlighted.
    /// Used to provide visual feedback when player aims at interactable objects.
    /// </summary>
    public interface IHighlightable
    {
        void OnHighlight();
        void OnUnhighlight();
    }
}
