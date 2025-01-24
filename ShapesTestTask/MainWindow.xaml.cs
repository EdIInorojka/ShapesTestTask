using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShapesTestTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IShapeService _shapeService;
        private Shape _draggedShape; // Для перетаскивания шейпа
        private Point _dragStartPoint; // Начальная точка для перетаскивания
        public MainWindow() : this(new ShapeService())
        {
        }

        private MainWindow(IShapeService shapeService)
        {
            InitializeComponent();
            _shapeService = shapeService;
            LoadShapes();
        }

        private void LoadShapes()
        {
            var shapes = _shapeService.GetShapes();

            foreach (var shape in shapes)
            {
                AddShapeToCanvas(shape);
                ShapeComboBox.Items.Add(shape);
            }
        }

        private void AddShapeButton_Click(object sender, RoutedEventArgs e)
        {
            var newShape = _shapeService.AddShape(ShapesCanvas.ActualWidth, ShapesCanvas.ActualHeight);

            if (newShape == null)
            {
                AddShapeButton.IsEnabled = false;

                MessageBox.Show("Нет свободного места для добавления новых объёктов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            AddShapeToCanvas(newShape);
            ShapeComboBox.Items.Add(newShape);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShapeComboBox.SelectedItem is Shape selectedShape)
            {
                
                ShapeComboBox.Items.Remove(ShapeComboBox.SelectedItem);

                RemoveExistingLine(selectedShape.PreviousShape, selectedShape);
                RemoveExistingLine(selectedShape, selectedShape.NextShape);
                _shapeService.DeleteShape(selectedShape.Name);
                if (selectedShape.NextShape != null)
                {
                    CreateArrowLineWithArrow(selectedShape.NextShape);
                }

                foreach (Border shape in ShapesCanvas.Children.OfType<Border>())
                {
                    if(shape.Tag == selectedShape)
                    {
                        ShapesCanvas.Children.Remove(shape);
                        break;
                    }
                }
                // Разблокируем кнопку добавления
                AddShapeButton.IsEnabled = true;
            }
        }

        private void ShapeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShapeComboBox.SelectedItem is Shape selectedShape)
            {
                DeleteButton.IsEnabled = true;
                HighlightShape(selectedShape);
            }
            else
            {
                DeleteButton.IsEnabled = false;
            }
        }

        private void AddShapeToCanvas(Shape shape)
        {
            // Создаем текст с названием фигуры
            var textBlock = new TextBlock
            {
                Text = shape.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Black
            };

            // Создаем прямоугольник с текстом внутри
            var border = new Border
            {
                Width = 100,
                Height = 50,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(shape.Color)),
                Background = Brushes.LightGray,
                Child = textBlock,
                Tag = shape // Сохраняем Shape внутри
            };

            Canvas.SetLeft(border, shape.X);
            Canvas.SetTop(border, shape.Y);

            // Добавляем обработчики для клика и перетаскивания
            border.MouseLeftButtonDown += Shape_MouseLeftButtonDown;
            border.MouseMove += Shape_MouseMove;
            border.MouseLeftButtonUp += Shape_MouseLeftButtonUp;

            // Добавляем прямоугольник на Canvas
            ShapesCanvas.Children.Add(border);

            if (shape.PreviousShape != null)
            {
                CreateArrowLineWithArrow(shape);
            }

            CheckFreePositions();
        }

        private void CreateArrowLineWithArrow(Shape shape)
        {
            Line line = CreateArrowLine(shape); // Основная линия, которая уже создаётся вашим методом

            if(line != null)
            {
                // Создание геометрии для стрелочки
                double arrowSize = 10; // Длина стрелки
                double arrowWidthFactor = 0.5; // Коэффициент ширины (уменьшаем угол)

                Vector direction = new Vector(line.X2 - line.X1, line.Y2 - line.Y1);
                direction.Normalize();

                // Точки для треугольника (стрелочки)
                Point endPoint = new Point(line.X2, line.Y2);
                Point arrowPoint1 = endPoint - arrowSize * direction + (arrowSize * arrowWidthFactor) * new Vector(-direction.Y, direction.X);
                Point arrowPoint2 = endPoint - arrowSize * direction - (arrowSize * arrowWidthFactor) * new Vector(-direction.Y, direction.X);

                // Создаём стрелочку (Path)
                Path arrowHead = new Path
                {
                    Stroke = Brushes.Black,
                    Fill = Brushes.Black,
                    Tag = shape,
                    Data = new PathGeometry(new[]
                    {
                    new PathFigure(endPoint, new[]
                    {
                        new LineSegment(arrowPoint1, true),
                        new LineSegment(arrowPoint2, true)
                    }, true)
                    
                })
                };

                // Добавляем линию и стрелочку в Canvas (или другой контейнер)
                var canvas = Application.Current.MainWindow.FindName("ShapesCanvas") as Canvas;
                canvas.Children.Add(line); // Добавляем саму линию
                canvas.Children.Add(arrowHead); // Добавляем стрелочку
            }
        }

        private Line CreateArrowLine(Shape shape)
        {
            if (shape.PreviousShape != null)
            {
                // Координаты середины сторон для PreviousShape
                double previousTopCenterX = shape.PreviousShape.X + 50; // Середина верхней стороны
                double previousTopCenterY = shape.PreviousShape.Y;

                double previousBottomCenterX = shape.PreviousShape.X + 50; // Середина нижней стороны
                double previousBottomCenterY = shape.PreviousShape.Y + 50;

                double previousRightCenterX = shape.PreviousShape.X + 100; // Середина правой стороны
                double previousRightCenterY = shape.PreviousShape.Y + 25;

                double previousLeftCenterX = shape.PreviousShape.X; // Середина левой стороны
                double previousLeftCenterY = shape.PreviousShape.Y + 25;


                // Координаты середины сторон для текущего элемента
                double currentTopCenterX = shape.X + 50;
                double currentTopCenterY = shape.Y;

                double currentBottomCenterX = shape.X + 50;
                double currentBottomCenterY = shape.Y + 50;

                double currentRightCenterX = shape.X + 100;
                double currentRightCenterY = shape.Y + 25;

                double currentLeftCenterX = shape.X;
                double currentLeftCenterY = shape.Y + 25;

                // Генерация всех четырёх линий
                var lines = new List<(Line line, double length)>
                {
                    (new Line
                    {
                        X1 = previousTopCenterX,
                        Y1 = previousTopCenterY,
                        X2 = currentBottomCenterX,
                        Y2 = currentBottomCenterY,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Tag = shape
                    }, GetDistance(previousTopCenterX, previousTopCenterY, currentBottomCenterX, currentBottomCenterY)),

                    (new Line
                    {
                        X1 = previousBottomCenterX,
                        Y1 = previousBottomCenterY,
                        X2 = currentTopCenterX,
                        Y2 = currentTopCenterY,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Tag = shape
                    }, GetDistance(previousBottomCenterX, previousBottomCenterY, currentTopCenterX, currentTopCenterY)),

                    (new Line
                    {
                        X1 = previousRightCenterX,
                        Y1 = previousRightCenterY,
                        X2 = currentLeftCenterX,
                        Y2 = currentLeftCenterY,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Tag = shape
                    }, GetDistance(previousRightCenterX, previousRightCenterY, currentLeftCenterX, currentLeftCenterY)),

                    (new Line
                    {
                        X1 = previousLeftCenterX,
                        Y1 = previousLeftCenterY,
                        X2 = currentRightCenterX,
                        Y2 = currentRightCenterY,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Tag = shape
                    }, GetDistance(previousLeftCenterX, previousLeftCenterY, currentRightCenterX, currentRightCenterY))

                };

                // Выбираем линию с минимальной длиной
                var shortestLine = lines.OrderBy(line => line.length).First().line;
                return shortestLine;
            }
            else
            {
                return null;
            }
        }

        // Метод для вычисления расстояния между двумя точками
        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        private void UpdateArrowLine(Shape shape)
        {
            // Удаляем старую линию к предыдущему Shape, если она есть
            if (shape.PreviousShape != null)
            {
                RemoveExistingLine(shape.PreviousShape, shape);
                CreateArrowLineWithArrow(shape);
            }

            // Удаляем старую линию к следующему Shape, если она есть
            if (shape.NextShape != null)
            {
                RemoveExistingLine(shape, shape.NextShape);
                CreateArrowLineWithArrow(shape.NextShape);
            }
        }

        private void RemoveExistingLine(Shape fromShape, Shape toShape)
        {
            // Найти существующую линию в Canvas
            var existingLine = ShapesCanvas.Children.OfType<Line>()
                .FirstOrDefault(line =>
                    line.Tag is Shape taggedShape &&
                    (taggedShape == toShape ));

            var existingArrowHead = ShapesCanvas.Children.OfType<Path>()
                .FirstOrDefault(line =>
                    line.Tag is Shape taggedShape &&
                    (taggedShape == toShape));

            if (existingLine != null)
            {
                ShapesCanvas.Children.Remove(existingLine);
            }
            if(existingArrowHead != null)
            {
                ShapesCanvas.Children.Remove(existingArrowHead);
            }
        }

        private void HighlightShape(Shape selectedShape)
        {
            // Сброс выделения всех шейпов
            foreach (var child in ShapesCanvas.Children)
            {
                if (child is Border border)
                {
                    if(Tag == selectedShape)
                    {
                        border.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        border.BorderThickness = new Thickness(0);
                    }
                }
            }

            // Выделяем текущий шейп
            var shapeToHighlight = ShapesCanvas.Children.OfType<Border>()
                .FirstOrDefault(b => b.Tag == selectedShape);

            if (shapeToHighlight != null)
            {
                shapeToHighlight.BorderThickness = new Thickness(4);
            }
        }

        private void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Shape shape)
            {
                _draggedShape = shape;
                _dragStartPoint = e.GetPosition(ShapesCanvas);
                HighlightShape(shape);
                ShapeComboBox.SelectedItem = shape;
            }
        }

        private void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(ShapesCanvas);
                var offsetX = currentPoint.X - _dragStartPoint.X;
                var offsetY = currentPoint.Y - _dragStartPoint.Y;

                _draggedShape.X += offsetX;
                _draggedShape.Y += offsetY;

                _dragStartPoint = currentPoint;

                // Обновляем позицию шейпа
                var border = ShapesCanvas.Children.OfType<Border>()
                    .FirstOrDefault(b => b.Tag == _draggedShape);

                if (border != null)
                {
                    Canvas.SetLeft(border, _draggedShape.X);
                    Canvas.SetTop(border, _draggedShape.Y);
                }

                UpdateArrowLine(_draggedShape);

                CheckFreePositions();
            }
        }

        private void Shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggedShape = null; // Завершаем перетаскивание
        }

        private void CheckFreePositions()
        {
            var newShape = _shapeService.AddShape(ShapesCanvas.ActualWidth, ShapesCanvas.ActualHeight);
            if (newShape != null)
            {
                AddShapeButton.IsEnabled = true;
                _shapeService.DeleteShape(newShape.Name);
                _shapeService.DecreaseNumberOfShapes();
            }
        }
    }
}