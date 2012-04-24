package com.tol1.irssinotifier.server;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.googlecode.objectify.NotFoundException;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.StatusMessages.ChannelStatusMessage;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

import flexjson.JSONSerializer;

@SuppressWarnings("serial")
public class UpdateChannelUrl extends HttpServlet {
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken");
		String guid = req.getParameter("guid");
		String newUrl = req.getParameter("newUrl");
		
		if(newUrl == null){
			IrssiNotifier.printError(resp.getWriter(), "Anna Url");
			return;
		}
		
		if(id == null || guid == null){
			IrssiNotifier.printError(resp.getWriter(), "Virheellinen pyyntö");
			return;
		}
		
		ObjectifyDAO dao = new ObjectifyDAO();
		try {
			IrssiNotifierUser user = dao.ofy().get(IrssiNotifierUser.class, id);
			if(user.guid.equals(guid)){
				if(user.ChannelURI.equalsIgnoreCase(newUrl)) {
					IrssiNotifier.log.info("Käyttäjän "+id+" client rekisteröitiin, notification channel uri pysyy muuttumattomana");
				} else {
					IrssiNotifier.log.info("Käyttäjän "+id+" client rekisteröitiin, notification channel uri vaihtuu arvosta "+user.ChannelURI+" arvoon "+newUrl);
				}
				user.ChannelURI = newUrl;
				dao.ofy().put(user);
				ChannelStatusMessage message = new ChannelStatusMessage(user.sendToastNotifications, user.sendTileNotifications);
				resp.getWriter().println(new JSONSerializer().exclude("class").serialize(message));
				resp.getWriter().close();
				return;
			}
			else{
				IrssiNotifier.printError(resp.getWriter(), "GUID ei täsmää");
			}
		} catch (NotFoundException e) {
			IrssiNotifier.printError(resp.getWriter(), e.getLocalizedMessage());
			return;
		}
	}
}
