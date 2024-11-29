namespace FlowHub_MAUI.DataAccess.IServices;

public interface IFlowsService
{
    void GetFlows();
    IList<FlowsModelView>? AllFlows { get; internal set; }
}
