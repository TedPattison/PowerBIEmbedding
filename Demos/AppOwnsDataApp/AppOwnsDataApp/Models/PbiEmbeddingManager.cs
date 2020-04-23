using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;

namespace AppOwnsDataApp.Models {

  public class PbiEmbeddingManager {

    private static readonly string applicationId = ConfigurationManager.AppSettings["application-id"];
    private static readonly string applicationSecret = ConfigurationManager.AppSettings["application-secret"];
    private static readonly string tenantName = ConfigurationManager.AppSettings["tenant-name"];

    private static readonly Guid workspaceId = new Guid(ConfigurationManager.AppSettings["app-workspace-id"]);
    private static readonly Guid datasetId = new Guid(ConfigurationManager.AppSettings["dataset-id"]);
    private static readonly Guid reportId = new Guid(ConfigurationManager.AppSettings["report-id"]);
    private static readonly Guid dashboardId = new Guid(ConfigurationManager.AppSettings["dashboard-id"]);

    // endpoint for tenant-specific authority 
    private static readonly string tenantAuthority = "https://login.microsoftonline.com/" + tenantName;

    // Power BI Service API Root URL
    const string urlPowerBiRestApiRoot = "https://api.powerbi.com/";

    static string GetAppOnlyAccessToken() {

      var appConfidential = ConfidentialClientApplicationBuilder.Create(applicationId)
                              .WithClientSecret(applicationSecret)
                              .WithAuthority(tenantAuthority)
                              .Build();

      string[] scopesDefault = new string[] { "https://analysis.windows.net/powerbi/api/.default" };
      var authResult = appConfidential.AcquireTokenForClient(scopesDefault).ExecuteAsync().Result;
      return authResult.AccessToken;
    }

    private static PowerBIClient GetPowerBiClient() {
      var tokenCredentials = new TokenCredentials(GetAppOnlyAccessToken(), "Bearer");
      return new PowerBIClient(new Uri(urlPowerBiRestApiRoot), tokenCredentials);
    }

    public static async Task<ReportEmbeddingData> GetReportEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);
      var embedUrl = report.EmbedUrl;
      var reportName = report.Name;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "edit");
      string embedToken =
            (await pbiClient.Reports.GenerateTokenInGroupAsync(workspaceId,
                                                               report.Id,
                                                               generateTokenRequestParameters)).Token;

      return new ReportEmbeddingData {
        reportId = reportId,
        reportName = reportName,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static async Task<DashboardEmbeddingData> GetDashboardEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var dashboard = await pbiClient.Dashboards.GetDashboardInGroupAsync(workspaceId, dashboardId);
      var embedUrl = dashboard.EmbedUrl;
      var dashboardDisplayName = dashboard.DisplayName;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");

      string embedToken =
         (await pbiClient.Dashboards.GenerateTokenInGroupAsync(workspaceId,
                                                               dashboardId,
                                                               generateTokenRequestParameters)).Token;

      return new DashboardEmbeddingData {
        dashboardId = dashboardId,
        dashboardName = dashboardDisplayName,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public async static Task<QnaEmbeddingData> GetQnaEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var dataset = await pbiClient.Datasets.GetDatasetInGroupAsync(workspaceId, datasetId.ToString());

      string embedUrl = "https://app.powerbi.com/qnaEmbed?groupId=" + workspaceId;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
      string embedToken =
             (await pbiClient.Datasets.GenerateTokenInGroupAsync(workspaceId,
                                                                 dataset.Id,
                                                                 generateTokenRequestParameters)).Token;

      return new QnaEmbeddingData {
        datasetId = datasetId,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static async Task<NewReportEmbeddingData> GetNewReportEmbeddingData() {

      string embedUrl = "https://app.powerbi.com/reportEmbed?groupId=" + workspaceId;

      PowerBIClient pbiClient = GetPowerBiClient();

      GenerateTokenRequest generateTokenRequestParameters =
                           new GenerateTokenRequest(accessLevel: "create", datasetId: datasetId.ToString());
      string embedToken =
        (await pbiClient.Reports.GenerateTokenForCreateInGroupAsync(workspaceId,
                                                                    generateTokenRequestParameters)).Token;

      return new NewReportEmbeddingData {
        workspaceId = workspaceId,
        datasetId = datasetId,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

    public static async Task<ReportEmbeddingData> GetEmbeddingDataForReport(string currentReportId) {
      // convert string-based report ID to GUID
      Guid reportId = new Guid(currentReportId);

      PowerBIClient pbiClient = GetPowerBiClient();
      var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);
      var embedUrl = report.EmbedUrl;
      var reportName = report.Name;

      GenerateTokenRequest generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "edit");
      string embedToken =
            (await pbiClient.Reports.GenerateTokenInGroupAsync(workspaceId,
                                                                reportId,
                                                                generateTokenRequestParameters)).Token;

      return new ReportEmbeddingData {
        reportId = reportId,
        reportName = reportName,
        embedUrl = embedUrl,
        accessToken = embedToken
      };

    }

  }

}