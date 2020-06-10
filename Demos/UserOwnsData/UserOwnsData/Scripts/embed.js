"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var $ = require("jquery");
var pbimodels = require("powerbi-models");
require('powerbi-models');
require('powerbi-client');
var Workspace = /** @class */ (function () {
    function Workspace() {
    }
    return Workspace;
}());
var Dashboard = /** @class */ (function () {
    function Dashboard() {
    }
    return Dashboard;
}());
var Report = /** @class */ (function () {
    function Report() {
    }
    return Report;
}());
var Dataset = /** @class */ (function () {
    function Dataset() {
    }
    return Dataset;
}());
var ViewModel = /** @class */ (function () {
    function ViewModel() {
    }
    return ViewModel;
}());
$(function () {
    $("#embed-toolbar").hide();
    var workspaceSelector = $("#workspace-selector");
    var workspacesList = $("#workspaces-list");
    var dashboardsList = $("#dashboards-list");
    var reportsList = $("#reports-list");
    var datasetsList = $("#datasets-list");
    var viewModel = window['viewModel'];
    console.log("cw", viewModel.currentWorkspace);
    workspaceSelector.text(viewModel.currentWorkspace);
    var linkMyWorkspace = $("<a>", { "href": "/embed/" })
        .text("My Workspace")
        .addClass("dropdown-item");
    workspacesList.append(linkMyWorkspace);
    workspacesList.append($("<div>").addClass("dropdown-divider"));
    viewModel.workspaces.forEach(function (workspace) {
        var link = $("<a>", { "href": "/embed/?workspaceId=" + workspace.id })
            .text(workspace.name)
            .addClass("dropdown-item");
        workspacesList.append(link);
    });
    if (viewModel.dashboards.length == 0) {
        dashboardsList.append($("<li>").text("You have no dashboards"));
    }
    else {
        viewModel.dashboards.forEach(function (dashboard) {
            var li = $("<li>");
            li.append($("<i>").addClass("fa fa-dashboard"));
            li.append($("<a>", {
                "href": "javascript:void(0);"
            }).text(dashboard.displayName).click(function () { embedDashboard(dashboard); }));
            dashboardsList.append(li);
        });
    }
    if (viewModel.reports.length == 0) {
        reportsList.append($("<li>").text("You have no reports"));
    }
    else {
        viewModel.reports.forEach(function (report) {
            var li = $("<li>");
            li.append($("<i>").addClass("fa fa-bar-chart"));
            li.append($("<a>", {
                "href": "javascript:void(0);"
            }).text(report.name).click(function () { embedReport(report); }));
            reportsList.append(li);
        });
    }
    if (viewModel.datasets.length == 0) {
        datasetsList.append($("<li>").text("You have no datasets"));
    }
    else {
        viewModel.datasets.forEach(function (dataset) {
            var li = $("<li>");
            li.append($("<i>").addClass("fa fa-database"));
            li.append($("<a>", {
                "href": "javascript:void(0);"
            }).text(dataset.name).click(function () { embedNewReport(dataset); }));
            datasetsList.append(li);
        });
    }
    // load new report in edit mode as Save action on new report
    var urlParams = new URLSearchParams(window.location.search);
    var newReportId = urlParams.get("newReport");
    if (newReportId) {
        var newReport = viewModel.reports.find(function (report) { return report.id == newReportId; });
        embedReport(newReport, true);
    }
});
var embedReport = function (report, editMode) {
    if (editMode === void 0) { editMode = false; }
    $("#embedding-instructions").hide();
    var viewModel = window['viewModel'];
    var token = viewModel.token;
    var models = pbimodels;
    var config = {
        type: 'report',
        id: report.id,
        embedUrl: report.embedUrl,
        accessToken: token,
        tokenType: models.TokenType.Aad,
        permissions: models.Permissions.All,
        viewMode: editMode ? models.ViewMode.Edit : models.ViewMode.View,
        settings: {
            panes: {
                filters: { visible: false },
                pageNavigation: { visible: true }
            }
        }
    };
    // Get a reference to the embedded report HTML element
    var reportContainer = document.getElementById('embed-container');
    var powerbi = window.powerbi;
    // Embed the report and display it within the div container.
    powerbi.reset(reportContainer);
    var embeddedReport = powerbi.embed(reportContainer, config);
    embeddedReport.on("saved", function (event) {
        // check for saveAs
        if (event.detail.saveAs) {
            if (window.location.href.includes("?")) {
                window.location.href = window.location.href + "&newReport=" + event.detail.reportObjectId;
            }
            else {
                window.location.href = window.location.href + "?newReport=" + event.detail.reportObjectId;
            }
        }
    });
    var viewMode = editMode ? "edit" : "view";
    $("#breadcrumb").text("Reports > " + report.name);
    $("#embed-toolbar").show();
    if (viewModel.currentWorkspaceIsReadOnly) {
        $("#toggle-edit").hide();
    }
    else {
        $("#toggle-edit").show();
        $("#toggle-edit").unbind("click");
        $("#toggle-edit").click(function () {
            // toggle between view and edit mode
            viewMode = (viewMode == "view") ? "edit" : "view";
            embeddedReport.switchMode(viewMode);
            // show filter pane when entering edit mode
            var showFilterPane = (viewMode == "edit");
            embeddedReport.updateSettings({
                panes: {
                    filters: { visible: showFilterPane, expanded: false }
                }
            });
        });
        $("#full-screen").unbind("click");
        $("#full-screen").click(function () {
            embeddedReport.fullscreen();
        });
    }
    ;
};
var embedDashboard = function (dashboard) {
    $("#embedding-instructions").hide();
    var viewModel = window['viewModel'];
    var token = viewModel.token;
    var models = pbimodels;
    var config = {
        type: 'dashboard',
        id: dashboard.id,
        embedUrl: dashboard.embedUrl,
        accessToken: token,
        tokenType: models.TokenType.Aad,
        pageView: "fitToWidth" // options: "actualSize", "fitToWidth", "oneColumn"
    };
    // Get a reference to the embedded report HTML element
    var reportContainer = document.getElementById('embed-container');
    var powerbi = window.powerbi;
    // Embed the report and display it within the div container.
    powerbi.reset(reportContainer);
    var embeddedDashboard = powerbi.embed(reportContainer, config);
    $("#breadcrumb").text("Dashboards > " + dashboard.displayName);
    $("#embed-toolbar").show();
    $("#toggle-edit").hide();
    $("#full-screen").unbind("click");
    $("#full-screen").click(function () {
        embeddedDashboard.fullscreen();
    });
};
var embedNewReport = function (dataset) {
    $("#embedding-instructions").hide();
    var viewModel = window['viewModel'];
    var token = viewModel.token;
    var models = pbimodels;
    var config = {
        datasetId: dataset.id,
        embedUrl: "https://app.powerbi.com/reportEmbed",
        accessToken: token,
        tokenType: models.TokenType.Aad,
        settings: {
            panes: {
                filters: { visible: true, expanded: false }
            }
        }
    };
    // Get a reference to the embedded report HTML element
    var reportContainer = document.getElementById('embed-container');
    var powerbi = window.powerbi;
    // Embed the report and display it within the div container.
    powerbi.reset(reportContainer);
    var embeddedReport = powerbi.createReport(reportContainer, config);
    $("#breadcrumb").text("Datasets > " + dataset.name + " > New Report");
    $("#embed-toolbar").show();
    $("#toggle-edit").hide();
    $("#full-screen").unbind("click");
    $("#full-screen").click(function () {
        embeddedReport.fullscreen();
    });
    // handle save action on new report to refresh page
    embeddedReport.on("saved", function (event) {
        console.log(event);
        console.log((typeof event));
        if (window.location.href.includes("?")) {
            window.location.href = window.location.href + "&newReport=" + event.detail.reportObjectId;
        }
        else {
            window.location.href = window.location.href + "?newReport=" + event.detail.reportObjectId;
        }
    });
    embeddedReport.on("saved", function (event) {
        console.log(event);
        console.log((typeof event));
        if (window.location.href.includes("?")) {
            window.location.href = window.location.href + "&newReport=" + event.detail.reportObjectId;
        }
        else {
            window.location.href = window.location.href + "?newReport=" + event.detail.reportObjectId;
        }
    });
};
// reset left-nav to height of browser window
$(function () {
    var heightBuffer = 8;
    var newHeight = window.innerHeight - ($("#banner").height() + heightBuffer);
    $("#left-nav").height(newHeight);
    $("#content-box").height(newHeight);
    $(window).resize(function () {
        var newHeight = window.innerHeight - ($("#banner").height() + heightBuffer);
        $("#left-nav").height(newHeight);
        $("#content-box").height(newHeight);
    });
});
//# sourceMappingURL=embed.js.map