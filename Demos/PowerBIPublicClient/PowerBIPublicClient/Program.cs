using System;
using Microsoft.PowerBI.Api;
using PowerBIPublicClient.Models;

namespace PowerBIPublicClient {

  class Program {

    static void Main() {
      DisplayUserWorkspaces();
      //DisplayReportsPersonalWorkspace();
    }

    public static void DisplayUserWorkspaces() {

      var requestedScopes = PowerBiPermissionScopes.ReadUserWorkspaces;
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(requestedScopes);

      var workspaces = pbiClient.Groups.GetGroups().Value;

      foreach (var workspace in workspaces) {
        Console.WriteLine($" - {workspace.Name} [{workspace.Id.ToString()}]");
      }
      Console.WriteLine();
    }

    public static void DisplayReportsPersonalWorkspace() {

      var requestedScopes = PowerBiPermissionScopes.;
      PowerBIClient pbiClient = TokenManager.GetPowerBiClient(requestedScopes);

      var reports = pbiClient.Reports.GetReports().Value;
      foreach (var report in reports) {
        Console.WriteLine($" - {report.Name} [{report.Id.ToString()}]");
      }
      Console.WriteLine();
    }

  }
}
