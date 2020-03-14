using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WSClient.UI
{
    public class Table : IControl, ICollectionControl
    {
        private readonly List<TableRow> _rows;
        
        public ICollection DataSource { get; set; }
        public ICollection<TableHeader> Headers { get; }
        public Rectangle RequiredSize { get; private set; }
        public Rectangle RenderSize { get; private set; }

        public Table()
        {
            _rows = new List<TableRow>();
            Headers = new List<TableHeader>();
        }

        public void PopulateData()
        {
            if (DataSource?.Count > 0)
            {
                foreach (var (data, index) in DataSource.OfType<object>().Select((x, i) => (x, i)))
                {
                    var row = default(TableRow);

                    if (_rows.Count > index)
                    {
                        row = _rows[index];
                    }
                    else
                    {
                        row = new TableRow(Headers);
                        _rows.Add(row);
                    }

                    row.BindData(data);
                    row.PopulateData();
                }
            }
            else
            {
                _rows.Clear();
            }
        }
        public void EvaluateSize(Rectangle size)
        {
            var headersWidth = 0; 
            var headersHeight = 0;
            var avSize = size;
            foreach (var header in Headers)
            {
                header.EvaluateSize(avSize);
                headersWidth += header.RequiredSize.Width;
                if (headersHeight < header.RequiredSize.Height)
                {
                    headersHeight = header.RequiredSize.Height;
                }

                avSize = new Rectangle(avSize.Width - header.Width, avSize.Height);
            }

            avSize = new Rectangle(size.Width, size.Height - headersHeight);
            var rowsHeight = 0;
            foreach (var row in _rows)
            {
                row.EvaluateSize(avSize);
                rowsHeight += row.RequiredSize.Height;

                avSize = new Rectangle(size.Width, avSize.Height - row.RequiredSize.Height);
            }

            RequiredSize = new Rectangle(headersWidth, headersHeight + rowsHeight);
        }
        public void Render(Point possition, Rectangle size)
        {
            var hsize = RenderHeaders(possition, size);

            var rowPoint = new Point(possition.X, possition.Y + hsize.Height);
            var rowSize = new Rectangle(hsize.Width, size.Height - hsize.Height);
            var rsize = RenderRows(rowPoint, rowSize);

            RenderSize = new Rectangle(hsize.Width, hsize.Height + rsize.Height);
        }


        public Rectangle RenderHeaders(Point possition, Rectangle size)
        {
            var offsetX = 0;
            var availableWidth = size.Width;
            var availableHeight = size.Height;

            var totalHeaderHeight = Math.Min(size.Height, Headers.Select(x => x.RequiredSize.Height).Max());

            var w = 0;
            var h = 0;
            foreach (var header in Headers)
            {
                var headerSize = header.RequiredSize;
                var width = (availableWidth < headerSize.Width) ? availableWidth : headerSize.Width;
                var height = totalHeaderHeight;
                var point = new Point(possition.X + offsetX, possition.Y);
                var rectangle = new Rectangle(width, height);

                header.Render(point, rectangle);

                offsetX += width;
                availableWidth -= width;

                w += header.RenderSize.Width;
                h = h > header.RenderSize.Height ? h : header.RenderSize.Height;
            }

            return new Rectangle(w, h);
        }

        public Rectangle RenderRows(Point possition, Rectangle size)
        {
            var offsetY = 0;
            var availableHeigth = size.Height;
            foreach (var row in _rows)
            {
                var width = size.Width;
                var height = Math.Min(row.RequiredSize.Height, availableHeigth);
                var rectangle = new Rectangle(width, height);
                var point = new Point(possition.X, possition.Y + offsetY);

                row.Render(point, rectangle);

                offsetY += row.RenderSize.Height;
                availableHeigth -= row.RenderSize.Height;
            }

            return new Rectangle(size.Width, offsetY);
        }
    }
}
