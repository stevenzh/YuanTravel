<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Mobile.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	400错误
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="JsCssContent" runat="server">
    <link href="../../Content/error.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="container noexist ovf mt10">
    <div class="box">
        <img src="../../images/404tm.jpg" width="246" height="208" class="fl" />
        <div class="fl content">
        	<h3 class="f16">非常抱歉，您打开的页面不存在</h3>	
            <ul>
            	<li>您可以：</li>
                <li class="cblue">1、请检查您输入的网址是否正确。<br />
2、如果您不能确认您输入的网址，请直接访问旅游分销首页<a href="http://yuan.sh-cct.cn" style="color:#549ccc">yuan.sh-cct.cn</a><br />
3、或致电：021-00000000 直接预订。</li>
            </ul>
        </div>	
    </div>    	
</div>
</asp:Content>


