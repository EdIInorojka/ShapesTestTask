namespace ShapesTestTask
{
    internal class Shape
    {
        public string Name { get; set; }
        public double X {  get; set; }
        public double Y { get; set; }
        public string Color { get; set; }
        public Shape PreviousShape { get; set; }
        public Shape NextShape { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
