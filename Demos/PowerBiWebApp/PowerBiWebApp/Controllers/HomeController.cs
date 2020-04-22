using PowerBiWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PowerBiWebApp.Controllers {
  public class HomeController : Controller {

    public ActionResult Index() {
      return View();
    }

    [Authorize]
    public async Task<ActionResult> Workspaces() {
      var viewModel = await PowerBiServiceApiManager.GetWorkspaces();
      return View(viewModel);

    }

    [Authorize]
    public async Task<ActionResult> AdminWorkspaces() {
      var viewModel = await PowerBiServiceApiManager.GetAdminWorkspaces();
      return View(viewModel);

    }

    [Authorize]
    public async Task<ActionResult> ClientSideEmbedding() {
        string json = await PowerBiServiceApiManager.GetViewModelJSON();
        return View(new ClientSideEmbeddingViewModel { jsonViewModel = json });      
    }

  }
}