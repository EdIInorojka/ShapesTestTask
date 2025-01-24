namespace ShapesTestTask
{
    internal class Shape
    {
        public string Name { get; set; } //Название
        public double X {  get; set; } 
        public double Y { get; set; }
        public string Color { get; set; }
        public Shape PreviousShape { get; set; } //Предыдущий шейп
        public Shape NextShape { get; set; } //Слудующий шейп

        public override string ToString() 
        {
            return Name;
        }

    }
}
