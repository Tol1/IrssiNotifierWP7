package com.tol1.irssinotifier.server;

import java.io.IOException;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.users.User;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;
import com.tol1.irssinotifier.server.datamodels.IrssiNotifierUser;

@SuppressWarnings("serial")
public class RegisterPhoneClient extends HttpServlet {
	
	private static UserService userService = UserServiceFactory.getUserService();
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {
		ObjectifyDAO dao = new ObjectifyDAO();
		User user = userService.getCurrentUser();
        if (user != null) {
        	String guid = req.getParameter("guid");
			if(guid == null){
				resp.getWriter().println("{ \"error\": \"GUID missing\" }");
				resp.getWriter().close();
				return;
			}
        	String id = user.getUserId();
        	
        	IrssiNotifierUser iNUser = new IrssiNotifierUser(id, guid);
        	
        	dao.ofy().put(iNUser);
        	
        	/*Key userIdKey = KeyFactory.createKey("User", id);
        	Entity databaseUser;
        	try {
				databaseUser = datastore.get(userIdKey);
			} catch (EntityNotFoundException e) {
				databaseUser = new Entity(userIdKey);
				databaseUser.setProperty("ChannelURI", null);
				databaseUser.setProperty("guid", null);
				databaseUser.setProperty("sendToastNotifications", false);
				databaseUser.setProperty("sendTileNotifications", false);
				datastore.put(databaseUser);
			}
        	
        	databaseUser.setProperty("ChannelURI", null);
        	databaseUser.setProperty("guid", guid);
			databaseUser.setProperty("sendToastNotifications", false);
			databaseUser.setProperty("sendTileNotifications", false);
			datastore.put(databaseUser);*/
			resp.getWriter().println("{ \"success\": \""+id+"\" }");
			
        } else {
            resp.sendRedirect("/client/login");
        }
	}
}
