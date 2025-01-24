using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapesTestTask
{
    internal interface IShapeService
    {
        public IEnumerable<Shape> GetShapes();
        public Shape AddShape(double canvasWidth, double canvasHeight);

        void DeleteShape(string shapeName);
        void DecreaseNumberOfShapes();
    }
}
