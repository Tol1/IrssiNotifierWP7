package com.tol1.irssinotifier.server;

import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.URL;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.googlecode.objectify.NotFoundException;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.Message;

@SuppressWarnings("serial")
public class UpdateSettings extends HttpServlet {
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken");
		String guid = req.getParameter("guid");
		
		if(id == null && guid == null){
			IrssiNotifier.printError(resp.getWriter(), "Virheellinen pyyntö");
			return;
		}
		
		ObjectifyDAO dao = new ObjectifyDAO();
		try {
			IrssiNotifierUser user = dao.ofy().get(IrssiNotifierUser.class, id);
			if(user.guid.equals(guid)){
				String param;
				if((param = req.getParameter("toast")) != null){
					user.sendToastNotifications = Boolean.parseBoolean(param);
				}
				if((param = req.getParameter("tile")) != null){
					user.sendTileNotifications = Boolean.parseBoolean(param);
				}
				if((param = req.getParameter("clearcount")) != null){
					user.tileCount = 0;
					String clearMessage = Message.GenerateTileNotification(0);
					HttpURLConnection conn = MessageHandler.DoSend(clearMessage, "token", "1", new URL(user.ChannelURI));
					MessageHandler.HandleResponse(conn, resp, user, dao);
				}
				dao.ofy().put(user);
				resp.getWriter().println("{ \"success\": true }");
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
		/*
		Entity databaseUser;
		try {
			databaseUser = IrssiNotifier.checkAuthentication(id, guid);
		} catch (Exception e) {
			IrssiNotifier.printError(resp.getWriter(), e.getLocalizedMessage());
			return;
		}
		String param;
		if((param = req.getParameter("enable")) != null){
			databaseUser.setProperty("sendToastNotifications", Boolean.parseBoolean(param));
		}
		IrssiNotifier.datastore.put(databaseUser);
		resp.getWriter().println("{ \"success\": true }");
		resp.getWriter().close();
		return;*/
	}
}
