

// TODO: add Power BI configuration data for workspace ID and report ID
var workspaceId = "POWERBI_WORKSPACE_ID_HERE";
var reportId = "POWERBI_REPORT_ID_HERE";

// TODO: add configuration data for Azure AD application which supports implicit flow
var clientId = "AZURE_APPLICATION_CLIENT_ID_HERE";
var tenantName = "TENANT_NAME_HERE"; 
var redirectUri = "https://localhost:44300";

// calculate authority from tenant name
var authority = "https://login.microsoftonline.com/" + tenantName;

const msalConfig = {
  auth: { clientId: clientId, authority: authority, redirectUri: redirectUri },
  cache: { cacheLocation: "localStorage", storeAuthStateInCookie: true }
};

const azureAppUserAgent = new Msal.UserAgentApplication(msalConfig);

function login() {
  azureAppUserAgent.loginPopup(scopesReadReport)
    .then(function (loginResponse) { updateIU(); })
    .catch(function (error) { console.log(error); });
}

function logout() {
  azureAppUserAgent.logout();
  updateIU();
}

const scopesReadReport = {
  scopes: [
    "https://analysis.windows.net/powerbi/api/Report.Read.All"
  ]
};

const scopesEditReport = {
  scopes: [
    "https://analysis.windows.net/powerbi/api/Report.ReadWrite.All"
  ]
};

function getAccessToken(scopes) {
  // create deferred promise to return to caller
  var deferred = $.Deferred();
  console.log("attempting to get token using acquireTokenSilent");
  azureAppUserAgent.acquireTokenSilent(scopes)
    .then(function (tokenResponse) {
      console.log("Token acquired with acquireTokenSilent");
      deferred.resolve(tokenResponse.accessToken);
    })
    .catch(function (err) {
      console.log("acquireTokenSilent failed:");
      console.log("attempting to get token using acquireTokenPopup");
      azureAppUserAgent.acquireTokenPopup(scopes)
        .then(function (tokenResponse) {
          console.log("Token acquired with acquireTokenPopup");
          deferred.resolve(tokenResponse.accessToken);
        })
        .catch(function (err) {
          console.log("acquireTokenPopup failed");
          deferred.fail();
        });
    });
  return deferred;
}

var pbiApiRoot = "https://api.powerbi.com/v1.0/myorg/";
var restUrlGetReport = pbiApiRoot + "groups/" + workspaceId + "/reports/" + reportId + "/";
var models = window['powerbi-client'].models;

function embedReport() {

  // call getAccessToken() and pass callback function to done()
  getAccessToken(scopesReadReport).done(function (accessToken) {
    // access token acquired 
    // call Power BI Service API to get embed URL
    $.ajax({
      url: restUrlGetReport,
      headers: {
        "Accept": "application/json;odata.metadata=minimal;",
        "Authorization": "Bearer " + accessToken
      }
    })
      .done(function (data) {
        // call to Power BI Seervice now complete
        // get embed URL using data.embedUrl
        var config = {
          type: 'report',
          id: reportId,
          embedUrl: data.embedUrl,
          accessToken: accessToken,
          tokenType: models.TokenType.Aad,
          permissions: models.Permissions.All,
          viewMode: models.ViewMode.View,
          settings: {
            filterPaneEnabled: false,
            navContentPaneEnabled: true,
          }
        };

        // Get a reference to the embedded report HTML element
        var reportContainer = document.getElementById('embed-container');

        // Embed the report and display it within the div container.
        var report = powerbi.embed(reportContainer, config);

      });
  });
}

function editReport() {

  // call getAccessToken() and pass callback function to done()
  getAccessToken(scopesEditReport).done(function (accessToken) {
    // access token acquired 
    // call Power BI Service API to get embed URL
    $.ajax({
      url: restUrlGetReport,
      headers: {
        "Accept": "application/json;odata.metadata=minimal;",
        "Authorization": "Bearer " + accessToken
      }
    })
      .done(function (data) {
        // call to Power BI Seervice now complete
        // get embed URL using data.embedUrl
        var config = {
          type: 'report',
          id: reportId,
          embedUrl: data.embedUrl,
          accessToken: accessToken,
          tokenType: models.TokenType.Aad,
          permissions: models.Permissions.All,
          viewMode: models.ViewMode.Edit,
          settings: {
            filterPaneEnabled: false,
            navContentPaneEnabled: true
          }
        };

        // Get a reference to the embedded report HTML element
        var reportContainer = document.getElementById('embed-container');

        // Embed the report and display it within the div container.
        var report = powerbi.embed(reportContainer, config);

      });
  });
}

// jQuery document ready handler
$(function() {
  // attach event handlers for buttons
  $("#signin").click(login);
  $("#logout").click(logout);
  $("#embed-report").click(embedReport);
  $("#edit-report").click(editReport);
  // call init to resize embed container
  initUI();
});

function initUI() {
  var widthBuffer = 12;
  var heightBuffer = 24;
  var toolbarHeight = $("#banner").height();
  $("#embed-container").height($(window).height() - (heightBuffer + toolbarHeight));
  $("#embed-container").width($(window).width() - widthBuffer);
  $(window).resize(function () {
    var toolbarHeight = $("#banner").height();
    $("#embed-container").height($(window).height() - (heightBuffer + toolbarHeight));
    $("#embed-container").width($(window).width() - widthBuffer);
  });
}

function updateIU() {
  var userAccount = azureAppUserAgent.getAccount();
  if (userAccount) {
    $("#welcomeMessage").text("Welcome " + userAccount.name);
    $("#signin").hide();
    $("#logout").show();
    $("#top-nav").show();
  }
  else {
    $("#welcomeMessage").text("");
    $("#signin").show();
    $("#logout").hide();
    $("#top-nav").hide();
  }
}