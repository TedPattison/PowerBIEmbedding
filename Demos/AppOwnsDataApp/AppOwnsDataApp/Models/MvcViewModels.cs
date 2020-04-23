using System;

namespace AppOwnsDataApp.Models {

  // data required for embedding a report
  public class ReportEmbeddingData {
    public Guid reportId;
    public string reportName;
    public string embedUrl;
    public string accessToken;
  }

  // data required for embedding a new report
  public class NewReportEmbeddingData {
    public Guid workspaceId;
    public Guid datasetId;
    public string embedUrl;
    public string accessToken;
  }

  // data required for embedding a dashboard
  public class DashboardEmbeddingData {
    public Guid dashboardId;
    public string dashboardName;
    public string embedUrl;
    public string accessToken;
  }

  // data required for embedding a dashboard
  public class QnaEmbeddingData {
    public Guid datasetId;
    public string embedUrl;
    public string accessToken;
  }

}
