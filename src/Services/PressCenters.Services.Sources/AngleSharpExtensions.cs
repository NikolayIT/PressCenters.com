namespace PressCenters.Services.Sources
{
    using AngleSharp.Dom;

    public static class AngleSharpExtensions
    {
        // Removes elementToRemove when it is anywhere inside element (but not element itself). It used to call
        // RemoveChild on every node of the subtree, throwing and swallowing an exception per node, and threw a
        // NullReferenceException when element was null.
        public static void RemoveRecursively(this INode element, INode elementToRemove)
        {
            if (element == null || elementToRemove == null || elementToRemove == element
                || !element.Contains(elementToRemove))
            {
                return;
            }

            elementToRemove.Parent?.RemoveChild(elementToRemove);
        }
    }
}
