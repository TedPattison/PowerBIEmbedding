using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net;

using Microsoft.Identity.Client;

using Microsoft.Rest;
using Microsoft.PowerBI.Api;
using Newtonsoft.Json;
using PowerBiWebApp.Models.Security;

namespace PowerBiWebApp.Models {
  public class PowerBiServiceApiManager {

    private readonly static Guid appWorkspaceId = (ConfigurationManager.AppSettings["app-workspace-id"] != "") ?
                                                 new Guid(ConfigurationManager.AppSettings["app-workspace-id"]) :
                                                 Guid.Empty;


    private static async Task<string> ExecuteGetRequest(string urlRestEndpoint) {

      string accessToken = TokenManager.GetAccessToken();

      HttpClient client = new HttpClient();
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlRestEndpoint);
      request.Headers.Add("Authorization", "Bearer " + accessToken);
      request.Headers.Add("Accept", "application/json;odata.metadata=minimal");

      HttpResponseMessage response = await client.SendAsync(request);

      if (response.StatusCode != HttpStatusCode.OK) {
        throw new ApplicationException("Error!!!!!");
      }

      return await response.Content.ReadAsStringAsync();
    }

    public static async Task<WorkspacesViewModel> GetWorkspaces() {

      PowerBIClient pbiClient = TokenManager.GetPowerBiClient();
      var workspaces = (await pbiClient.Groups.GetGroupsAsync()).Value;
      return new WorkspacesViewModel { workspaces = workspaces };
    
    }

    public static async Task<WorkspacesViewModel> GetAdminWorkspaces() {

      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantReadAll);
      var workspaces = (await pbiClient.Groups.GetGroupsAsAdminAsync(top:100)).Value;
      return new WorkspacesViewModel { workspaces = workspaces };

    }

    public static async Task<string> GetViewModelJSON() {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient();
      Object viewModel;
      if (appWorkspaceId == Guid.Empty) {
        viewModel = new {
          datasets = (await pbiClient.Datasets.GetDatasetsAsync()).Value,
          reports = (await pbiClient.Reports.GetReportsAsync()).Value,
          dashboards = (await pbiClient.Dashboards.GetDashboardsAsync()).Value,
          token = TokenManager.GetAccessToken()
        };
      }
      else {
        viewModel = new {
          datasets = (await pbiClient.Datasets.GetDatasetsInGroupAsync(appWorkspaceId)).Value,
          reports = (await pbiClient.Reports.GetReportsInGroupAsync(appWorkspaceId)).Value,
          dashboards = (await pbiClient.Dashboards.GetDashboardsInGroupAsync(appWorkspaceId)).Value,
          token = TokenManager.GetAccessToken()
        };
      }

      return JsonConvert.SerializeObject(viewModel);
    }

  }
}