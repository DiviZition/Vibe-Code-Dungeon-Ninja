namespace Core
{
    public interface IView
    {
    }

    public interface IView<TModel> : IView where TModel : IModel
    {
        void Init(TModel model);
    }
}