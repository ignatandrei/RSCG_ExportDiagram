namespace RSCG_ExportDiagram;

class Eq<T>: IEqualityComparer<T>
{
    private Func<T, T, bool> equals;

    public Eq(Func<T, T, bool> equals)
    {
        this.equals = equals;
    }

    public bool Equals(T x, T y)
    {
        return equals(x, y);
    }

    public int GetHashCode(T obj)
    {
        return base.GetHashCode();
    }
}

