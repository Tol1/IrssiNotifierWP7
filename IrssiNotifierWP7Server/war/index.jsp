<%@page import="com.tol1.irssinotifier.server.IrssiNotifier"%>
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
		ObjectifyDAO dao = new ObjectifyDAO();
		if (user == null) {
	%>
	<div class="root">
		<h1>Irssi Notifier for Windows Phone</h1>

		<p>Irssi Notifier on sovellus, jonka avulla on mahdollista saada
			irssin hilighteistä ja yksityisviesteistä notifikaatio suoraan
			Windows Phone -käyttöjärjestelmää käyttävään matkapuhelimeen. Palvelu
			käyttää WP7-käyttöjärjestelmän "Push notification"-palvelua, ja
			ilmoitus uudesta viestistä on saatavilla sekä ponnahdusviestinä että
			päivittyvän livetiilen kautta.</p>
		<p>Sovelluksen käyttöön vaaditaan Google-tunnukset palveluun
			kirjautumista ja puhelimen rekisteröintiä varten. Lisäksi
			vaatimuksena on luonnollisesti irssi. Irssiin tarvittava perl-skripti
			vaatii lisäksi palvelimelta wget-sovelluksen.
		</p>
		<p>
			Sovelluksen innoittajana toimi Lauri "murgo" Härsilän kehittämä <a
				href="http://irssinotifier.appspot.com">Irssi Notifier</a>-sovellus
			Android-käyttöjärjestelmää käyttäville laitteille.
		</p>
		<p>
			<a href="<%=userService.createLoginURL(request.getRequestURI())%>">Kirjaudu
				sisään</a> nähdäksesi lisätietoja ja ohjeita sovelluksen käyttämisestä.
		</p>
	</div>
	<%
		} else {
	%>
	<div class="root">
		<h1>Tervetuloa!</h1>
		<%
			IrssiNotifierUser inUser = null;
			try {
				inUser = IrssiNotifier.getUser(dao, user);
				%>
				<p>
					Puhelimesi on rekisteröity palveluun, ja sen yksilöllinen tunnus on <span class="emphasis"><%=inUser.UUID %></span>.
					Tätä tunnusta tarvitset määrittäessäsi asetuksia irssiskriptille.
				</p>
				<p>
					Tällä hetkellä toiminnassa ovat seuraavat palvelut:
					<ul>
						<li>Ponnahdusviestit: <%=inUser.sendToastNotifications?"Käytössä":"Pois käytöstä" %></li>
						<li>Tiilinotifikaatiot: <%=inUser.sendTileNotifications?"Käytössä":"Pois käytöstä" %></li>
					</ul>
				<%
			} catch (UserNotFoundException unfe) {
	%>
		<p class="error">Et ole vielä rekisteröinyt puhelintasi palvelun
			käyttäjäksi.</p>
		<%
			}
		%>
		<p>
			<a href="/script/irssinotifierwp7.pl">Lataa irssiskripti</a>
		</p>
		<p>
			Palvelun käyttöönottoohjeet:
		</p>
		<ol>
			<li <%= inUser != null?"style=\"text-decoration: line-through;\"":"" %>>Asenna IrssiNotifier-sovellus Windows Phone 7 -puhelimeesi.</li>
			<li <%= inUser != null?"style=\"text-decoration: line-through;\"":"" %>>Käynnistä sovellus. Ensimmäisellä käynnistyskerralla sovellus pyytää rekisteröitymään palveluun</li>
			<li <%= inUser != null?"style=\"text-decoration: line-through;\"":"" %>>Suoritettuasi rekisteröinnin päivitä tämä sivu nähdäksesi skriptin asennus- ja määrittelyohjeet</li>
		 
		<%	if(inUser != null) {%>
			<li>Asenna irssiskripti kirjoittamalla seuraavat komennot shelliin (ei siis irssiin vaan päätteelle, josta irssi käynnistetään)
				<code class="emphasis">
					mkdir -p ~/.irssi/scripts/autorun;
					wget https://irssinotifierwp.appspot.com/script/irssinotifierwp7.pl -O ~/.irssi/scripts/irssinotifierwp7.pl;
					ln -s ~/.irssi/scripts/irssinotifierwp7.pl ~/.irssi/scripts/autorun/irssinotifierwp7.pl;
				</code>
			 </li>
			 <li>Lataa skripti irssiin komennolla <span class="emphasis">/script load irssinotifierwp7.pl</span> (Tämä siis irssiin!)</li>
			 <li>Määritä API key komennolla <span class="emphasis">/set irssinotifierwp_api_token <%=inUser.UUID %></span> (Tämä myös irssiin)</li>
		<% } %>
		</ol>
		
		<p>
			Skriptin asetukset:
		</p>
		<ul>
			<li>/set irssinotifierwp_away_only [ON/OFF] - Kun päällä, notifikaatio lähetetään puhelimelle vain jos olet /away</li>
			<li>/set irssinotifierwp_ignore_active_window [ON/OFF] - Kun päällä, notifikaatioita ei lähetetä kanavalta, joka irssissä on auki.</li>
			<li>/set irssinotifierwp_require_idle_seconds [num] - Nollaa suurempi arvo määrittelee, montako sekuntia irssin pitää olla käyttämättömänä, ennen kuin notifikaatioita lähetetään.</li>
		</ul>
	</div>
	<%
		}
	%>
</body>
</html>