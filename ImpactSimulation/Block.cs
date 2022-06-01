namespace ImpactSimulation
{
    class Block
    {
        public (decimal X, int Y) Position;

        public static decimal Resiliency { get; set; }

        public decimal Mass { get; set; }
        public int Size { get; set; }
        public decimal Speed { get; set; }

        public enum Scale
        {
            Small,
            Medium,
            Large
        };

        public Block(decimal mass, (decimal X, int Y) position, int size, decimal speed)
        {
            Mass = mass;
            Position = position;
            Size = size;
            Speed = speed;
        }

        public static void Impact(Block bl1, Block bl2)
        {
            decimal tempSpeed1 = bl1.Speed;
            decimal tempSpeed2 = bl2.Speed;
            bl1.Speed = ((bl1.Mass - Resiliency * bl2.Mass) * tempSpeed1 + (1 + Resiliency) * bl2.Mass * tempSpeed2) / (bl1.Mass + bl2.Mass);
            bl2.Speed = ((bl2.Mass - Resiliency * bl1.Mass) * tempSpeed2 + (1 + Resiliency) * bl1.Mass * tempSpeed1) / (bl1.Mass + bl2.Mass);
            bl1.Position.X = bl2.Position.X - bl1.Size;
        }

        public static void Impact(Block bl)
        {
            bl.Speed = -bl.Speed * Resiliency;
            bl.Position.X = 400;
        }
    }
}
