namespace IoCloud.Shared.BusinessRules
{
    public interface IBusinessRule<TValue> : IBusinessRule
    {
        TValue Apply();
    }

    public interface IBusinessRule
    {
    }
}
