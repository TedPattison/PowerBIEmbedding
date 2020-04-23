using System;
using Microsoft.PowerBI.Api;
using PowerBIPublicClient.Models;

namespace PowerBIPublicClient {

  class Program {

    static void Main() {
      DisplayReportsPersonalWorkspace();
      //DisplayUserWorkspaces();
      //DisplayMultipleItems();
      //DisplayAllWorkspacesInTenant();
    }

    public static void DisplayReportsPersonalWorkspace() {

      var requestedScopes = PowerBiPermissionScopes.ReadWorkspaceAssets;
      PowerBIClient pbiClient = TokenManager.GetPowerBiClientInteractive(requestedScopes);

      var reports = pbiClient.Reports.GetReports().Value;

      Console.WriteLine("Reports in personal workspace");
      foreach (var report in reports) {
        Console.WriteLine($" - {report.Name} [{report.Id.ToString()}]");
      }
      Console.WriteLine();
    }

    public static void DisplayUserWorkspaces() {

      var requestedScopes = PowerBiPermissionScopes.ReadUserWorkspaces;
      PowerBIClient pbiClient = TokenManager.GetPowerBiClientInteractive(requestedScopes);

      var workspaces = pbiClient.Groups.GetGroups().Value;

      Console.WriteLine("Workspaces in which current user is a member");
      foreach (var workspace in workspaces) {
        Console.WriteLine($" - {workspace.Name} [{workspace.Id.ToString()}]");
      }
      Console.WriteLine();
    }

    public static void DisplayMultipleItems() {

      var requestedScopes1 = PowerBiPermissionScopes.ReadWorkspaceAssets;
      PowerBIClient pbiClient1 = TokenManager.GetPowerBiClient(requestedScopes1);

      Console.WriteLine("Reports in personal workspace");
      var reports = pbiClient1.Reports.GetReports().Value;
      foreach (var report in reports) {
        Console.WriteLine($" - {report.Name} [{report.Id.ToString()}]");
      }
      Console.WriteLine();

      var requestedScopes2 = PowerBiPermissionScopes.ReadUserWorkspaces;
      PowerBIClient pbiClient2 = TokenManager.GetPowerBiClient(requestedScopes2);

      var workspaces = pbiClient2.Groups.GetGroups().Value;

      Console.WriteLine("Workspaces in which current user is a member");
      foreach (var workspace in workspaces) {
        Console.WriteLine($" - {workspace.Name} [{workspace.Id.ToString()}]");
      }
      Console.WriteLine();
    }

    public static void DisplayAllWorkspacesInTenant() {

      var requestedScopes = PowerBiPermissionScopes.TenantReadAll;
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(requestedScopes);

      var workspaces = pbiClient.Groups.GetGroupsAsAdmin(top: 100).Value;

      Console.WriteLine("All workspaces in current tenant");
      foreach (var workspace in workspaces) {
        Console.WriteLine($" - {workspace.Name} [{workspace.Id.ToString()}]");
      }
      Console.WriteLine();
    }


  }
}
