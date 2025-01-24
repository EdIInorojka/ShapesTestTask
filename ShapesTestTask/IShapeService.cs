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
