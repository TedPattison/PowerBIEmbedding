using PowerBiServiceApiDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerBiServiceApiDemo {
  class Program {
    static void Main() {
      Console.WriteLine();

      PowerBiServiceApiDemos.DisplayReportsInPersonalWorkspace();
      //PowerBiServiceApiDemos.DisplayUserWorkspaces();

      //PowerBiSdkDemos.DisplayUserWorkspaces();
      // PowerBiSdkDemos.DisplayAdminWorkspaces();

      //PowerBiSdkDemos.DeleteAllUserWorkspaces();
      //PowerBiSdkDemos.CreateAndPopulateWorkspace("Wingtip Sales");

      //PowerBiSdkDemos.GetWorkspaceInfo("wingtip Sales");
      //PowerBiSdkDemos.DisplayWorkspaceAssets("wingtip Sales");

    }
  }
}
