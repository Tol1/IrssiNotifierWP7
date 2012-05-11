<%@ page language="java" contentType="text/html; charset=UTF-8"
    pageEncoding="UTF-8"%>
<%@ page import="com.google.appengine.api.users.User" %>
<%@ page import="com.google.appengine.api.users.UserService" %>
<%@ page import="com.google.appengine.api.users.UserServiceFactory" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
<title>IrssiNotifier for Windows Phone</title>
<link href="/css/default.css" rel="stylesheet" type="text/css">
</head>
<body>
<%
		UserService userService = UserServiceFactory.getUserService();
		User user = userService.getCurrentUser();
		if(user == null){
%>
	<div class="root">
		<h1>Irssi Notifier for Windows Phone</h1>
		
		<p>
			Irssi Notifier on sovellus, jonka avulla on mahdollista saada irssin hilighteistä ja yksityisviesteistä notifikaatio suoraan Windows Phone -käyttöjärjestelmää käyttävään matkapuhelimeen.
			Palvelu käyttää WP7-käyttöjärjestelmän "Push notification"-palvelua, ja ilmoitus uudesta viestistä on saatavilla sekä ponnahdusviestinä että päivittyvän livetiilen kautta.
		</p>
		<p>
			Sovelluksen käyttöön vaaditaan Google-tunnukset palveluun kirjautumista ja puhelimen rekisteröintiä varten.
			Lisäksi vaatimuksena on luonnollisesti irssi. Irssiin tarvittava perl-skripti vaatii lisäksi palvelimelta wget-sovelluksen sekä openssl-kirjaston.
		</p>
		<p>
			Sovelluksen innoittajana toimi Lauri "murgo" Härsilän kehittämä <a href="http://irssinotifier.appspot.com">Irssi Notifier</a>-sovellus Android-käyttöjärjestelmää käyttäville laitteille.
		</p>
		<p>
			<a href="<%=userService.createLoginURL(request.getRequestURI()) %>">Kirjaudu sisään</a>
		</p>
	</div>
<%
		}
		else{
%>
	Tervetuloa <%= user.getNickname() %>!
<%
		}
%>
</body>
</html>