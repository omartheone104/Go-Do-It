namespace GoDoIt;

interface ISaver<T>
{
    public void Save(T obj, Stream stream);

    public T Load(Stream stream);

}