namespace WSClient.UI
{
    public struct Rectangle
    {
        public int Width { get; }
        public int Height { get; }

        public Rectangle(int width, int height)
        {
            Width = width > 0 ? width : 0;
            Height = height > 0 ? height : 0;
        }
    }
}
