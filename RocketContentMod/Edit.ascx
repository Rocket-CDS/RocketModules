<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Edit.ascx.cs" Inherits="RocketContentMod.Edit" %>

<script type="text/javascript" src="/DesktopModules/DNNrocket/Simplisity/js/simplisity.js"></script>
<link rel="stylesheet" href="/DesktopModules/DNNrocket/css/w3.css">
<link rel="stylesheet" href="/DesktopModules/DNNrocket/Simplisity/css/simplisity.css">
<link rel="stylesheet" href="/DesktopModules/DNNrocket/API/Themes/config-w3/1.0/css/rocketcds-theme.css">
<link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:regular,bold,italic,thin,light,bolditalic,black,medium">
<link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">

<style>
    #editBarContainer { display: none !important }
    .personalBarContainer { display: none !important }
    #Body { margin-left: 0px !important }
    .material-icons { vertical-align: middle; }
    iframe.editBar-iframe{ display: none !important; }
</style>

<div id="simplisity_startpanel" class="simplisity_panel">
    <asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
</div>

