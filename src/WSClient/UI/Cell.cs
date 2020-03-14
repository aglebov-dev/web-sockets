using System;

namespace WSClient.UI
{
    public class Cell : IControl
    {
        private readonly TableHeader _headerSize;
        private readonly string _value;

        public Rectangle RequiredSize { get; private set; }
        public Rectangle RenderSize { get; private set; }

        public Cell(TableHeader headerSize, string value)
        {
            _headerSize = headerSize;
            _value = value;
        }

        public void EvaluateSize(Rectangle size)
        {
            var width = _headerSize.RequiredSize.Width;
            var height = _value.Length / (width - 1) + Math.Min(1, (_value.Length % (width - 1)));

            RequiredSize = new Rectangle(width + 1, height + 1);
        }

        public void Render(Point possition, Rectangle size)
        {
            var rectangle = new Rectangle(_headerSize.RenderSize.Width, size.Height);
            if (rectangle.Width <= 0 || rectangle.Height <= 0)
            {
                return;
            }

            RenderText(possition, rectangle);
            RenderBorder(possition, rectangle);

            RenderSize = rectangle;
        }

        private void RenderText(Point possition, Rectangle size)
        {
            var textWidth = size.Width - 1;
            var y = possition.Y;
            var stringOffset = 0;

            if (textWidth > 0)
            {
                while (stringOffset < _value.Length)
                {
                    var length = Math.Min(textWidth, _value.Length - stringOffset);
                    var line = _value.Substring(stringOffset, length);

                    Console.CursorLeft = possition.X;
                    Console.CursorTop = y;
                    Console.Write(line);

                    y++;
                    stringOffset += length;
                }
            }
        }

        private void RenderBorder(Point possition, Rectangle size)
        {
            var textWidth = size.Width - 1;
            var y = possition.Y;
            var h = y + size.Height - 1;
            while (y < h)
            {
                Console.CursorLeft = possition.X + textWidth;
                Console.CursorTop = y;
                Console.Write('|');
                y++;
            }

            Console.CursorLeft = possition.X;
            Console.CursorTop = y;
            Console.Write(new string('-', textWidth));
            Console.Write('+');
        }
    }
}
