namespace Vangaorth.EquiToCube
{
    public struct Triple<T>
    {
        public T X;
        public T Y;
        public T Z;

        public Triple(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
