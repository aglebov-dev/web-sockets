using System;

namespace WSClient.UI
{
    public enum Alignment
    {
        Left,
        Right,
        Center
    }

    public class TableHeader : IControl
    {
        private readonly Binding _binding;
        public int Width { get; }
        public string Title { get; }
        public Alignment Alignment { get; set; }
        public Rectangle RequiredSize { get; private set; }
        public Rectangle RenderSize { get; private set; }

        public TableHeader(int width, string title, Binding binding)
        {
            Width = width;
            Title = title;
            _binding = binding;
        }

        internal string GetBindingValue<T>(T data)
        {
            return _binding.GetValue(data);
        }

        public void EvaluateSize(Rectangle size) 
        {
            var width = Math.Min(Width, size.Width);
            var height = Title.Length / (width - 1) + Math.Min(1, (Title.Length % (width - 1)));

            RequiredSize = new Rectangle(width, height);
        }

        public void Render(Point possition, Rectangle size)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            var textWidth = size.Width - 1;
            var y = possition.Y;
            var stringOffset = 0;

            if (textWidth > 0)
            {
                while (stringOffset < Title.Length)
                {
                    var length = Math.Min(textWidth, Title.Length - stringOffset);
                    var line = Title.Substring(stringOffset, length);
                    var alignmentOffset = Alignment == Alignment.Right 
                        ? textWidth - line.Length 
                        : Alignment == Alignment.Center
                            ? (textWidth - line.Length) / 2
                            : 0;

                    Console.CursorLeft = possition.X + alignmentOffset;
                    Console.CursorTop = y;
                    Console.Write(line);

                    y++;
                    stringOffset += length;
                }
            }

            y = possition.Y;
            while (y < size.Height)
            {
                Console.CursorLeft = possition.X + textWidth;
                Console.CursorTop = y;
                Console.Write('|');
                y++;
            }

            Console.CursorLeft = possition.X;
            Console.CursorTop = possition.Y + size.Height;
            Console.Write(new string('-', textWidth));
            Console.Write('+');

            RenderSize = new Rectangle(size.Width, size.Height + 1);
        }
    }
}
