<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="RocketDirectoryMod.Settings" %>

<script type="text/javascript" src="/DesktopModules/DNNrocket/Simplisity/js/simplisity.js"></script>
<link rel="stylesheet" href="/DesktopModules/DNNrocket/css/w3.css">
<link rel="stylesheet" href="/DesktopModules/DNNrocket/Simplisity/css/simplisity.css">
<link rel="stylesheet" href="/DesktopModules/DNNrocket/API/Themes/config-w3/1.0/css/dnn-theme.css">
<link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:regular,bold,italic,thin,light,bolditalic,black,medium">
<link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">

<style>
    .material-icons { vertical-align: middle; }
</style>

<asp:PlaceHolder ID="phData2" runat="server"></asp:PlaceHolder>
<div id="simplisity_startpanel" class="simplisity_panel">
    <asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
</div>

<div class=" simplisity_loader " style="display: none;"><div class="simplisity_loader_inner"></div></div>
