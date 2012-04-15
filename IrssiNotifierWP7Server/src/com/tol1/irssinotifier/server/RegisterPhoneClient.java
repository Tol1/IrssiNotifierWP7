package com.tol1.irssinotifier.server;

import java.io.IOException;
import javax.servlet.ServletException;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.google.appengine.api.datastore.DatastoreService;
import com.google.appengine.api.datastore.DatastoreServiceFactory;
import com.google.appengine.api.datastore.Entity;
import com.google.appengine.api.datastore.EntityNotFoundException;
import com.google.appengine.api.datastore.Key;
import com.google.appengine.api.datastore.KeyFactory;
import com.google.appengine.api.users.User;
import com.google.appengine.api.users.UserService;
import com.google.appengine.api.users.UserServiceFactory;

@SuppressWarnings("serial")
public class RegisterPhoneClient extends HttpServlet {
	
	private static UserService userService = UserServiceFactory.getUserService();
	private static DatastoreService datastore = DatastoreServiceFactory.getDatastoreService();
	
	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp)
			throws ServletException, IOException {

		User user = userService.getCurrentUser();
        if (user != null) {
        	String guid = req.getParameter("guid");
			if(guid == null){
				resp.getWriter().println("{ \"error\": \"GUID missing\" }");
				resp.getWriter().close();
				return;
			}
        	String id = user.getUserId();
        	
        	Key userIdKey = KeyFactory.createKey("User", id);
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
			datastore.put(databaseUser);
			resp.getWriter().println("{ \"success\": \""+id+"\" }");
			
        } else {
            resp.sendRedirect("/client/login");
        }
	}
}
