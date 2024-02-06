using vesa.Core.Infrastructure;

namespace eShop.Ordering.Administration.Service.Slices.BuildAllStateViews;

public class BuildAllStateViewsCommand : Command
{
    public BuildAllStateViewsCommand() : base()
    {
    }

    public BuildAllStateViewsCommand(string triggeredBy, int lastSequenceNumber = 0) : base(triggeredBy, lastSequenceNumber)
    {
    }
}
