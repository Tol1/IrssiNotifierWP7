<%@ page import="com.tol1.irssinotifier.server.IrssiNotifier"%>
<%@ page language="java" contentType="text/html; charset=UTF-8"
	pageEncoding="UTF-8"%>
<%@ page import="com.google.appengine.api.users.User"%>
<%@ page import="com.google.appengine.api.users.UserService"%>
<%@ page import="com.google.appengine.api.users.UserServiceFactory"%>
<%@ page import="com.tol1.irssinotifier.server.utils.ObjectifyDAO"%>
<%@ page
	import="com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser"%>
<%@ page
	import="com.tol1.irssinotifier.server.exceptions.UserNotFoundException"%>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fmt" uri="http://java.sun.com/jsp/jstl/fmt" %>
<%
Cookie[] cookies = request.getCookies();
Cookie languageCookie = null;
if(cookies != null){
	for(Cookie cookie : cookies){
		if(cookie.getName().equals("language")){
			languageCookie = cookie;
		}
	}
}
if(request.getParameter("language") != null){
	if(request.getParameter("language").toString().equalsIgnoreCase("en")){
		pageContext.setAttribute("lang", "en");
	}
	else{
		pageContext.setAttribute("lang", "fi");
	}
	languageCookie = new Cookie("language", pageContext.getAttribute("lang").toString());
}
else if(languageCookie != null){
	if(languageCookie.getValue().equalsIgnoreCase("en")){
		pageContext.setAttribute("lang", "en");
	}
	else{
		pageContext.setAttribute("lang", "fi");
	}
}
else{
	pageContext.setAttribute("lang", "fi");
	languageCookie = new Cookie("language", pageContext.getAttribute("lang").toString());
}
languageCookie.setMaxAge(60*60*24*360);
response.addCookie(languageCookie);
	%>
<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<fmt:setLocale value="${lang}" />
<fmt:bundle basename="site">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
<title>Irssi Notifier for Windows Phone</title>
<link href="/css/default.css" rel="stylesheet" type="text/css">
</head>
<body>
	<div class="root">
		<span class="lang">
			<c:set var="anotherLang"><fmt:message key="languageSelection"/></c:set>
			<fmt:message key="languageTitle">
				<fmt:param>
					<a href="?language=${anotherLang}"><fmt:message key="languageSelection"/></a>
				</fmt:param>
			</fmt:message>
		</span>
	<%
		UserService userService = UserServiceFactory.getUserService();
		User user = userService.getCurrentUser();
		ObjectifyDAO dao = new ObjectifyDAO();
		if (user == null) {
	%>
		<h1>Irssi Notifier for Windows Phone</h1>

		<p><fmt:message key="description1"/></p>
		<p><fmt:message key="description2"/></p>
		<p>
			<fmt:message key="description3">
				<fmt:param>
					<a href="http://irssinotifier.appspot.com">Irssi Notifier</a>
				</fmt:param>
			</fmt:message>
		</p>
		<p>
			<fmt:message key="loginDescription">
				<fmt:param>
					<a href="<%=userService.createLoginURL(request.getRequestURI())%>"><fmt:message key="loginText"/></a>
				</fmt:param>
			</fmt:message>
		</p>
	<%
		} else {
	%>
		<h1><fmt:message key="welcomeText"/></h1>
		<%
			IrssiNotifierUser inUser = null;
			try {
				inUser = IrssiNotifier.getUser(dao, user);
				%>
				<p>
					<fmt:message key="phoneRegistered">
						<fmt:param>
							<span class="emphasis"><%=inUser.UUID %></span>
						</fmt:param>
					</fmt:message>
				</p>
				<p>
					<fmt:message key="currentStatusTitle"/>
					<ul>
						<li>
							<fmt:message key="toastStatus">
								<fmt:param>
									<% if(inUser.sendToastNotifications){ %><fmt:message key="enabledText"/><%}else{ %><fmt:message key="disabledText"/><%} %>
								</fmt:param>
							</fmt:message>
						</li>
						<li>
							<fmt:message key="tileStatus">
								<fmt:param>
									<% if(inUser.sendTileNotifications){ %><fmt:message key="enabledText"/><%}else{ %><fmt:message key="disabledText"/><%} %>
								</fmt:param>
							</fmt:message>
						</li>
					</ul>
				<%
			} catch (UserNotFoundException unfe) {
	%>
		<p class="error"><fmt:message key="phoneNotRegistered"/></p>
		<%
			}
		%>
		<p class="header">
			<fmt:message key="instructionsTitle"/>
		</p>
		<span class="addition"><fmt:message key="upgradeInstructions"/></span>
		<ol>
			<li <%= inUser != null?"style=\"text-decoration: line-through;\"":"" %>><fmt:message key="instructions1"/></li>
			<li <%= inUser != null?"style=\"text-decoration: line-through;\"":"" %>><fmt:message key="instructions2"/></li>
			<li <%= inUser != null?"style=\"text-decoration: line-through;\"":"" %>><fmt:message key="instructions3"/></li>
		 
		<%	if(inUser != null) {%>
			<li><fmt:message key="instructions4"/>
				<code class="emphasis">
					mkdir -p ~/.irssi/scripts/autorun;
					wget https://irssinotifierwp.appspot.com/script/irssinotifierwp7.pl -O ~/.irssi/scripts/irssinotifierwp7.pl;
					ln -s ~/.irssi/scripts/irssinotifierwp7.pl ~/.irssi/scripts/autorun/irssinotifierwp7.pl;
				</code>
			 </li>
			 <li>
			 	<fmt:message key="instructions5">
			 		<fmt:param>
			 			<span class="emphasis">/script load irssinotifierwp7.pl</span>
			 		</fmt:param>
			 	</fmt:message>
			 </li>
			 <li>
			 	<fmt:message key="instructions6">
			 		<fmt:param>
			 			<span class="emphasis">/set irssinotifierwp_api_token <%=inUser.UUID %></span>
			 		</fmt:param>
			 	</fmt:message>
			 </li>
		<% } %>
		</ol>
		
		<p class="header">
			<fmt:message key="settingsTitle"/>
		</p>
		<ul>
			<li>/set irssinotifierwp_away_only [ON/OFF] - <fmt:message key="awaySettingDescription"/></li>
			<li>/set irssinotifierwp_ignore_active_window [ON/OFF] - <fmt:message key="activeSettingDescription"/></li>
			<li>/set irssinotifierwp_require_idle_seconds [num] - <fmt:message key="idleSettingDescription"/></li>
		</ul>
		<p>
			<a href="/script/irssinotifierwp7.pl"><fmt:message key="downloadScriptText"/></a>
		</p>
	<%
		}
	%>
	</div>
</body>
</fmt:bundle>
</html>