using Microsoft.PowerBI.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PowerBiWebApp.Models {

  public class WorkspacesViewModel {
    public IList<Group> workspaces;
  }

  public class ClientSideEmbeddingViewModel {
    public string jsonViewModel { get; set; }
  }

  

}