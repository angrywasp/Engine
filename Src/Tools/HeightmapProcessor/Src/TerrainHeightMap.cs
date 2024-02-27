namespace HeightmapProcessor
{
	public class TerrainHeightMap
	{
		private float[,] values;
        private int width;
        private int height;

		public float this[int x, int z] => values[x, z];

		public float[,] Values => values;

        public int Width => width;

        public int Height => height;

		public TerrainHeightMap(int width, int height, float[,] values, float verticalScale)
		{
            this.values = values;

			this.width = width;
			this.height = height;

			for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    values[x, z] *= verticalScale;
		}
	}
}