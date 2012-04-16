package com.tol1.irssinotifier.server;

import java.io.IOException;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.googlecode.objectify.NotFoundException;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;

@SuppressWarnings("serial")
public class UpdateChannelUrl extends HttpServlet {
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		String id = req.getParameter("apiToken");
		String guid = req.getParameter("guid");
		String newUrl = req.getParameter("newUrl");
		
		if(newUrl == null){
			IrssiNotifier.printError(resp.getWriter(), "{ \"success\": false, \"message\": \"Missing url\" }");
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
				user.ChannelURI = newUrl;
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
		
		/*Entity databaseUser;
		try {
			databaseUser = IrssiNotifier.checkAuthentication(id, guid);
		} catch (Exception e1) {
			IrssiNotifier.printError(resp.getWriter(), e1.getLocalizedMessage());
			return;
		}
		
    	databaseUser.setProperty("ChannelURI", newUrl);
		IrssiNotifier.datastore.put(databaseUser);
		resp.getWriter().println("{ \"success\": true }");
		resp.getWriter().close();
		return;*/
	}
}
