function actionExportMedia() {
    top.location.href = '/umbraco/theoutfield/exportmedia/handlers/ExportMedia.ashx?id=' + UmbClientMgr.mainTree().getActionNode().nodeId;
}