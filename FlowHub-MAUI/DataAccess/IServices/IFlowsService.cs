


namespace FlowHub_MAUI.DataAccess.IServices;

public interface IFlowsService
{
    void GetFlows();
    Task LoadFlowsToDBFromOnline();
    UserModelView? GetUserAccount(ParseUser? usr = null);
    string UpdateFlow(FlowsModelView flow);
    bool SyncAllDataToDatabaseAsync(IEnumerable<FlowsModelView> flows, IEnumerable<FlowCommentsView> flowComments);
    Task<bool> LogUserIn(ParseUser user, string password);
    Task<bool> SignUpUser(ParseUser user);
    void UpdateUser(UserModelView user);
    string AddFlowComment(FlowCommentsView comment);
    void SaveBoth(FlowsModelView flow, FlowCommentsView comment);
    Task LoadFlowAndCommentsLinksToDBFromOnline();
    Task LoadFlowCommentsToDBFromOnline();
    Task SyncAllData();

    IList<FlowsModelView>? AllFlows { get; internal set; }
    IList<FlowModelAndCommentLink>? AllFlowAndCommentsLink { get; set; }
    IList<FlowCommentsView>? AllFlowComments { get; set; }
}
