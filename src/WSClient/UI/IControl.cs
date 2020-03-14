namespace WSClient.UI
{
    interface IControl
    {
        Rectangle RequiredSize { get; }
        Rectangle RenderSize { get; }

        void EvaluateSize(Rectangle size);
        void Render(Point possition, Rectangle size);
    }
}
