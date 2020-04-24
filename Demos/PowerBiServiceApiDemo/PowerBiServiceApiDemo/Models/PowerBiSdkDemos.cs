using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using System;
using System.IO;
using System.Linq;


namespace PowerBiServiceApiDemo.Models {

  class PowerBiSdkDemos {

    public static void DisplayUserWorkspaces() {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ReadUserWorkspaces);
      Console.WriteLine("Workspaces:");
      var workspaces = pbiClient.Groups.GetGroups().Value;
      foreach (var workspace in workspaces) {
        Console.WriteLine($" - {workspace.Name} [{workspace.Id.ToString()}]");
      }
      Console.WriteLine();
    }

    public static Group GetWorkspaceByName(string WorkspaceName) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ReadUserWorkspaces);
      var workspaces = pbiClient.Groups.GetGroups().Value;
      foreach (var workspace in workspaces) {
        if (workspace.Name.Equals(WorkspaceName,StringComparison.CurrentCultureIgnoreCase)) {
          return workspace;
        }
      }
      // return null if workspace is not found in user workspaces
      return null;
    }

    public static Group GetWorkspaceById(Guid WorkspaceId) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ReadUserWorkspaces);
      string groupsFilter = $"id eq '{WorkspaceId.ToString()}'";
      return pbiClient.Groups.GetGroups(groupsFilter).Value.FirstOrDefault();
    }

    public static void GetWorkspaceInfo(string WorkspaceName) {
      Group workspace = GetWorkspaceByName(WorkspaceName);
      if (workspace!=null) {
        ;
        Console.WriteLine($"Workspace Info:");
        Console.WriteLine($"  Name: {workspace.Name}");
        Console.WriteLine($"  ID: {workspace.Id.ToString()}");
        Console.WriteLine($"  IsReadOnly: {workspace.IsReadOnly.GetValueOrDefault()}");
        Console.WriteLine($"  IsOnDedicatedCapacity: {workspace.IsOnDedicatedCapacity.GetValueOrDefault()}");
      }
      else {
        Console.WriteLine($"Workspace named {WorkspaceName} not found");
      }
      Console.WriteLine();
    }

    public static void DeleteAllUserWorkspaces() {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ReadUserWorkspaces);
      Console.WriteLine("Deleting Workspaces....");
      var workspaces = pbiClient.Groups.GetGroups().Value;
      foreach (var workspace in workspaces) {
        Console.WriteLine($" - Deleting {workspace.Name} [{workspace.Id.ToString()}]");
        pbiClient.Groups.DeleteGroup(workspace.Id);
      }
      Console.WriteLine();
    }

    public static void DisplayAdminWorkspaces() {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.TenantReadAll);
      Console.WriteLine("Admin Workspaces:");
      var workspaces = pbiClient.Groups.GetGroupsAsAdmin(top:100, filter: "state ne 'Deleted'").Value;
      foreach (var workspace in workspaces) {
        Console.WriteLine($" - {workspace.Name} [{workspace.Id.ToString()}]");
      }
      Console.WriteLine();
    }

    public static void DisplayWorkspaceAssets(string WorkspaceName) {
      Console.WriteLine();

      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ReadWorkspaceAssets);

      Group workspace = GetWorkspaceByName(WorkspaceName);
      if (workspace==null) {
        Console.WriteLine($"Workspace named {WorkspaceName} not found");
      }
      else {

        Console.WriteLine("Datasets:");
        var datasets = pbiClient.Datasets.GetDatasetsInGroup(workspace.Id).Value;
        foreach (var dataset in datasets) {
          Console.WriteLine(" - " + dataset.Name + " [" + dataset.Id + "]");
        }

        Console.WriteLine();
        Console.WriteLine("Reports:");
        var reports = pbiClient.Reports.GetReportsInGroup(workspace.Id).Value;
        foreach (var report in reports) {
          Console.WriteLine(" - " + report.Name + " [" + report.Id + "]");
        }

        Console.WriteLine();
        Console.WriteLine("Dashboards:");
        var dashboards = pbiClient.Dashboards.GetDashboardsInGroup(workspace.Id).Value;
        foreach (var dashboard in dashboards) {
          Console.WriteLine(" - " + dashboard.DisplayName + " [" + dashboard.Id + "]");
        }

      }
      Console.WriteLine();      
    }

    public static void CreateAndPopulateWorkspace(string WorkspaceName) {
      
      Guid workspaceId = CreateAppWorkspace(WorkspaceName);
      MemoryStream stream = new MemoryStream(Properties.Resources.Wingtip_Sales_Analysis_pbix);
      PublishPBIX(workspaceId, stream, "Wingtip Sales");

    }

    static Guid CreateAppWorkspace(string Name) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ManageWorkspaceAssets);
      // create new app workspace
      GroupCreationRequest request = new GroupCreationRequest(Name);
      Group aws = pbiClient.Groups.CreateGroup(request);
      // return app workspace ID
      return aws.Id;
    }

    static void PublishPBIX(Guid appWorkspaceId, string PbixFilePath, string ImportName) {
      Console.WriteLine("Publishing " + PbixFilePath);
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ManageWorkspaceAssets);
      FileStream stream = new FileStream(PbixFilePath, FileMode.Open, FileAccess.Read);
      var import = pbiClient.Imports.PostImportWithFileInGroup(appWorkspaceId, stream, ImportName);
      Console.WriteLine("Publishing process completed");
    }

    static void PublishPBIX(Guid appWorkspaceId, Stream PbixFileStream, string ImportName) {
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(PowerBiPermissionScopes.ManageWorkspaceAssets);
      var import = pbiClient.Imports.PostImportWithFileInGroup(appWorkspaceId, PbixFileStream, ImportName);
      Console.WriteLine("Publishing process completed");
    }
  }
}
