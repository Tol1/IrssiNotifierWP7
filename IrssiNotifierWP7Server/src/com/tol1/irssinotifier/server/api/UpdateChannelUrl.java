package com.tol1.irssinotifier.server.api;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.tol1.irssinotifier.server.IrssiNotifier;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;
import com.tol1.irssinotifier.server.datamodels.StatusMessages.ChannelStatusMessage;
import com.tol1.irssinotifier.server.exceptions.*;
import com.tol1.irssinotifier.server.utils.ObjectifyDAO;

import flexjson.JSONSerializer;

@SuppressWarnings("serial")
public class UpdateChannelUrl extends HttpServlet {
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		resp.setCharacterEncoding("UTF-8");
		if(!IrssiNotifier.versionCheck(req.getParameter("version"))){
			IrssiNotifier.printError(resp.getWriter(), new OldVersionException());
			return;
		}
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
			IrssiNotifierUser user = IrssiNotifier.getUser(dao, id);
			
			if(user.guid.equals(guid)){
				if(user.ChannelURI != null && user.ChannelURI.equalsIgnoreCase(newUrl)) {
					IrssiNotifier.log.info("Käyttäjän "+id+" client rekisteröitiin, notification channel uri pysyy muuttumattomana");
				} else {
					IrssiNotifier.log.info("Käyttäjän "+id+" client rekisteröitiin, notification channel uri vaihtuu arvosta "+user.ChannelURI+" arvoon "+newUrl);
					user.ChannelURI = newUrl;
					dao.ofy().put(user);
				}
				ChannelStatusMessage message = new ChannelStatusMessage(user.sendToastNotifications, user.sendTileNotifications, user.errorOccurred);
				if(user.errorOccurred){
					user.errorOccurred = false;
					dao.ofy().put(user);
				}
				resp.getWriter().println(new JSONSerializer().exclude("class").serialize(message));
				resp.getWriter().close();
				return;
			}
			else{
				IrssiNotifier.printError(resp.getWriter(), new InvalidGUIDException());
			}
		} catch (UserNotFoundException e) {
			IrssiNotifier.printError(resp.getWriter(), e);
			return;
		}
	}
}
