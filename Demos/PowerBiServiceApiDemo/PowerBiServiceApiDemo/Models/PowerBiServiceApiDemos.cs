using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PowerBiServiceApiDemo.Models {

  class PowerBiServiceApiDemos {

    private static string ExecuteGetRequest(string restUrl, string accessToken) {
      HttpClient client = new HttpClient();
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, restUrl);
      request.Headers.Add("Authorization", "Bearer " + accessToken);
      request.Headers.Add("Accept", "application/json;odata.metadata=minimal");
      HttpResponseMessage response = client.SendAsync(request).Result;
      if (response.StatusCode != HttpStatusCode.OK) {
        throw new ApplicationException("Error occured calling the Power BI Servide API");
      }
      return response.Content.ReadAsStringAsync().Result;
    }

    public static void DisplayUserWorkspaces() {

      string restUrl = "https://api.powerbi.com/v1.0/myorg/groups/";

      string accessToken = TokenManager.GetAccessToken(PowerBiPermissionScopes.ReadUserWorkspaces);

      var jsonResponse = ExecuteGetRequest(restUrl, accessToken);
      PbiWorkspaceCollection workspaces = JsonConvert.DeserializeObject<PbiWorkspaceCollection>(jsonResponse);

      foreach (PbiWorkspace workspace in workspaces.value) {
        Console.WriteLine(workspace.name);
      }

      Console.WriteLine();
    }

    public static void DisplayReportsInPersonalWorkspace() {

      string restUrl = "https://api.powerbi.com/v1.0/myorg/reports/";
      string accessToken = TokenManager.GetAccessToken(PowerBiPermissionScopes.ReadWorkspaceAssets);

      var jsonResponse = ExecuteGetRequest(restUrl, accessToken);
      PbiReportCollection reports = JsonConvert.DeserializeObject<PbiReportCollection>(jsonResponse);

      Console.WriteLine("Reports in personal workspace");
      foreach (PbiReport report in reports.value) {
        Console.WriteLine($" - {report.name}");
      }
      Console.WriteLine();
    }

  }

  public class PbiWorkspace {
    public string id { get; set; }
    public bool isReadOnly { get; set; }
    public bool isOnDedicatedCapacity { get; set; }
    public string name { get; set; }
  }

  public class PbiWorkspaceCollection {
    public List<PbiWorkspace> value { get; set; }
  }

  public class PbiReport {
    public string id { get; set; }
    public string name { get; set; }
    public string webUrl { get; set; }
    public string embedUrl { get; set; }
    public bool isOwnedByMe { get; set; }
    public string datasetId { get; set; }
  }

  public class PbiReportCollection {
    public List<PbiReport> value { get; set; }
  }

}
