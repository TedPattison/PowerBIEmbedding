using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UserOwnsData.Services;

namespace UserOwnsData.Pages {

  public class CreateWorkspaceModel : PageModel {

    private PowerBiServiceApi PowerBiApi;
    private readonly IWebHostEnvironment Env;

    public CreateWorkspaceModel(PowerBiServiceApi powerBiApi, IWebHostEnvironment env) {
      this.PowerBiApi = powerBiApi;
      this.Env = env;
    }

    [BindProperty]
    public string WorkspaceName { get; set; }

    [BindProperty]
    public bool AddContent { get; set; }

    public void OnGet() {
      // no code required for get operation
    }

    public IActionResult OnPost() {

      try {
        string appWorkspaceId = this.PowerBiApi.CreateAppWorkspace(this.WorkspaceName);
        if (AddContent) {
          // upload sample PBIX file #1
          string pbixPath = this.Env.WebRootPath + @"/PBIX/Wingtip Sales Analysis.pbix";
          string importName = "Wingtip Sales";
          this.PowerBiApi.PublishPBIX(appWorkspaceId, pbixPath, importName);
          // upload sample PBIX file #2
          pbixPath = this.Env.WebRootPath + @"/PBIX/Northwind Retro.pbix";
          importName = "Northwind Retro";
          this.PowerBiApi.PublishPBIX(appWorkspaceId, pbixPath, importName);
        }
        return RedirectToPage("/Workspaces");
      }
      catch (Exception ex) {
        string errorMessage = "Error trying to create new app workspace - " + ex.Message;
        return RedirectToPage("/Error", new { ErrorMessage = errorMessage });
      }

    }

  }
}