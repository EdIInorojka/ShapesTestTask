namespace ShapesTestTask
{
    internal interface IShapeService
    {
        public IEnumerable<Shape> GetShapes();
        public Shape AddShape(double canvasWidth, double canvasHeight); //Добавление шейпа

        void DeleteShape(string shapeName); //Удаление шейпа
        void DecreaseNumberOfShapes(); //Уменьшение счётчика шейпов
    }
}
