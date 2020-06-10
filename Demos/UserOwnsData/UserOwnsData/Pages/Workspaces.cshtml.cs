using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.PowerBI.Api.Models;
using UserOwnsData.Services;

namespace UserOwnsData.Pages {

  public class WorkspacesModel : PageModel {

    public PowerBiServiceApi PowerBiApi;
    public IList<Group> workspaces;

    public WorkspacesModel(PowerBiServiceApi powerBiApi) {
      this.PowerBiApi = powerBiApi;
    }

    public async Task OnGet(string workspaceId) {

      this.workspaces = await this.PowerBiApi.GetWorkspaces();
    }

  }
}