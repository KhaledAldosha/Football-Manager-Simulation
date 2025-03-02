using System;

namespace GUI
{
    public class Box
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Title { get; set; }

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
