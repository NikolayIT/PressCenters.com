namespace PressCenters.Services.Sources
{
    using System;

    using AngleSharp.Dom;

    public static class AngleSharpExtensions
    {
        public static void RemoveRecursively(this INode element, INode elementToRemove)
        {
            if (elementToRemove == null)
            {
                return;
            }

            try
            {
                element.RemoveChild(elementToRemove);
            }
            catch (Exception)
            {
            }

            foreach (var node in element.ChildNodes)
            {
                node.RemoveRecursively(elementToRemove);
            }
        }
    }
}
