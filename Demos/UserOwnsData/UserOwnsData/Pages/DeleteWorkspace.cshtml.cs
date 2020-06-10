using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UserOwnsData.Services;

namespace UserOwnsData.Pages {
  public class DeleteWorkspaceModel : PageModel {

    private PowerBiServiceApi PowerBiApi;

    public DeleteWorkspaceModel(PowerBiServiceApi powerBiApi) {
      this.PowerBiApi = powerBiApi;
    }

    public IActionResult OnGet(string workspaceId) {
      // delete workspace
      this.PowerBiApi.DeleteAppWorkspace(workspaceId);
      // return to workspaces page
      return RedirectToPage("/Workspaces");
    }

  }
}