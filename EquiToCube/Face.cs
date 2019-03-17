using System;

namespace Vangaorth.EquiToCube
{
    public enum Face
    {
        Back = 0, 
        Left = 1, 
        Front = 2, 
        Right = 3, 
        Top = 4,
        Bottom = 5
    }

    public static class FaceExtensions
    {
        public static string ToFriendlyString(this Face face)
        {
            switch (face)
            {
                case Face.Back: return "Back";
                case Face.Left: return "Left";
                case Face.Front: return "Front";
                case Face.Right: return "Right";
                case Face.Top: return "Top";
                case Face.Bottom: return "Bottom";
            }

            throw new Exception($"Unexpected {nameof(face)} value ({face})");
        }
    }
}
