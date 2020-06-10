using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.PowerBI.Api.Models;
using UserOwnsData.Services;

namespace UserOwnsData.Pages {

  public class WorkspaceModel : PageModel {

    private PowerBiServiceApi PowerBiApi;
    public WorkspaceViewModel WorkspaceDetails;

    public WorkspaceModel(PowerBiServiceApi powerBiApi) {
      this.PowerBiApi = powerBiApi;
    }

    public async Task OnGet(string workspaceId) {

      this.WorkspaceDetails = await this.PowerBiApi.GetWorkspaceDetails(workspaceId);
    }
  }
}