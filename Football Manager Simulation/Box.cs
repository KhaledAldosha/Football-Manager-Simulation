using System;

namespace GUI
{
    // Represents a rectangular area on the screen.
    // Used for UI elements such as windows or info boxes.
    public class Box
    {
        // Left X coordinate of the box
        public int Left { get; set; }
        // Top Y coordinate of the box
        public int Top { get; set; }
        // Width of the box
        public int Width { get; set; }
        // Height of the box
        public int Height { get; set; }
        // Title to display on the box (if any)
        public string Title { get; set; }
        // Constructor that sets the box's location, size, and title.
        public Box(int left, int top, int width, int height, string title)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Title = title;
        }
    }
}
