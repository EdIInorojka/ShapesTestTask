namespace ShapesTestTask
{
    internal class ShapeService : IShapeService
    {
        private Random random = new Random();
        private readonly List<Shape> shapes = new();
        private int shapesCounter = 1;
        private const int ShapeWidth = 100;
        private const int ShapeHeight = 50;

        //Возврат списка шейпов
        public IEnumerable<Shape> GetShapes()
        {
            return shapes;
        }

        //Добавление шейпа
        public Shape AddShape(double canvasWidth, double canvasHeight)
        {
            var name = $"Shape {shapesCounter}";

            var position = FindRandomFreePosition(canvasWidth, canvasHeight);

            if (position == null) return null;

            var newShape = new Shape
            {
                Name = name,
                X = position.Value.x,
                Y = position.Value.y,
                Color = GetRandomColor(),
                PreviousShape = shapes.LastOrDefault()
            };
            if(newShape.PreviousShape != null )
            {
                newShape.PreviousShape.NextShape = newShape;
            }

            shapes.Add(newShape);
            shapesCounter++;
            return newShape;
        }


        //Удаление шейпа и переназначение предыдущего и слудующего шейпов
        public void DeleteShape(string shapeName)
        {
            var shape = shapes.FirstOrDefault(s => s.Name == shapeName);
            if(shape.PreviousShape != null)
            {
                if(shape.NextShape != null)
                {
                    shape.NextShape.PreviousShape = shape.PreviousShape;
                    shape.PreviousShape.NextShape = shape.NextShape;
                }
                else
                {
                    shape.PreviousShape.NextShape = null;

                }
            }
            else
            {
                shape.NextShape.PreviousShape = null;
            }

            if (shape != null)
            {
                shapes.Remove(shape);
            }
        }


        //Уменьшение счётчика шейпов
        public void DecreaseNumberOfShapes()
        {
            shapesCounter--;
        }


        //Нахождение случайной свободной позиции
        private (double x, double y)? FindRandomFreePosition(double canvasWidth, double canvasHeight)
        {
            double x, y;
            var maxRetries = 10000;
            int attempts = 0;

            do
            {
                x = random.Next(0, (int)canvasWidth);
                y = random.Next(0, (int)canvasHeight);

                attempts++;
            } while (!IsPositionAvailable(x, y, canvasWidth, canvasHeight) && attempts < maxRetries);

            return attempts < maxRetries ? (x, y) : null; 
        }


        //Проверка на доступность позиции
        private bool IsPositionAvailable(double x, double y, double canvasWidth, double canvasHeight)
        {
            if (x > canvasWidth - 150 || y > canvasHeight - 150)
            {
                return false;
            }
            foreach (var shape in shapes)
            {
                double deltaX = Math.Abs(shape.X - x);
                double deltaY = Math.Abs(shape.Y - y);
                if (deltaX < ShapeWidth && deltaY < ShapeHeight )
                {
                    return false; 
                }
            }
            return true;
        }

        //Получение рандомного цвета для обводки шейпа
        private string GetRandomColor()
        {
            var random = new Random();
            return $"#{random.Next(0x1000000):X6}";
        }
    }
}
