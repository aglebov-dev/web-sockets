using System;
using System.Collections.Generic;
using System.Linq;

namespace WSClient.UI
{
    public class TableRow : IControl, ICollectionControl
    {
        private object _data;
        private readonly IEnumerable<TableHeader> _headers;
        private readonly List<Cell> _cells;

        public Rectangle RequiredSize { get; private set; }
        public Rectangle RenderSize { get; private set; }

        public TableRow(IEnumerable<TableHeader> headers)
        {
            _headers = headers ?? Enumerable.Empty<TableHeader>();
            _cells = new List<Cell>();
        }

        public void BindData(object data)
        {
            _data = data;
        }

        public void PopulateData()
        {
            _cells.Clear();
            foreach (var header in _headers)
            {
                var value = header.GetBindingValue(_data);
                var cell = new Cell(header, value);
                _cells.Add(cell);
            }
        }

        public void EvaluateSize(Rectangle size)
        {
            var avSize = size;
            foreach (var cell in _cells)
            {
                cell.EvaluateSize(size);
                avSize = new Rectangle(avSize.Width - cell.RequiredSize.Width, avSize.Height);
            }
            
            var query = _cells.Select(x => x.RequiredSize);
            var width = query.Select(x => x.Width).Sum();
            var height = query.Select(x => x.Height).Sum();

            RequiredSize = new Rectangle(width, height);
        }

        public void Render(Point possition, Rectangle size)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            var offsetX = possition.X;
            var offestY = possition.Y;
            var maxCellHeight = _cells.Select(x => x.RequiredSize.Height).Max();
            var height = Math.Min(size.Height, maxCellHeight);

            foreach (var cell in _cells)
            {
                var point = new Point(offsetX, offestY);
                var rectangle = new Rectangle(0, height);

                cell.Render(point, rectangle);

                offsetX += cell.RenderSize.Width;
            }

            RenderSize = new Rectangle(offsetX, maxCellHeight);
        }
    }
}
