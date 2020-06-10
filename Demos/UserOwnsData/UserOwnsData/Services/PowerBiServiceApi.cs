using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UserOwnsData.Services {

  public class WorkspaceViewModel {
    public Group Workspace;
    public IList<Dashboard> Dashboards;
    public IList<Report> Reports;
    public IList<Dataset> Datasets;
    public IList<Dataflow> Dataflows;
  }

  public class PowerBiServiceApi {

    public static readonly string[] RequiredScopes =
      new string[] {
        "email",
        "profile",
        "https://analysis.windows.net/powerbi/api/Group.Read.All",
        "https://analysis.windows.net/powerbi/api/Dashboard.Read.All",
        "https://analysis.windows.net/powerbi/api/Report.ReadWrite.All",
        "https://analysis.windows.net/powerbi/api/Dataset.Read.All",
        "https://analysis.windows.net/powerbi/api/Dataflow.Read.All",
        "https://analysis.windows.net/powerbi/api/Content.Create",
        "https://analysis.windows.net/powerbi/api/Workspace.ReadWrite.All",
    };

    readonly ITokenAcquisition tokenAcquisition;
    readonly string urlPowerBiServiceApiRoot;

    public PowerBiServiceApi(ITokenAcquisition tokenAcquisition, IConfiguration configuration) {
      this.tokenAcquisition = tokenAcquisition;
      this.urlPowerBiServiceApiRoot = configuration["PowerBi:ServiceRootUrl"];
    }

    private PowerBIClient getPowerBiClient() {
      var accessToken = this.tokenAcquisition.GetAccessTokenForUserAsync(RequiredScopes).Result;
      var tokenCredentials = new TokenCredentials(accessToken, "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);
    }

    public async Task<IList<Group>> GetWorkspaces() {
      PowerBIClient pbiClient = this.getPowerBiClient();
      var workspaces = (await pbiClient.Groups.GetGroupsAsync()).Value;
      return workspaces;
    }

    public async Task<WorkspaceViewModel> GetWorkspaceDetails(string workspaceId) {

      PowerBIClient pbiClient = this.getPowerBiClient();

      string filter = $"id eq '{workspaceId}'";

      var workspaceIdGuid = new Guid(workspaceId);

      return new WorkspaceViewModel {
        Workspace = (await pbiClient.Groups.GetGroupsAsync(filter)).Value.First(),
        Dashboards = (await pbiClient.Dashboards.GetDashboardsInGroupAsync(workspaceIdGuid)).Value,
        Reports = (await pbiClient.Reports.GetReportsInGroupAsync(workspaceIdGuid)).Value,
        Datasets = (await pbiClient.Datasets.GetDatasetsInGroupAsync(workspaceIdGuid)).Value,
        Dataflows = (await pbiClient.Dataflows.GetDataflowsAsync(workspaceIdGuid)).Value
      };

    }

    public string CreateAppWorkspace(string Name) {
      PowerBIClient pbiClient = this.getPowerBiClient();
      // create new app workspace
      GroupCreationRequest request = new GroupCreationRequest(Name);
      Group aws = pbiClient.Groups.CreateGroup(request);
      // return app workspace ID
      return aws.Id.ToString();
    }

    public void DeleteAppWorkspace(string WorkspaceId) {
      PowerBIClient pbiClient = this.getPowerBiClient();
      Guid workspaceIdGuid = new Guid(WorkspaceId);
      pbiClient.Groups.DeleteGroup(workspaceIdGuid);
    }

    public void PublishPBIX(string appWorkspaceId, string PbixFilePath, string ImportName) {
      PowerBIClient pbiClient = this.getPowerBiClient();
      FileStream stream = new FileStream(PbixFilePath, FileMode.Open, FileAccess.Read);
      var import = pbiClient.Imports.PostImportWithFileInGroup(new Guid(appWorkspaceId), stream, ImportName);
      Console.WriteLine("Publishing process completed");
    }

    public async Task<string> GetEmbeddedViewModel(string appWorkspaceId = "") {

      var accessToken = this.tokenAcquisition.GetAccessTokenForUserAsync(RequiredScopes).Result;
      var tokenCredentials = new TokenCredentials(accessToken, "Bearer");
      PowerBIClient pbiClient = new PowerBIClient(new Uri(urlPowerBiServiceApiRoot), tokenCredentials);

      Object viewModel;
      if (string.IsNullOrEmpty(appWorkspaceId)) {
        viewModel = new {
          currentWorkspace = "My Workspace",
          workspaces = (await pbiClient.Groups.GetGroupsAsync()).Value,
          datasets = (await pbiClient.Datasets.GetDatasetsAsync()).Value,
          reports = (await pbiClient.Reports.GetReportsAsync()).Value,
          dashboards = (await pbiClient.Dashboards.GetDashboardsAsync()).Value,
          token = accessToken
        };
      }
      else {
        Guid workspaceId = new Guid(appWorkspaceId);
        var workspaces = (await pbiClient.Groups.GetGroupsAsync()).Value;
        var currentWorkspace = workspaces.First((workspace) => workspace.Id == workspaceId);
        viewModel = new {
          workspaces = workspaces,
          currentWorkspace = currentWorkspace.Name,
          currentWorkspaceIsReadOnly = currentWorkspace.IsReadOnly,
          datasets = (await pbiClient.Datasets.GetDatasetsInGroupAsync(workspaceId)).Value,
          reports = (await pbiClient.Reports.GetReportsInGroupAsync(workspaceId)).Value,
          dashboards = (await pbiClient.Dashboards.GetDashboardsInGroupAsync(workspaceId)).Value,
          token = accessToken
        };
      }

      return JsonConvert.SerializeObject(viewModel);
    }

  }

}
